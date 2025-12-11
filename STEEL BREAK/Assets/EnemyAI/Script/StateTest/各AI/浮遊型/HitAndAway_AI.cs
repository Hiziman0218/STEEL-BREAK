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
    public enum AIState_HitAndAwayAI
    {
        Chase,
        Attack,
        Away,
        Return,
        Hit,
    }

    public class HitAndAwayAI
        : StatefulObjectBase<HitAndAwayAI, AIState_HitAndAwayAI>
    {
        [Header("プレイヤー")]
        public Transform m_Player;
        [Header("エネミーモデル")]
        public Transform m_EnemyModel;
        [Header("センターポイントの取得")]
        public GameObject m_CenterMarker;

        [Header("移動速度")]
        [Range(10f, 50f)]
        public float m_speed = 30f;
        [Header("旋回強度")]
        [Tooltip("大きくすれば急旋回、小さくすれば大きな弧を描く旋回\n" +
            "低速旋回（ゆったり大きな弧）30〜60\n" +
            "中速旋回（自然なターン）90〜180\n" +
            "高速旋回（急激な方向転換）270～360"
         )]
        [Range(0f,360f)]
        public float m_RotationSpeed = 90;
        [Header("旋回開始距離")]
        [Range(10,150)]
        public float m_RotationStart = 40;

        [Header("再突入条件の角度")]
        [Range(0f, 90f)]
        public float m_ReEntryAngle = 30f;
        [Header("再突入条件の時間")]
        [Range(10f, 100f)]
        public float m_AwayDuration = 20f;

        [Header("攻撃可能距離")]
        public float m_AttackDistance = 40;
        [Header("クールタイム")]
        [Range(1f,30f)]
        public float m_CoolTime = 4;
        [Header("正面の攻撃可能角度[-1 = 完全に背後, 0 = 真横, 1 = 正面]")]
        public float m_forwardDotThreshold = 0.8f;

        [Header("突撃時の最大突進スピード")]
        [Range(10f, 40f)]
        public float m_maxspeed = 20f;
        [Header("加速度")]
        [Range(10f, 100f)]
        public float m_acceleration = 40f;
        [Header("追従補正（値が小さいほど緩く追従する）")]
        [Range(0.001f, 0.1f)]
        public float m_turnsmooth = 0.005f;

        [HideInInspector]
        public CoolDown m_CoolDown;
        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        // 自分専用ユニット
        public GameObject myAgent;
        [HideInInspector]
        //現在スピード
        public float m_currentspeed = 0;
        [HideInInspector]
        private CharaBase charaBase;
        [HideInInspector]
        //エージェントのコントローラー取得用
        public SteeringController m_RCController;
        [HideInInspector]
        public Enemy m_Enemy;

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
            //エージェントのコンポーネント取得
            m_RCController = myAgent.GetComponent<SteeringController>();
            m_RCController.speed = m_speed;

            //エネミーのスクリプトを取得
            Enemy m_Enemy = GetComponent<Enemy>();

            //イベント登録
            if (m_Enemy != null)
            {
                // ダメージイベントを購読
                 m_Enemy.OnStagger+= HandleDamaged;
            }

            //アタッチしているスプリクトの自動取得
            AutoComponentInitializer.InitializeComponents(this);
            m_Rigidbody = GetComponent<Rigidbody>();

            //存在していないクラスが指定されたら本体消滅
            if (!AddStateByName("Chase"))
                Destroy(gameObject);
            if (!AddStateByName("Attack"))
                Destroy(gameObject);
            if (!AddStateByName("Away"))
                Destroy(gameObject);
            if (!AddStateByName("Return"))
                Destroy(gameObject);
            if (!AddStateByName("Hit_HitAndAwayAI"))
                Destroy(gameObject);

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<HitAndAwayAI>();

            //初期起動時は、プレイヤーを追いかける状態に移行させる
            ChangeState(AIState_HitAndAwayAI.Chase);
        }

        private void HandleDamaged(Enemy enemy)
        {
            ChangeState(AIState_HitAndAwayAI.Hit);
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

                // 型が State<GunBattery_AI> かどうかをチェック
                if (!typeof(State<HitAndAwayAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(HitAndAwayAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<HitAndAwayAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<HitAndAwayAI>;

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
