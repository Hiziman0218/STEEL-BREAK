using UnityEngine;
using System;
using System.Reflection;

namespace StateMachineAI
{
    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くできない。
    /// </summary>
    /// 
    public enum AIState_GunBatteryAI
    {
        Caution,
        Attack,
        Hit,
    }

    public class GunBatteryAI
        : StatefulObjectBase<GunBatteryAI, AIState_GunBatteryAI>
    {
        [Header("プレイヤー")]
        public Transform m_Player;
        [Header("砲身モデル")]
        public Transform[] m_Muzzles;

        [Header("砲身の仰角制限（縦方向の制限）")]
        [Range(-10f, 0f)]
        public float minPitchAngle = -5f;
        [Range(0f, 80f)]
        public float maxPitchAngle = 60f;

        [Header("砲台の横回転のラグタイム")]
        [Range(1f, 10f)]
        public float m_rotationSpeedH;
        [Header("砲身の縦回転のラグタイム")]
        [Range(1f, 10f)]
        public float m_rotationSpeedV;

        [Header("攻撃可能距離")]
        public float m_AttackDistance = 30f;

        [HideInInspector]   
        //回転ポイント（自動取得）
        public Transform m_RotPoint;
        [HideInInspector]
        public CoolDown m_CoolDown;
        [HideInInspector]
        private CharaBase charaBase;
        [HideInInspector]
        public Enemy m_Enemy;

        void Start()
        {
            //プレイヤーをタグで検索して取得
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //キャラベース取得
            charaBase = GetComponent<CharaBase>();
            //キャラベースがあればイベント登録
            if (charaBase != null)
            {
                // ダメージイベントを購読
                charaBase.OnDamage += HandleDamaged;
            }

            //アタッチしているスプリクトの自動取得
            AutoComponentInitializer.InitializeComponents(this);

            //コライダーを取得
            Collider[] myColliders = GetComponents<Collider>();

            // 砲身の親を自動で取得
            if (m_Muzzles != null && m_Muzzles.Length > 0)
            {
                m_RotPoint = m_Muzzles[0].parent;
            }

            //存在していないクラスが指定されたら本体消滅
            if (!AddStateByName("Caution"))
                Destroy(gameObject);
            if (!AddStateByName("Attack_GunBatteryAI"))
                Destroy(gameObject);
            if (!AddStateByName("Hit_GunBatteryAI"))
                Destroy(gameObject);

            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<GunBatteryAI>();

            //初期起動時は、プレイヤーを追いかける状態に移行させる
            ChangeState(AIState_GunBatteryAI.Caution);
        }

        private void HandleDamaged()
        {
            ChangeState(AIState_GunBatteryAI.Hit);
        }

        protected override void Update()
        {
            // プレイヤーがいないときの共通処理
            if (m_Player == null)
            {
                return;
            }

            // 親クラスの Update を呼んでステートマシンを動かす
            base.Update();
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
                if (!typeof(State<GunBatteryAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(GunBatteryAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。");
                    return true;
                }

                State<GunBatteryAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<GunBatteryAI>;

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
