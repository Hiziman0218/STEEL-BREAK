using UnityEngine;
using System;
using System.Collections.Generic;

using System.Reflection;

namespace StateMachineAI
{
    public enum AIState_Titania_T
    {
        Idle_T,
        Spawn_T,
        RandomMove_T,
        TurnBeam_T,
        LockBeam_T,
        RushBeam_T,
    }

    public class Titania_T
        : StatefulObjectBase<Titania_T, AIState_Titania_T>
    {
        [Header("プレイヤー")]
        public Transform m_Player;
        [Header("エネミーモデル")]
        public Transform m_EnemyModel;
        [Header("センターポイントの取得")]
        public GameObject m_CenterMarker;
        [Header("ビーム発射口")]
        public Transform m_BeamPoint;

        [Header("行動確率設定 0の行動はしない")]
        [Range(0f, 10f)] public float wSpawn = 2f;
        [Range(0f, 10f)] public float wRush = 2f;
        [Range(0f, 10f)] public float wBeam = 2f;
        [Range(0f, 10f)] public float wMove = 8f;

        [Header("雑魚敵のスポーン位置を取得")]
        public List<GameObject> m_SpawnPoints = new List<GameObject>();
        [Header("雑魚敵の守護位置を取得")]
        public GameObject m_Guard_Point;
        [Header("雑魚敵のプレハブ")]
        public GameObject m_Fairys;

        [Header("雑魚敵の場に残る上限数")]
        [Range(10, 20)]
        public int m_MaxFairys;
        [Header("生成できる守護雑魚の上限")]
        [Range(1, 5)]
        public int m_MaxDefensFairys = 5;
        [Header("役職確率（0.0でソルジャー100%、1.0でガーディアン100%）")]
        [Range(0.0f, 1.0f)]
        public float m_SpawnPer = 0.3f;
        [Header("生成するときの出現間隔")]
        public float m_waitSeconds = 0.5f;

        [Header("攻撃可能距離")]
        public float m_AttackDistance = 30;
        [Header("正面の攻撃可能角度[-1 = 完全に背後, 0 = 真横, 1 = 正面]")]
        public float m_forwardDotThreshold = 0.8f;

        [Header("突撃時の最大突進スピード")]
        [Range(10f, 40f)]
        public float m_maxspeed = 10f;
        [Header("加速度")]
        [Range(10f, 100f)]
        public float m_acceleration = 40f;
        [Header("追従補正（値が小さいほど緩く追従する）")]
        [Range(0.001f, 0.1f)]
        public float m_turnsmooth = 0.005f;

        [Header("攻撃型の敵管理")]
        public List<GameObject> m_spawnedAttackEnemies = new List<GameObject>();
        [Header("防御型の敵管理")]
        public List<GameObject> m_spawnedDefensEnemies = new List<GameObject>();
        [Header("全体の敵管理")]
        public List<GameObject> m_spawnedEnemies = new List<GameObject>();

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
        //コルーチンのフラグ管理用
        public bool isSpawningFairy = false;

        void Start()
        {
            //プレイヤーをタグで検索して取得
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //スポーンポイントを取得
            Transform parent = GameObject.Find("SpawnRoot").transform;
            foreach (Transform child in parent)
            {
                m_SpawnPoints.Add(child.gameObject);
            }

            //センターポインターを個別に取得する
            m_CenterMarker = PoolManager.Instance.Get("CenterPoint", transform.position + transform.forward, m_Player);

            //agent生成
            myAgent = PoolManager.Instance.Get("Titania", transform.position + transform.forward, m_CenterMarker.transform);

            //アタッチしているスプリクトの自動取得
            AutoComponentInitializer.InitializeComponents(this);
            m_Rigidbody = GetComponent<Rigidbody>();

            //存在していないクラスが指定されたら本体消滅
            foreach (AIState_Titania_T state in Enum.GetValues(typeof(AIState_Titania_T)))
            {
                if (!AddStateByName(state.ToString()))
                {
                    Debug.LogError($"{state} の追加に失敗したため、本体を削除します。");
                    Destroy(gameObject);
                    return;
                }
            }

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<Titania_T>();

            //初期起動時は、行動決め状態に移行させる
            ChangeState(AIState_Titania_T.Idle_T);
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

                // 型が State<Titania_T> かどうかをチェック
                if (!typeof(State<Titania_T>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(Titania_T) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<Titania_T> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<Titania_T>;

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
