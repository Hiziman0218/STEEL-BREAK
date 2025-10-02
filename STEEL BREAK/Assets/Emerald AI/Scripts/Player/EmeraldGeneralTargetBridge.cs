using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [RequireComponent(typeof(TargetPositionModifier))]
    [RequireComponent(typeof(FactionExtension))]
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/getting-started/setting-up-a-player-with-emerald-ai")]
    public class EmeraldGeneralTargetBridge : MonoBehaviour, IDamageable, ICombat
    {
        [Header("開始時の体力（初期HP）")]
        public int StartingHealth = 50;

        [Header("不死（ダメージを受けても死亡しない）")]
        public bool Immortal = false;

        [Header("ダメージを受けたときのイベント")]
        public UnityEvent OnTakeDamage;

        [Header("死亡時に発火するイベント")]
        public UnityEvent OnDeath;

        [Header("死亡時にデバッグログを出力する")]
        public bool DebugLogDeath = true;

        [Header("設定折りたたみを隠す（エディタ用）")]
        public bool HideSettingsFoldout;

        [Header("ヘルス設定の折りたたみ（エディタ用）")]
        public bool HealthSettingsFoldout = true;

        public int StartHealth { get => StartingHealth; set => StartingHealth = value; }

        [field: Header("現在の体力（実行時に更新されます）")]
        [field: SerializeField] public int Health { get; set; }

        [field: Header("アクティブな効果一覧（文字列）")]
        [field: SerializeField] public List<string> ActiveEffects { get; set; }

        [Header("照準位置補正用の参照（TargetPositionModifier）")]
        TargetPositionModifier m_TargetPositionModifier;

        [Header("自身のコライダー参照")]
        Collider m_Collider;

        void Start()
        {
            Health = StartingHealth;
            m_TargetPositionModifier = GetComponent<TargetPositionModifier>();
            m_Collider = GetComponent<Collider>();
        }

        public void Damage(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 100, bool CriticalHit = false)
        {
            DefaultDamage(DamageAmount, AttackerTransform);

            // 有効な場合、ターゲット位置にコンバットテキスト（与ダメージ）を生成します。
            if (CombatTextSystem.Instance != null) CombatTextSystem.Instance.CreateCombatText(DamageAmount, DamagePosition(), CriticalHit, false, false);
        }

        void OnEnable()
        {
            if (Health <= 0) ResetTarget();
        }

        /// <summary>
        /// 外部ソースからダメージを受けた際の、このオブジェクトのダメージ位置を参照するために使用します。
        /// </summary>
        public Vector3 DamagePosition()
        {
            if (m_TargetPositionModifier != null)
                return new Vector3(m_TargetPositionModifier.TransformSource.position.x, m_TargetPositionModifier.TransformSource.position.y + m_TargetPositionModifier.PositionModifier, m_TargetPositionModifier.TransformSource.position.z);
            else
                return transform.position + new Vector3(0, transform.localScale.y / 2, 0);
        }

        void DefaultDamage(int DamageAmount, Transform Target)
        {
            if (Immortal) return;

            Health -= DamageAmount;
            OnTakeDamage.Invoke();

            if (Health <= 0)
            {
                if (DebugLogDeath)
                    Debug.Log("非AIターゲットは死亡しました。");

                if (m_Collider != null) m_Collider.enabled = false;
                gameObject.layer = 0;
                gameObject.tag = "Untagged";
                OnDeath.Invoke();
            }
        }

        /// <summary>
        /// この Non-AI ターゲットを、死亡前のデフォルト設定にリセットします（体力、レイヤー、タグなど）。
        /// </summary>
        public void ResetTarget()
        {
            Health = StartingHealth;
            if (m_Collider != null) m_Collider.enabled = true;
        }

        public Transform TargetTransform()
        {
            return transform;
        }

        /// <summary>
        /// このターゲットが攻撃中かどうかの検出に使用します。
        /// </summary>
        public bool IsAttacking()
        {
            return false;
        }

        /// <summary>
        /// このターゲットが防御中かどうかの検出に使用します。
        /// </summary>
        public bool IsBlocking()
        {
            return false;
        }

        /// <summary>
        /// このターゲットが回避中かどうかの検出に使用します。
        /// </summary>
        public bool IsDodging()
        {
            return false;
        }

        public void TriggerStun(float StunLength)
        {
            // 任意：ここにカスタムのスタン付与処理を記述できます（必須ではありません）
        }
    }
}
