using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine.AI;
//using Unity.VisualScripting;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEditorInternal;
using RaycastPro.Detectors;
using Plugins.RaycastPro.Demo.Scripts;
using static UnityEngine.GraphicsBuffer;

namespace StateMachineAI
{
    /// <summary>
    /// 生成時に割り振られる役割
    /// Guardianはティターニアを守る
    /// Soldierは攻撃部隊
    /// </summary>
    public enum EnemyRole
    {
        Guardian,
        Soldier
    }

    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くできない。
    /// </summary>
    /// 
    public enum AIState_Fairy
    {
        Chase_Fairy,
        Shot,
        RandamMove,
        Guard,
        CeackGuard,
    }

    public class FairyAI
        : StatefulObjectBase<FairyAI, AIState_Fairy>
    {
        [Header("プレイヤー")]
        public Transform m_Player;
        [Header("エネミーモデル")]
        public Transform m_EnemyModel;
        [Header("センターポイントの取得")]
        public GameObject m_CenterMarker;

        [Header("半径")]
        public float m_Radius = 8f;
        [Header("回転速度")]
        public float m_RotSpeed = 2f;
        [Header("上下幅の揺れ")]
        public float m_Vertical = 0.3f;
        [Header("x軸のねじれ")]
        public float m_Twist_x = 0.5f;

        [Header("攻撃可能距離")]
        public float m_AttackDistance = 10;
        [Header("正面の攻撃可能角度[-1 = 完全に背後, 0 = 真横, 1 = 正面]")]
        public float m_forwardDotThreshold = 0.8f;

        [Header("突撃時の最大突進スピード")]
        [Range(10f, 40f)]
        public float m_maxspeed = 10f;

        [HideInInspector]
        public CoolDown m_CoolDown;
        [HideInInspector]
        //クールタイム設定用
        public float m_CoolTime;
        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        // 自分専用ユニット
        public GameObject myAgent;
        [HideInInspector]
        //各役職用変数
        public EnemyRole m_Role;
        [HideInInspector]
        //自分が守るポイント取得用
        public GameObject m_GuardPointer;
        [HideInInspector]
        //守護位置のリスト
        public List<Transform> m_GuardPoint;


        public GameObject FlyingAgentObject;
        public SteeringDetector m_Detector;

        public GameObject m_MoveTarget;

        void Start()
        {
            m_MoveTarget = new GameObject();

            //プレイヤーをタグで検索して取得
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //アタッチしているスプリクトの自動取得
            AutoComponentInitializer.InitializeComponents(this);
            m_Rigidbody = GetComponent<Rigidbody>();

            //ガードポイントを取得
            Transform parent = GameObject.Find("GuardPoint").transform;
            foreach (Transform child in parent)
            {
                m_GuardPoint.Add(child);
            }

            //センターポインターを個別に取得する
            m_CenterMarker = PoolManager.Instance.Get("CenterPoint", transform.position + transform.forward, m_Player);

            //エージェントを取得
            myAgent = PoolManager.Instance.Get("Soldier", transform.position + transform.forward, m_Player);

            //存在していないクラスが指定されたら本体消滅
            if (!AddStateByName("Chase_Fairy"))
                Destroy(gameObject);
            if (!AddStateByName("Shot"))
                Destroy(gameObject);
            if (!AddStateByName("RandamMove"))
                Destroy(gameObject);
            if (!AddStateByName("Guard"))
                Destroy(gameObject);
            if (!AddStateByName("CeackGuard"))
                Destroy(gameObject);

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<FairyAI>();

            GameObject Dummy = GameObject.Instantiate(FlyingAgentObject, transform.position, transform.rotation);
            m_Detector = Dummy.GetComponent<SteeringDetector>();
            m_Detector.destination = m_Player;

            // myAgent = PoolManager.Instance.Get("Soldier", transform.position + transform.forward, m_Player);
            // 攻撃しに行く
            ChangeState(AIState_Fairy.Chase_Fairy);
        }
        /*
        // 消滅時に色々解除  死亡ステートが作れたらそっちに移動したほうがいいかも
        void OnDestroy()
        {
            // ロールごとのagent解除
            if (m_Role == EnemyRole.Guardian)
            {
                PoolManager.Instance.Return("Guardian", myAgent);
            }
            else
            {
                PoolManager.Instance.Return("Soldier", myAgent);
            }

            // 敵管理リストから除外
            Titania titania = FindObjectOfType<Titania>();
            if (titania != null)
            {
                // 登録解除
                //titania.m_spawnedEnemies.Remove(gameObject);
            }

        }*/


        /// <summary>
        /// クラス名を元にステートを生成して追加する
        /// </summary>
        /// <param name="ClassName">生成するクラスの名前</param>
        public bool AddStateByName(string ClassName)
        {
            try
            {
                // 現在のアセンブリからクラスを取得
                Type StateType = Assembly.GetExecutingAssembly().GetType($"StateMachineAI.{ClassName}");

                // クラスが見つからなかった場合の対処
                if (StateType == null)
                {
                    Debug.LogError($"{ClassName} クラスが見つかりませんでした。");
                    return true;
                }

                // 型が State<GunBattery_AI> かどうかをチェック
                if (!typeof(State<FairyAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(FairyAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<FairyAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<FairyAI>;

                if (StateInstance != null)
                {
                    // ステートリストに追加
                    stateList.Add(StateInstance);
                    Debug.Log($"{ClassName} をステートリストに追加しました。");
                    return true;
                }
                else
                {
                    Debug.LogError($"{ClassName} のインスタンス生成に失敗しました。");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"エラーが発生しました。: {ex.Message}");
                return false;
            }
        }

    }
}
