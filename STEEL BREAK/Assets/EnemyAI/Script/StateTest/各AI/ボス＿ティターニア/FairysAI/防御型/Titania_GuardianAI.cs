using UnityEngine;
using System;
using System.Collections.Generic;

using System.Reflection;
using RaycastPro.Detectors;

namespace StateMachineAI
{

    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くできない。
    /// </summary>
    public enum AIState_Guardian
    {
        Chase,
        Shot,
        RandamMove,
        Guard,
        CeackGuard,
        Hit
    }

    public class GuardianFairysAI
        : StatefulObjectBase<GuardianFairysAI, AIState_Guardian>
    {
        [Header("プレイヤー")]
        public Transform m_Player;
        [Header("エネミーモデル")]
        public Transform m_EnemyModel;
        [Header("射撃するためのエネミーコンポーネント")]
        public Enemy m_Enemy;
        [Header("センターポイントの取得")]
        public GameObject m_CenterMarker;

        [Header("攻撃可能距離")]
        public float m_AttackDistance = 20;
        [Header("守護位置から離れても追い続ける距離")]
        public float m_ReturnDistance = 50;
        [Header("最大連射数")]
        [Range(1f, 30f)]
        public int m_MaxRange = 8;
        [Header("攻撃のクールタイム")]
        [Range(1f, 10f)]
        public float m_CoolTime = 4f;

        [HideInInspector]
        public CoolDown m_CoolDown;
        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        // 自分専用ユニット
        public GameObject myAgent;
        [HideInInspector]
        //エージェントのディテクター
        public SteeringDetector m_Detector;
        //自分が守るポイント取得用
        public GameObject m_GuardPointer;
        [HideInInspector]
        //守護位置のリスト
        public List<Transform> m_GuardPoint;
        [HideInInspector]
        //守護位置に帰る処理流すかどうかのフラグ
        public bool m_atk_flag = false;

        void Start()
        {
            //プレイヤーをタグで検索して取得
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //自分のtransformを取得
            m_EnemyModel = transform;

            //アタッチしているスプリクトの自動取得
            AutoComponentInitializer.InitializeComponents(this);
            m_Rigidbody = GetComponent<Rigidbody>();

            //ガードポイントを取得
            Transform parent = GameObject.Find("GuardPoint").transform;
            foreach (Transform child in parent)
            {
                m_GuardPoint.Add(child);
            }

            //エネミーのスクリプトを取得
            m_Enemy = GetComponent<Enemy>();
            //キャラベースがあればイベント登録
            if (m_Enemy != null)
            {
                // ダメージイベントを購読
                m_Enemy.OnDamage += HandleDamaged;
            }

            //センターポインターを個別に取得する
            m_CenterMarker = PoolManager.Instance.Get("CenterPoint", transform.position + transform.forward, m_Player);

            //エージェントを取得
            myAgent = PoolManager.Instance.Get("Guardian", transform.position + transform.forward, m_Player);
            //エージェントのSteeringDetectorを取得
            m_Detector = myAgent.GetComponent<SteeringDetector>();

            //自動でクラス名を探して取得
            foreach (AIState_Guardian state in Enum.GetValues(typeof(AIState_Guardian)))
            {
                // enum名からクラス名を組み立て
                string className = $"{state}_Guardian";
                //存在していないクラスが指定されたら本体消滅
                if (!AddStateByName(className))
                {
                    Debug.LogError("ステートの取得ができませんでした" + className);
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    Debug.Log("クラス：" + className + "を取得");
                }
            }

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<GuardianFairysAI>();

            // 守護位置を見つける
            ChangeState(AIState_Guardian.CeackGuard);
        }

        override protected void Update()
        {
            base.Update();

            //攻撃可能かのチェック
            (float distance, _, _) = Distance_Check.Check(transform, m_Player);
            //守護位置と自分の距離
            (float guarddistance, _, _) = Distance_Check.Check(m_GuardPointer.transform, transform);

            if (m_atk_flag == true)
            {
                //もしm_GuardPointerから一定距離離れるor攻撃範囲内からプレイヤーが外れたら
                if (guarddistance > m_ReturnDistance && distance > m_AttackDistance)
                {
                    m_atk_flag = false;
                    //守護位置に戻っていく
                    ChangeState(AIState_Guardian.Guard);
                }
            }

        }

        private void HandleDamaged()
        {
            ChangeState(AIState_Guardian.Hit);
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

                // 型が State<GyardianFairysAI> かどうかをチェック
                if (!typeof(State<GuardianFairysAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(GuardianFairysAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<GuardianFairysAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<GuardianFairysAI>;

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
