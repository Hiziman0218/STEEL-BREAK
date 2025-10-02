using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [RequireComponent(typeof(TargetPositionModifier))]
    [RequireComponent(typeof(FactionExtension))]
    public class EmeraldPlayerBridge : MonoBehaviour, IDamageable, ICombat
    {
        [field: Header("開始時の体力（初期HP）")]
        public int StartHealth { get; set; } = 100;

        [field: Header("現在の体力（実行時に同期）")]
        public int Health { get; set; } = 100;

        [Header("不死（ダメージを受けても死亡しない）")]
        [HideInInspector] public bool Immortal = false;

        [Space(5)]
        [Header("ダメージを受けたときに発火するイベント")]
        public UnityEvent OnTakeDamage;

        [Header("死亡時に発火するイベント")]
        public UnityEvent OnDeath;

        [field: Header("アクティブな効果一覧（文字列）")]
        public List<string> ActiveEffects { get; set; } = new List<string>();

        [Header("照準位置補正用の参照（TargetPositionModifier）")]
        TargetPositionModifier m_TargetPositionModifier;

        [Header("自身のコライダー参照")]
        Collider m_Collider;

        [Header("直近の攻撃がクリティカルか（内部用）")]
        bool m_CriticalHit;

        public virtual void Awake()
        {
            m_TargetPositionModifier = GetComponent<TargetPositionModifier>();
            m_Collider = GetComponent<Collider>();
        }

        public virtual void Start()
        {
            Health = StartHealth;

            // ここで、キャラクターコントローラ側の体力値に合わせて
            // StartHealth と Health を同期させてください。
        }

        /// <summary>
        /// IDamageable インターフェイス経由で内部的に呼び出されます。
        /// </summary>
        public void Damage(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 100, bool CriticalHit = false)
        {
            m_CriticalHit = CriticalHit;
            DamageCharacterController(DamageAmount, AttackerTransform);
        }

        void OnEnable()
        {
            if (Health <= 0) ResetTarget();
        }

        /// <summary>
        /// 渡されたダメージ量に基づいてコンバットテキストを表示します。
        /// ブロックや回避などを自前で判定して使いたい場合に備え、別関数として分離しています。
        /// </summary>
        public virtual void DisplayDamageText(int DamageAmount)
        {
            // 有効な場合、ターゲット位置にコンバットテキスト（与ダメージ）を生成します。
            if (CombatTextSystem.Instance != null) CombatTextSystem.Instance.CreateCombatText(DamageAmount, DamagePosition(), m_CriticalHit, false, false);
        }

        /// <summary>
        /// 外部ソースからAIがダメージを受けた際に、このオブジェクトのダメージ位置を参照するために使用します。
        /// </summary>
        public Vector3 DamagePosition()
        {
            if (m_TargetPositionModifier != null)
                return new Vector3(m_TargetPositionModifier.TransformSource.position.x, m_TargetPositionModifier.TransformSource.position.y + m_TargetPositionModifier.PositionModifier, m_TargetPositionModifier.TransformSource.position.z);
            else
                return transform.position + new Vector3(0, transform.localScale.y / 2, 0);
        }

        public virtual void DamageCharacterController(int DamageAmount, Transform Target)
        {
            if (Immortal) return;

            // ここに、プレイヤーのキャラクターコントローラへダメージを与えるコードを記述してください。

            OnTakeDamage.Invoke();

            // ダメージ処理後、Health 変数をキャラクターコントローラ側の体力値に同期させてください。

            if (Health <= 0)
            {
                // プレイヤーが死亡した際の挙動を制御します。

                if (m_Collider != null) m_Collider.enabled = false;
                OnDeath.Invoke();
            }
        }

        /// <summary>
        /// この Non-AI ターゲットを、死亡前のデフォルト設定にリセットします。
        /// （体力、レイヤー、タグを含む）
        /// </summary>
        public void ResetTarget()
        {
            Health = StartHealth;
            if (m_Collider != null) m_Collider.enabled = true;
        }

        public virtual Transform TargetTransform()
        {
            return transform;
        }

        /// <summary>
        /// このターゲットが攻撃中かどうかの検出に使用します。
        /// </summary>
        public virtual bool IsAttacking()
        {
            return false;
        }

        /// <summary>
        /// このターゲットが防御中かどうかの検出に使用します。
        /// </summary>
        public virtual bool IsBlocking()
        {
            return false;
        }

        /// <summary>
        /// このターゲットが回避中かどうかの検出に使用します。
        /// </summary>
        public virtual bool IsDodging()
        {
            return false;
        }

        public virtual void TriggerStun(float StunLength)
        {
            // 必要であれば、ここにカスタムのスタン処理を記述できます（必須ではありません）。
        }
    }
}
