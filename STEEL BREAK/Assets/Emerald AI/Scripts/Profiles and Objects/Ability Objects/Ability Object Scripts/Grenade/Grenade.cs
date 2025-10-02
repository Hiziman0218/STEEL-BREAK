using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;
using UnityEngine.Audio;

namespace EmeraldAI
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Rigidbody))]
    public class Grenade : MonoBehaviour, IAvoidable
    {
        #region 変数
        [Header("現在のグレネードアビリティデータ（GrenadeAbility）")]
        GrenadeAbility CurrentAbilityData;

        [Header("所有者の EmeraldSystem 参照")]
        EmeraldSystem EmeraldComponent;

        [Header("現在のターゲット（Transform）")]
        Transform CurrentTarget;

        public Transform AbilityTarget { get => CurrentTarget; set => CurrentTarget = value; }

        [Header("初期ターゲット位置（投擲時の参照位置）")]
        Vector3 InitialTargetPosition;

        [Header("自分自身の AudioSource 参照")]
        AudioSource m_AudioSource;

        [Header("移動音などのサウンドエフェクト（Resources からロード）")]
        GameObject m_SoundEffect;

        [Header("自分自身の Rigidbody 参照")]
        Rigidbody m_Rigidbody;

        [Header("このグレネードの所有者（発射元）")]
        GameObject Owner;

        [Header("グレネードのコライダー参照")]
        Collider GrenadeCollider;

        [Header("投擲（初期化）された時間（秒）")]
        float StartTime;

        [Header("初期化が完了したか（内部フラグ）")]
        bool Initialized;

        [Header("いずれかと衝突したか（内部フラグ）")]
        bool HasCollided;

        [Header("ターゲットとの角度（内部参照用）")]
        float TargetAngle;
        #endregion

        /// <summary>
        /// 【Awake】
        /// プロジェクタイルを最初に使用するときに必要なコンポーネントや設定を初期化します。
        /// </summary>
        void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
            m_AudioSource.loop = true;
            m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
            m_AudioSource.spatialBlend = 1;
            m_AudioSource.maxDistance = 20;
            m_SoundEffect = Resources.Load("Emerald Sound") as GameObject;

            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.linearDamping = 0.1f;
            m_Rigidbody.angularDamping = 0.05f;

            GrenadeCollider = GetComponent<Collider>();
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        /// <summary>
        /// 【Initialize】
        /// 渡された情報でプロジェクタイル（グレネード）を初期化します。
        /// </summary>
        /// <param name="owner">このプロジェクタイルの所有者。</param>
        /// <param name="currentTarget">このプロジェクタイルの現在のターゲット。</param>
        /// <param name="abilityData">このプロジェクタイルのアビリティデータ。</param>
        public void Initialize(GameObject owner, Transform currentTarget, GrenadeAbility abilityData)
        {
            Owner = owner;
            CurrentTarget = currentTarget;
            EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            CurrentAbilityData = abilityData;

            GrenadeCollider.enabled = false;
            gameObject.layer = CurrentAbilityData.GrenadeSettings.GrenadeLayer;

            // 投げてから 0.1 秒後にコライダーを有効化（手のコライダーとの干渉を避ける）
            Invoke(nameof(EnableCollider), 0.1f);

            Initialized = false;
            HasCollided = false;

            InitializeProjectile(); // プロジェクタイルの設定を初期化。
            StartTime = Time.time;
        }

        /// <summary>
        /// 【EnableCollider】
        /// 投擲後 0.1 秒でグレネードのコライダーを有効化します。
        /// 手から離れる猶予を設け、所有者のコライダーと干渉しないようにします。
        /// </summary>
        void EnableCollider()
        {
            GrenadeCollider.enabled = true;
        }

        /// <summary>
        /// 【InitializeProjectile】
        /// 投擲方向・初速などを設定します。
        /// </summary>
        void InitializeProjectile()
        {
            InitialTargetPosition = CurrentTarget.transform.position;
            transform.LookAt(InitialTargetPosition);
            Initialized = true;

            if (CurrentAbilityData.GrenadeSettings.ThrowSound)
                m_AudioSource.PlayOneShot(CurrentAbilityData.GrenadeSettings.ThrowSound);

            Vector3 TargetDirection = InitialTargetPosition - transform.position;
            float Distance = Vector3.Distance(InitialTargetPosition, transform.position);

            m_Rigidbody.linearVelocity = new Vector3(0f, 0f, 0f);
            m_Rigidbody.angularVelocity = new Vector3(0f, 0f, 0f);

            float RandomizedDepth = Random.Range(0.85f, 1f);
            float ThrowHeightOffset = Mathf.Lerp(1f, 0.5f, CurrentAbilityData.GrenadeSettings.ThrowHeight / 10f);

            m_Rigidbody.AddForce(
                (TargetDirection.normalized * Distance * (ThrowHeightOffset * RandomizedDepth)) +
                (Vector3.up * CurrentAbilityData.GrenadeSettings.ThrowHeight),
                ForceMode.Impulse);
        }

        /// <summary>
        /// 【Update】
        /// プロジェクタイル生成後の経過時間を追跡します。
        /// </summary>
        void Update()
        {
            ProjectileTimeout(); // アクティブになってからの時間を追跡
        }

        void FixedUpdate()
        {
            // 衝突前かつ「投擲回転」が有効なときに、見た目の回転演出を与える
            if (!HasCollided && CurrentAbilityData.GrenadeSettings.RotateOnThrow)
            {
                Quaternion deltaRotation = Quaternion.Euler(new Vector3(300, 20, 20) * Time.fixedDeltaTime);
                m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);
            }
        }

        /// <summary>
        /// 【ProjectileTimeout】
        /// アクティブからの時間を追跡し、ExplosionTime に達したら爆発させます。
        /// </summary>
        void ProjectileTimeout()
        {
            if (!Initialized) return;

            float TimeAlive = Time.time - StartTime;
            if (TimeAlive > CurrentAbilityData.GrenadeSettings.ExplosionTime)
            {
                Explode();
                Initialized = false;
            }
        }

        void OnCollisionEnter(Collision col)
        {
            if (!HasCollided) HasCollided = true;

            // 着地や跳ね返り時のSEをランダムなピッチ・音量で再生
            m_AudioSource.pitch = Random.Range(0.85f, 1.15f);
            m_AudioSource.volume = Random.Range(0.6f, 1f);

            if (col.relativeVelocity.magnitude > 0.4f)
                m_AudioSource.PlayOneShot(CurrentAbilityData.GrenadeSettings.ImpactSoundsList[Random.Range(0, CurrentAbilityData.GrenadeSettings.ImpactSoundsList.Count)]);
        }

        /// <summary>
        /// 【Explode】
        /// 周囲のAIにダメージや効果を与えます（本スクリプトの公開変数設定に基づく）。
        /// </summary>
        public void Explode()
        {
            // 爆発エフェクトを生成
            CurrentAbilityData.GrenadeSettings.ExplodeGrenade(EmeraldComponent.gameObject, transform.position);

            // 少し遅らせて自動デスポーン
            Invoke(nameof(Despawn), 0.11f);

            // ダメージ対象（AI検出用レイヤー）
            List<Collider> TargetsInRange = Physics.OverlapSphere(transform.position, CurrentAbilityData.GrenadeSettings.ExplosionRadius, EmeraldComponent.DetectionComponent.DetectionLayerMask).ToList();
            TargetsInRange.Remove(Owner.GetComponent<Collider>()); // 所有者のコライダーが含まれていれば除外

            for (int i = 0; i < TargetsInRange.Count; i++)
            {
                // 所有者視点で「Enemy」の関係のみダメージ（ただし CanDamageFriendlies が有効なら味方にもヒット）
                if (CurrentAbilityData.GrenadeSettings.CanDamageFriendlies || EmeraldAPI.Faction.GetTargetFactionRelation(EmeraldComponent, TargetsInRange[i].transform) == "Enemy")
                {
                    ICombat m_ICombat = TargetsInRange[i].GetComponent<ICombat>();
                    DamageTarget(TargetsInRange[i].gameObject, m_ICombat);
                }
            }

            // 物理的に吹き飛ばす対象（指定レイヤーの Rigidbody を探索）
            List<Collider> RigidbodiesInRange = Physics.OverlapSphere(transform.position, CurrentAbilityData.GrenadeSettings.ExplosionRadius, CurrentAbilityData.GrenadeSettings.RigidbodyLayers).ToList();
            RigidbodiesInRange.Remove(Owner.GetComponent<Collider>());

            for (int i = 0; i < RigidbodiesInRange.Count; i++)
            {
                Rigidbody TargetRigidbody = RigidbodiesInRange[i].GetComponent<Rigidbody>();
                if (TargetRigidbody != null) StartCoroutine(AddRagdollForce(TargetRigidbody));
            }
        }

        /// <summary>
        /// 【DamageTarget】
        /// 爆心半径内の各対象にダメージを与えます（IDamageable を持つことが前提）。
        /// </summary>
        void DamageTarget(GameObject Target, ICombat m_ICombat)
        {
            // テレポート中の対象は無視
            if (Target.transform.localScale == Vector3.one * 0.003f) return;

            // ノックバックが有効なら、確率でノックバック
            if (CurrentAbilityData.KnockbackSettings.Enabled && CurrentAbilityData.KnockbackSettings.RollForKnockback())
            {
                Vector3 Direction = (Target.transform.position - transform.position).normalized;
                if (m_ICombat != null) Owner.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(CurrentAbilityData.KnockbackSettings.KnockbackSequence(Direction, m_ICombat.TargetTransform(), m_ICombat));
            }

            // スタンが有効なら、確率でスタン
            if (CurrentAbilityData.StunnedSettings.Enabled && CurrentAbilityData.StunnedSettings.RollForStun())
            {
                if (m_ICombat != null) m_ICombat.TriggerStun(CurrentAbilityData.StunnedSettings.StunLength);
            }

            // ダメージが無効化されている場合は終了
            if (!CurrentAbilityData.DamageSettings.Enabled) return;

            var m_IDamageable = Target.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                bool IsCritHit = CurrentAbilityData.DamageSettings.GenerateCritHit();

                // 爆心からの距離に応じてダメージを減衰
                int DamageMitigation = Mathf.RoundToInt(
                    (1f - Vector3.Distance(Target.transform.position, transform.position) / CurrentAbilityData.GrenadeSettings.ExplosionRadius)
                    * CurrentAbilityData.DamageSettings.GenerateDamage(IsCritHit));

                m_IDamageable.Damage(DamageMitigation, transform, 0, IsCritHit);
                CurrentAbilityData.DamageSettings.DamageTargetOverTime(CurrentAbilityData, CurrentAbilityData.DamageSettings, Owner, Target);

                // 対象が死亡し、AIでLBDを持つならラグドール力を与える
                if (m_IDamageable.Health <= 0)
                {
                    EmeraldSystem DetectedEmeraldAgent = Target.GetComponent<EmeraldSystem>();
                    if (DetectedEmeraldAgent != null && DetectedEmeraldAgent.LBDComponent != null) StartCoroutine(AddRagdollForceAI(DetectedEmeraldAgent));
                }
            }
            else
            {
                Debug.Log(Target.gameObject + " には IDamageable および/または ICombat コンポーネントがありません。追加してください。");
            }
        }

        IEnumerator AddRagdollForceAI(EmeraldSystem EmeraldTarget)
        {
            float t = 0;
            float ForceMitigation = Mathf.RoundToInt((1f - Vector3.Distance(EmeraldTarget.transform.position, transform.position) / CurrentAbilityData.GrenadeSettings.ExplosionRadius) * CurrentAbilityData.DamageSettings.BaseDamageSettings.RagdollForce) * 4f;

            Rigidbody TargetRigidbody = null;
            if (EmeraldTarget != null) TargetRigidbody = EmeraldTarget.DetectionComponent.HeadTransform.GetComponent<Rigidbody>();

            if (TargetRigidbody != null)
            {
                while (t < 0.1f)
                {
                    t += Time.fixedDeltaTime;
                    TargetRigidbody.AddForce((transform.position - EmeraldTarget.transform.position).normalized * -ForceMitigation * 1f + (Vector3.up * ForceMitigation * 1f), ForceMode.Acceleration);
                    yield return null;
                }
            }
        }

        IEnumerator AddRagdollForce(Rigidbody TargetRigidbody)
        {
            float t = 0;
            float ForceMitigation = Mathf.RoundToInt((1f - Vector3.Distance(TargetRigidbody.transform.position, transform.position) / (float)CurrentAbilityData.GrenadeSettings.ExplosionRadius * 1.5f) * CurrentAbilityData.DamageSettings.BaseDamageSettings.RagdollForce) * 4f;

            if (TargetRigidbody != null)
            {
                while (t < 0.1f)
                {
                    t += Time.fixedDeltaTime;
                    TargetRigidbody.AddForce((transform.position - TargetRigidbody.transform.position).normalized * -ForceMitigation * 0.15f + (Vector3.up * ForceMitigation * 0.15f), ForceMode.Acceleration);
                    yield return null;
                }
            }
        }

        void Despawn()
        {
            this.enabled = false;
            EmeraldObjectPool.Despawn(gameObject);
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}
