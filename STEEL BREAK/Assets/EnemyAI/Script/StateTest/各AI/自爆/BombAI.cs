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
    public enum AIState_BombAI
    {
        Chase,
        Ramming,
        Hit,
        Explosion,
    }

    public class BombAI
        : StatefulObjectBase<BombAI, AIState_BombAI>
    {
        [Header("プレイヤー")]
        public Transform m_Player;
        [Header("攻撃判定を持った爆発エフェクト")]
        public GameObject blastPrefab;

        [Header("攻撃可能角度[-1 = 完全に背後, 0 = 真横, 1 = 正面]")]
        public float m_SideDotThreshold = 0.7f;
        [Header("自爆開始距離")]
        [Range(15f, 50f)]
        public float m_AttackDistance = 30;
        [Header("移動速度")]
        [Range(10f, 50f)]
        public float m_speed = 20f;
        [Header("自爆前の最大突進スピード")]
        [Range(10f, 200f)]
        public float m_maxspeed = 100f;
        [Header("自爆までの猶予")]
        [Range(1f, 10f)]
        public float m_explosion_count = 3f;
        [Header("加速度")]
        [Range(10f, 50f)]
        public float m_acceleration = 20f;
        [Header("追従補正（値が小さいほど緩く追従する）")]
        [Range(0.001f, 0.005f)]
        public float m_turnsmooth = 0.001f;

        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        public CapsuleCollider m_CapsuleCollider;
        [HideInInspector]
        // 自分専用ユニット
        public GameObject myAgent;
        [HideInInspector]
        // 現在速度を保持
        public float m_currentspeed = 0f;
        [HideInInspector]
        //エージェントのコントローラー取得用
        public SteeringController m_RCController;
        [HideInInspector]
        public CoolDown m_CoolDown;
        [HideInInspector]
        private CharaBase charaBase;
        [HideInInspector]
        public Enemy m_Enemy;

        void OnTriggerEnter(Collider other)
        {
            // プレイヤーや壁など、何かに当たったら自爆ステートに遷移
            ChangeState(AIState_BombAI.Explosion);
        }

        void Start()
        {
            //プレイヤーをタグで検索して取得
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //アタッチしているスプリクトの自動取得
            AutoComponentInitializer.InitializeComponents(this);
            //エージェント取得
            myAgent = PoolManager.Instance.Get("FlyingFollowing", transform.position, m_Player);
            //エージェントのコンポーネント取得
            m_RCController = myAgent.GetComponent<SteeringController>();
            m_RCController.speed = m_speed;

            //キャラベース取得
            charaBase = GetComponent<CharaBase>();
            //キャラベースがあればイベント登録
            if (charaBase != null)
            {
                // ダメージイベントを購読
                charaBase.OnDamage += HandleDamaged;
            }

            m_Rigidbody = GetComponent<Rigidbody>();
            m_CapsuleCollider = GetComponent<CapsuleCollider>();

            //存在していないクラスが指定されたら本体消滅
            if (!AddStateByName("Chase_BombAI"))
                Destroy(gameObject);
            if (!AddStateByName("Ramming_BombAI"))
                Destroy(gameObject);
            if (!AddStateByName("Hit_BombAI"))
                Destroy(gameObject);
            if (!AddStateByName("Explosion"))
                Destroy(gameObject);

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<BombAI>();

            //初期起動時は、プレイヤーを追いかける状態に移行させる
            ChangeState(AIState_BombAI.Chase);
        }

        private void HandleDamaged()
        {
            // 現在のステートが Ramming_BombAI 型なら無視
            if (stateMachine.CurrentState is Ramming_BombAI)
            {
                Debug.Log("Ramming中なのでHitに遷移しない");
                return;
            }

            ChangeState(AIState_BombAI.Hit);
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
                if (!typeof(State<BombAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(BombAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<BombAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<BombAI>;

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
