using UnityEngine;
using System;
using System.Reflection;
using Plugins.RaycastPro.Demo.Scripts;

namespace StateMachineAI
{
    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くできない。
    /// </summary>
    /// 
    public enum AIState_CenteringAI
    {
        Chase,
        CenterPoint,
        Attack,
        Hit,
    }

    public class CenteringAI
        : StatefulObjectBase<CenteringAI, AIState_CenteringAI>
    {
        [Header("プレイヤー(タグで取得)")]
        public Transform m_Player;
        [Header("エネミーモデル")]
        public Transform m_EnemyModel;
        [Header("センターポイントの取得")]//(一回のみ格納の方が効率的)
        public GameObject m_CenterMarker;
        [Header("向き補正")]
        public float m_Muki = 1;
        [Header("側面の攻撃可能角度[-1 = 完全に背後, 0 = 真横, 1 = 正面]")]
        public float m_SideDotThreshold = 0.3f;
        [Header("攻撃可能距離")]
        [Range(1f, 20f)]
        public float m_AttackDistance = 10f;
        [Header("最大連射数")]
        [Range(1f, 30f)]
        public int m_MaxRange = 5;
        [Header("攻撃クールタイム")]
        [Range(1f, 30f)]
        public int m_CoolTime = 4;

        public Enemy m_Enemy;
        [HideInInspector]
        public CoolDown m_CoolDown;
        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        public BoxCollider m_BoxCollider;
        [HideInInspector]
        // 自分専用ユニット
        public GameObject myAgent;
        public SteeringController m_Controller;

        void Start()
        {
            //プレイヤーをタグで検索して取得
            m_Player = GameObject.FindWithTag("Player")?.transform;
            //自分のモデルを取る
            m_EnemyModel = this.transform;

            //センターポインターを個別に取得する
            m_CenterMarker = PoolManager.Instance.Get("CenterPoint", transform.position + transform.forward, m_Player);

            //agent生成
            myAgent = PoolManager.Instance.Get("FlyingFollowing", transform.position, m_Player);
            //エージェントのコントローラー取得
            m_Controller = myAgent.GetComponent<SteeringController>();

            //エネミーのスクリプトを取得
            Enemy m_Enemy = GetComponent<Enemy>();
            //エネミーがあればその中のキャラベースを参照してイベント登録
            if (m_Enemy != null)
            {
                // ダメージイベントを購読
                m_Enemy.OnStagger += HandleDamaged;
            }

            //アタッチしているスプリクトの自動取得
            AutoComponentInitializer.InitializeComponents(this);

            m_Rigidbody = GetComponent<Rigidbody>();

            //存在していないクラスが指定されたら本体消滅
            if (!AddStateByName("Chase_CenteringAI"))
                Destroy(gameObject);
            if (!AddStateByName("CenterPoint"))
                Destroy(gameObject);
            if (!AddStateByName("Attack_CenteringAI"))
                Destroy(gameObject);
            if (!AddStateByName("Hit_CenteringAI"))
                Destroy(gameObject);

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<CenteringAI>();

            //初期起動時は、プレイヤーを追いかける状態に移行させる
            ChangeState(AIState_CenteringAI.Chase);
        }

        private void HandleDamaged(Enemy enemy)
        {
            ChangeState(AIState_CenteringAI.Hit);
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

                // 型が State<CenteringAI> かどうかをチェック
                if (!typeof(State<CenteringAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(CenteringAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<CenteringAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<CenteringAI>;

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
