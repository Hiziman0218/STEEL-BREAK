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
    public enum FairysRole
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
    public enum AIState_Fairys
    {
        Chase_Fairys,
        Shot_Fairys,
        RandamMove_Fairys,
        Guard_Fairys,
        CeackGuard_Fairys,
    }

    public class FairysAI
        : StatefulObjectBase<FairysAI, AIState_Fairys>
    {
        [Header("プレイヤー")]
        public Transform m_Player;
        [Header("エネミーモデル")]
        public Transform m_EnemyModel;
        [Header("センターポイントの取得")]
        public GameObject m_CenterMarker;

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

        void Start()
        {
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
            if (!AddStateByName("Chase_Fairys"))
                Destroy(gameObject);
            if (!AddStateByName("Shot_Fairys"))
                Destroy(gameObject);
            if (!AddStateByName("RandamMove_Fairys"))
                Destroy(gameObject);
            if (!AddStateByName("Guard_Fairys"))
                Destroy(gameObject);
            if (!AddStateByName("CeackGuard_Fairys"))
                Destroy(gameObject);

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<FairysAI>();
            
            // 追いかける
            ChangeState(AIState_Fairys.Chase_Fairys);
        }

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
                if (!typeof(State<FairysAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(FairysAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<FairysAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<FairysAI>;

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
