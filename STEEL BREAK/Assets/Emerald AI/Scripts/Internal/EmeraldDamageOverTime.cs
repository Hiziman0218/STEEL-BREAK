using UnityEngine;

namespace EmeraldAI.Utility
{
    // このクラスは「継続ダメージ（Damage over Time, DoT）」を対象へ付与・維持し、一定間隔でダメージを与えるためのコンポーネントです。
    public class EmeraldDamageOverTime : MonoBehaviour
    {
        [Header("ダメージの刻み間隔を計測する内部タイマー（秒）。TickRate に達すると一度ダメージ適用")]
        float DamageTimer; // 毎フレーム加算し、TickRate 以上になったらダメージを与える

        [Header("DoT 効果の総継続時間を計測する内部タイマー（秒）。DamageOverTimeLength を超えたら終了処理")]
        float ActiveLengthTimer; // エフェクト全体の寿命を測る

        [Header("このDoTを発生させたアビリティの参照。効果の識別と解除に使用")]
        EmeraldAbilityObject m_AbilityObject; // 対象の Active Effect 管理に使う

        [Header("DoT実行中のサウンド再生に使用する AudioSource（実行時に自動追加）")]
        AudioSource m_AudioSource; // OneShot 再生などに使用

        [Header("DoTの適用対象（被ダメージ側）の Transform 参照")]
        Transform m_TargetTransform; // IDamageable/ICombat の取得元

        [Header("DoTの攻撃者（加害側）の Transform 参照。ヘイトや方向情報の伝達に利用")]
        Transform m_AttackerTransform; // Damage 呼び出し時に攻撃者として渡す

        [Header("1回のダメージを与えるまでの間隔（秒）。この間隔ごとにダメージを1Tick適用")]
        float TickRate; // 例：0.5なら0.5秒ごとにダメージ

        [Header("1Tickあたりに与えるダメージ量（整数）")]
        int DamagePerTick; // 毎Tick適用する固定ダメージ

        [Header("DoTが持続する合計時間（秒）。この時間を超えたらDoTを終了")]
        float DamageOverTimeLength; // 効果の寿命

        [Header("DoT適用時に再生するエフェクト（任意）。ICombat.DamagePosition() 位置にスポーン")]
        GameObject DamageOverTimeEffect; // 視覚効果

        [Header("生成したDoTエフェクトの自動破棄までの秒数（オブジェクトプールのタイムアウト）")]
        float OverTimeEffectTimeOutSeconds; // 視覚効果の生存時間

        [Header("DoT適用中に再生するサウンド（任意）。ランダムに1つ選択して再生")]
        AudioClip DamageOverTimeSound; // 効果音

        void Start()
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>(); // 実行時に AudioSource を付与（重複回避のためここで生成）
        }

        /// <summary>
        /// DoTコンポーネントを初期化します。
        /// AbilityObject（発生源）、DamageData（ダメージ設定）、TargetTransform（対象）、AttackerTransform（攻撃者）を受け取り、
        /// Tick間隔やダメージ量、エフェクト・サウンドなどの各種パラメータを設定します。
        /// </summary>
        public void Initialize(EmeraldAbilityObject AbilityObject, AbilityData.DamageData DamageData, Transform TargetTransform, Transform AttackerTransform)
        {
            DamageTimer = 0; // ダメージ用タイマー初期化
            ActiveLengthTimer = 0; // 総継続時間タイマー初期化
            m_TargetTransform = TargetTransform; // 対象を記録
            m_AttackerTransform = AttackerTransform; // 攻撃者を記録
            m_AbilityObject = AbilityObject; // アビリティ情報を記録

            TickRate = DamageData.DamageOverTimeSettings.TickRate; // Tick間隔を取得
            DamagePerTick = DamageData.DamageOverTimeSettings.DamagePerTick; // 1Tickのダメージ量を取得
            DamageOverTimeEffect = DamageData.DamageOverTimeSettings.DamageOverTimeEffect; // 再生するエフェクト
            DamageOverTimeLength = DamageData.DamageOverTimeSettings.DamageOverTimeLength; // 総継続時間
            OverTimeEffectTimeOutSeconds = DamageData.DamageOverTimeSettings.OverTimeEffectTimeOutSeconds; // エフェクトの生存時間
            if (DamageData.DamageOverTimeSettings.OverTimeSounds.Count > 0)
                DamageOverTimeSound = DamageData.DamageOverTimeSettings.OverTimeSounds[Random.Range(0, DamageData.DamageOverTimeSettings.OverTimeSounds.Count)]; // 複数候補からランダムに1つ取得
        }

        /// <summary>
        /// DoT（EmeraldAIDamageOverTime）の内部タイマーを更新します。
        /// Tickごとにダメージを与え、総継続時間を超えたら効果の後処理（Active Effectからの削除、プールへの返却）を行います。
        /// </summary>
        void Update()
        {
            DamageTimer += Time.deltaTime; // 経過時間を加算（Tick判定用）
            ActiveLengthTimer += Time.deltaTime; // 総継続時間の計測

            if (ActiveLengthTimer >= DamageOverTimeLength + 0.05f) // わずかに余裕を持たせて終了判定
            {
                IDamageableHelper.RemoveAbilityActiveEffect(m_TargetTransform.gameObject, m_AbilityObject); // 現在のアビリティ効果を対象の「アクティブ効果」リストから削除

                if (!m_AudioSource.isPlaying) // サウンド再生が終わっていれば
                {
                    EmeraldObjectPool.Despawn(gameObject); // このDoTコンポーネントをオブジェクトプールへ返却（デスポーン）
                }
            }

            if (DamageTimer >= TickRate && ActiveLengthTimer <= DamageOverTimeLength + 0.05f) // Tick間隔に到達かつ効果継続中
            {
                DamageTarget(); // 1Tick分のダメージを適用
                DamageTimer = 0; // タイマーをリセット
            }
        }

        /// <summary>
        /// 現在のアビリティ情報を用いて、対象の IDamageable を通じてダメージを与えます。
        /// 併せて、視覚エフェクトやサウンドが設定されていれば再生します。
        /// </summary>
        void DamageTarget()
        {
            var m_IDamageable = m_TargetTransform.GetComponent<IDamageable>(); // ダメージ適用インターフェース
            var m_ICombat = m_TargetTransform.GetComponent<ICombat>(); // ダメージ表示位置の取得などに使用

            if (m_IDamageable != null && m_ICombat != null) // 対象が必要なインターフェースを持っている場合のみ実行
            {
                // IDamageable を通じて対象にダメージを与える
                m_IDamageable.Damage(DamagePerTick, m_AttackerTransform, 50, false); // 例：ノックバック量などは第三引数、クリティカル判定は第四引数

                // 視覚エフェクトをダメージ位置に生成（設定がある場合）
                if (DamageOverTimeEffect != null)
                {
                    EmeraldObjectPool.SpawnEffect(DamageOverTimeEffect, m_ICombat.DamagePosition(), Quaternion.identity, OverTimeEffectTimeOutSeconds);
                }

                // サウンドを再生（設定がある場合）
                if (DamageOverTimeSound != null)
                {
                    m_AudioSource.PlayOneShot(DamageOverTimeSound);
                }
            }
        }
    }
}
