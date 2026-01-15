using UnityEngine;
using System;
using System.Reflection;
using RaycastPro.Detectors;
using Plugins.RaycastPro.Demo.Scripts;

namespace StateMachineAI
{

    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くできない。
    /// </summary>
    public enum AIState_Ase
    {
        Chase,
        Shot,
        RandamMove,
        Hit,
    }

    public class AseAI
        : StatefulObjectBase<AseAI, AIState_Ase>
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
        public float m_AttackDistance = 10;
        [Header("攻撃のクールタイム")]
        [Range(1f, 10f)]
        public float m_CoolTime = 4f;
        [Header("移動速度")]
        [Range(2f, 50f)]
        public float m_MoveSpeed = 12f;
        [Header("攻撃後の停止時間")]
        public float m_TImes = 3.0f;

        [HideInInspector]
        public CoolDown m_CoolDown;
        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        // 自分専用ユニット
        public GameObject myAgent;
        [HideInInspector]
        //エージェントのディテクター
        public Detector m_Detector;
        [HideInInspector]
        public SteeringController m_Controller;


        void Start()
        {
            //プレイヤーをタグで検索して取得
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //自分のモデル位置を獲得
            m_EnemyModel = this.transform;

            //アタッチしているスプリクトの自動取得
            AutoComponentInitializer.InitializeComponents(this);
            m_Rigidbody = GetComponent<Rigidbody>();

            //センターポインターを個別に取得する
            m_CenterMarker = PoolManager.Instance.Get("CenterPoint", transform.position + transform.forward, m_Player);

            //エージェントを取得
            myAgent = PoolManager.Instance.Get("FlyingFollowing", transform.position, m_Player);
            m_Detector = myAgent.GetComponent<Detector>();
            m_Controller = myAgent.GetComponent<SteeringController>();

            // 初期速度を変更する
            m_Controller.speed = m_MoveSpeed;

            //エネミーのスクリプトを取得
            Enemy m_Enemy = GetComponent<Enemy>();
            //キャラベースがあればイベント登録
            if (m_Enemy != null)
            {
                // ダメージイベントを購読
                m_Enemy.OnStagger += HandleDamaged;
            }


            //存在していないクラスが指定されたら本体消滅
            foreach (AIState_Ase state in Enum.GetValues(typeof(AIState_Ase)))
            {
                string className = $"{state}_Ase"; // enum名からクラス名を組み立て
                if (!AddStateByName(className))
                {
                    Debug.LogError("ステートの取得ができませんでした");
                    Destroy(gameObject);
                    return;
                }
            }

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<AseAI>();
            
            // 追いかける
            ChangeState(AIState_Ase.Chase);
        }

        protected override void Update()
        {
            // 親クラスの Update を呼んでステートマシンを動かす
            base.Update();

            // プレイヤーがいないときの共通処理
            if (m_Player == null)
            {
                return;
            }

        }

        private void HandleDamaged(Enemy enemy)
        {
            ChangeState(AIState_Ase.Hit);
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

                // 型が State<AseAI> かどうかをチェック
                if (!typeof(State<AseAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<AseAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(AseAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<AseAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<AseAI>;

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
