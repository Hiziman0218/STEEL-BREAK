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
    public class ArrowProjectile : MonoBehaviour, IAvoidable
    {
        #region 変数
        [Header("現在のアビリティデータ（ArrowProjectileAbility）")]
        ArrowProjectileAbility CurrentAbilityData;

        [Header("現在のターゲット（Transform）")]
        Transform CurrentTarget;

        public Transform AbilityTarget { get => CurrentTarget; set => CurrentTarget = value; }

        [Header("初期ターゲット位置（ダメージ位置のキャッシュ）")]
        Vector3 InitialTargetPosition;

        [Header("ターゲット側の ICombat 参照（ダメージ位置取得に使用）")]
        ICombat StartingICombat;

        [Header("所有者の EmeraldSystem 参照")]
        EmeraldSystem EmeraldComponent;

        [Header("自分自身の AudioSource 参照")]
        AudioSource m_AudioSource;

        [Header("移動音などのサウンドエフェクト（Resources からロード）")]
        GameObject m_SoundEffect;

        [Header("当たり判定に使用する Collider（Trigger）")]
        Collider m_Collider;

        [Header("必要に応じて生成される SphereCollider（Trigger）")]
        SphereCollider m_SphereCollider;

        [Header("自分自身の Rigidbody 参照")]
        Rigidbody m_Rigidbody;

        [Header("このプロジェクタイルの所有者（発射元）")]
        GameObject Owner;

        [Header("衝突判定を無視するコライダー一覧（LBD 内部コライダー等）")]
        List<Collider> IgnoredColliders = new List<Collider>();

        [Header("発射（初期化）された時間（秒）")]
        float StartTime;

        [Header("初期化が完了したか（内部フラグ）")]
        bool Initialized;

        [Header("デスポーン用のコルーチン参照（内部用）")]
        Coroutine DespawnCoroutine;

        //[SerializeField]
        [Header("プロジェクタイルに関連する見た目/エフェクトのキャッシュ")]
        public List<ProjectileEffectsClass> m_ProjectileObjects = new List<ProjectileEffectsClass>();
        #endregion

        /// <summary>
        /// 最初にプロジェクタイルが使用される際に、必要なコンポーネントや設定を初期化します。
        /// </summary>
        void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
            m_AudioSource.loop = true;
            m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
            m_AudioSource.spatialBlend = 1;
            m_AudioSource.maxDistance = 20;

            // すでにコライダーが付いている場合は、それをプロジェクタイルのコライダーとして使用する。
            m_Collider = GetComponent<Collider>();
            if (m_Collider != null)
            {
                m_Collider.isTrigger = true;
                m_Collider.enabled = false;
            }
            // 付いていない場合は SphereCollider を生成する。
            else
            {
                m_Collider = gameObject.AddComponent<SphereCollider>();
                m_SphereCollider = gameObject.GetComponent<SphereCollider>();
                m_Collider.isTrigger = true;
                m_Collider.enabled = false;
            }

            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.useGravity = true;
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            m_SoundEffect = Resources.Load("Emerald Sound") as GameObject;

            // Awake 時に全エフェクトオブジェクトを走査し、Projectile Module で使用できるようキャッシュする。
            m_ProjectileObjects.Add(new ProjectileEffectsClass(gameObject.GetComponent<ParticleSystemRenderer>(), gameObject));

            foreach (Transform child in transform)
            {
                m_ProjectileObjects.Add(new ProjectileEffectsClass(child.GetComponent<ParticleSystemRenderer>(), child.gameObject));
            }
        }

        /// <summary>
        /// 渡された情報でプロジェクタイルを初期化します。
        /// </summary>
        /// <param name="owner">このプロジェクタイルの所有者。</param>
        /// <param name="currentTarget">このプロジェクタイルの現在のターゲット。</param>
        /// <param name="abilityData">このプロジェクタイルのアビリティデータ。</param>
        public void Initialize(GameObject owner, Transform currentTarget, ArrowProjectileAbility abilityData)
        {
            StartCoroutine(InitializeInternal(owner, currentTarget, abilityData));
        }

        IEnumerator InitializeInternal(GameObject owner, Transform currentTarget, ArrowProjectileAbility abilityData)
        {
            Owner = owner;
            CurrentTarget = currentTarget;
            if (CurrentTarget != null)
            {
                StartingICombat = CurrentTarget.GetComponent<ICombat>();
                InitialTargetPosition = StartingICombat.DamagePosition();
            }

            EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            CurrentAbilityData = abilityData;
            m_Collider.enabled = false; // 発射されるまではコライダーを有効化しない

            GetLBDColliders(); // Owner の LBD コンポーネントを参照し、内部コライダーを無視対象に設定する。

            if (m_SphereCollider != null)
            {
                m_SphereCollider.radius = CurrentAbilityData.ColliderSettings.ColliderRadius;
                m_SphereCollider.center = Vector3.forward * CurrentAbilityData.ColliderSettings.ZOffet;
            }

            gameObject.layer = CurrentAbilityData.ColliderSettings.ProjectileLayer;
            Initialized = false;

            if (CurrentAbilityData.ProjectileSettings.EffectsToDisable.Count > 0) SetEffectsState(true); // EffectsToDisable の名前と一致するエフェクトを有効化
            InitializeProjectile(); // プロジェクタイルの設定を初期化

            yield return new WaitForSeconds(CurrentAbilityData.ProjectileSettings.LaunchProjectileDelay);

            StartTime = Time.time;
            m_Collider.enabled = true; // 発射後にコライダーを有効化
            if (CurrentAbilityData.ProjectileSettings.TravelSound != null) m_AudioSource.PlayOneShot(CurrentAbilityData.ProjectileSettings.TravelSound);
            CurrentAbilityData.ProjectileSettings.SpawnLaunchProjectileEffect(Owner, transform.position);
            Initialized = true;
        }

        /// <summary>
        /// Owner の LBD コンポーネントを参照して、内部コライダーを無視対象へ登録します。
        /// </summary>
        void GetLBDColliders()
        {
            // Owner の LBD コンポーネントを参照し、内部コライダーを無視対象に設定する。
            LocationBasedDamage LBDComponent = Owner.GetComponent<LocationBasedDamage>();
            if (LBDComponent)
            {
                IgnoredColliders.Clear();
                for (int i = 0; i < LBDComponent.ColliderList.Count; i++)
                {
                    IgnoredColliders.Add(LBDComponent.ColliderList[i].ColliderObject);
                }
            }
        }

        /// <summary>
        /// プロジェクタイルの初期向きなどを設定します。
        /// </summary>
        void InitializeProjectile()
        {
            transform.LookAt(InitialTargetPosition);
            CurrentAbilityData.ProjectileSettings.SpawnEffect(Owner, transform.position);
        }

        /// <summary>
        /// プロジェクタイル生成後の経過時間の追跡と移動処理を行います。
        /// </summary>
        void Update()
        {
            ProjectileTimeout(); // アクティブになってからの時間を追跡
            MoveProjectile();    // 現在のターゲット方向へ移動
        }

        /// <summary>
        /// アクティブになってからの時間を追跡します。ProjectileTimeoutSeconds を超えたら
        /// 現在位置でインパクトエフェクトを再生し、プロジェクタイルをデスポーンします。
        /// </summary>
        void ProjectileTimeout()
        {
            if (!Initialized) return;

            float TimeAlive = Time.time - StartTime;
            if (TimeAlive > CurrentAbilityData.ProjectileSettings.ProjectileTimeoutSeconds && m_Collider.enabled)
            {
                CurrentAbilityData.ProjectileSettings.SpawnImpactEffect(Owner, transform.position);
                this.enabled = false;
                EmeraldObjectPool.Despawn(gameObject);
            }
        }

        /// <summary>
        /// アビリティが初期化済みのときに、ターゲットへ向かってプロジェクタイルを移動させます。
        /// </summary>
        void MoveProjectile()
        {
            if (Initialized)
            {
                var step = CurrentAbilityData.ArrowProjectileSettings.ProjectileSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, step);
            }
        }

        /// <summary>
        /// 衝突時の処理（自分自身や他のプロジェクタイル以外との衝突に対して処理）。
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            if (!this.enabled) return; // スクリプトが有効なときのみ衝突判定を許可
            if (((1 << other.gameObject.layer) & CurrentAbilityData.ColliderSettings.CollidableLayers) != 0 && other.gameObject != Owner && !IgnoredColliders.Contains(other)) Impact(other.gameObject);
        }

        /// <summary>
        /// 衝突関連の全機能を処理します。
        /// </summary>
        void Impact(GameObject TargetHit)
        {
            //if (Owner.GetComponent<EmeraldAISystem>().DetectionComponent.GetTargetFaction(TargetHit.GetComponent<LocationBasedDamageArea>().EmeraldComponent.transform) == "Friendly") return;

            m_Collider.enabled = false; // 予期しない衝突を防ぐため、コライダーを無効化
            Initialized = false;        // 初期化フラグを下ろしてプロジェクタイルの動作を停止
            CurrentAbilityData.ProjectileSettings.SpawnImpactEffect(Owner, transform.position); // インパクトエフェクトとサウンドを再生
            Invoke(nameof(ImpactDespawn), CurrentAbilityData.ColliderSettings.CollisionTimeout); // CollisionTimeout 後にデスポーン（演出終了猶予）
            DamageTarget(TargetHit); // ヒット対象にダメージを与える（LBD があれば部位ダメージ、なければ IDamageable へ）

            if (CurrentAbilityData.ProjectileSettings.EffectsToDisable.Count > 0)
            {
                if (!CurrentAbilityData.ArrowProjectileSettings.AttachToTarget) SetEffectsState(false); // EffectsToDisable の名前と一致するエフェクトを無効化
                else if (TargetHit.activeSelf) StartCoroutine(SetEffectsStateDelay(false)); // （遅延ありで）エフェクトを無効化
            }
        }

        /// <summary>
        /// SetEffectsState 呼び出しを遅延させます。
        /// </summary>
        IEnumerator SetEffectsStateDelay(bool State)
        {
            if (!State) yield return new WaitForSeconds(1);
            SetEffectsState(State);
        }

        /// <summary>
        /// EffectsToDisable の名前と一致するエフェクトの有効/無効を切り替えます。
        /// </summary>
        void SetEffectsState(bool State)
        {
            for (int i = 0; i < m_ProjectileObjects.Count; i++)
            {
                for (int j = 0; j < CurrentAbilityData.ProjectileSettings.EffectsToDisable.Count; j++)
                {
                    if (m_ProjectileObjects[i].EffectObject.name == CurrentAbilityData.ProjectileSettings.EffectsToDisable[j])
                    {
                        if (m_ProjectileObjects[i].EffectParticle != null) m_ProjectileObjects[i].EffectParticle.enabled = State;
                        else if (m_ProjectileObjects[i].EffectObject != null && m_ProjectileObjects[i].EffectObject != this.gameObject) m_ProjectileObjects[i].EffectObject.SetActive(State);
                    }
                }
            }
        }

        /// <summary>
        /// ヒット対象にダメージを与えます。LocationBasedDamageArea があれば部位ダメージ、なければ IDamageable にダメージ。
        /// </summary>
        void DamageTarget(GameObject Target)
        {
            LocationBasedDamageArea m_LocationBasedDamageArea = Target.GetComponent<LocationBasedDamageArea>();

            // AI の DetectionLayerMask に含まれるレイヤー、または対象に LBD がある場合のみダメージ可。
            if (!m_LocationBasedDamageArea && ((1 << Target.layer) & EmeraldComponent.DetectionComponent.DetectionLayerMask) == 0) return;

            // テレポート中の対象は除外。
            if (m_LocationBasedDamageArea != null && m_LocationBasedDamageArea.EmeraldComponent.transform.localScale == Vector3.one * 0.003f || Target.transform.localScale == Vector3.one * 0.003f) return;

            var m_ICombat = Target.GetComponentInParent<ICombat>();

            // ノックバックが有効なら、確率でノックバックを適用。
            if (CurrentAbilityData.KnockbackSettings.Enabled && CurrentAbilityData.KnockbackSettings.RollForKnockback())
            {
                Vector3 Direction = transform.forward;
                if (m_ICombat != null) Owner.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(CurrentAbilityData.KnockbackSettings.KnockbackSequence(Direction, m_ICombat.TargetTransform(), m_ICombat));
            }

            // スタンが有効なら、確率でスタンを付与。
            if (CurrentAbilityData.StunnedSettings.Enabled && CurrentAbilityData.StunnedSettings.RollForStun())
            {
                if (m_ICombat != null) m_ICombat.TriggerStun(CurrentAbilityData.StunnedSettings.StunLength);
            }

            // ダメージが無効化されている場合はここで終了。
            if (!CurrentAbilityData.DamageSettings.Enabled) return;

            if (m_LocationBasedDamageArea == null)
            {
                var m_IDamageable = Target.GetComponent<IDamageable>();
                if (m_IDamageable != null)
                {
                    bool IsCritHit = CurrentAbilityData.DamageSettings.GenerateCritHit();
                    m_IDamageable.Damage(CurrentAbilityData.DamageSettings.GenerateDamage(IsCritHit), Owner.transform, CurrentAbilityData.DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                    CurrentAbilityData.DamageSettings.DamageTargetOverTime(CurrentAbilityData, CurrentAbilityData.DamageSettings, Owner, Target);
                    m_AudioSource.Stop();
                }
                else
                {
                    Debug.Log(Target.gameObject + " には IDamageable コンポーネントがありません。追加してください。");
                }
            }
            else if (m_LocationBasedDamageArea != null)
            {
                bool IsCritHit = CurrentAbilityData.DamageSettings.GenerateCritHit();
                m_LocationBasedDamageArea.DamageArea(CurrentAbilityData.DamageSettings.GenerateDamage(IsCritHit), Owner.transform, CurrentAbilityData.DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                CurrentAbilityData.DamageSettings.DamageTargetOverTime(CurrentAbilityData, CurrentAbilityData.DamageSettings, Owner, m_ICombat.TargetTransform().gameObject);
                m_AudioSource.Stop();
            }

            if (CurrentAbilityData.ArrowProjectileSettings.AttachToTarget)
            {
                AttachToCollider(Target.transform);
            }
        }

        /// <summary>
        /// 指定されたコライダー（Transform）にプロジェクタイルを取り付けます。
        /// </summary>
        void AttachToCollider(Transform Target)
        {
            transform.SetParent(Target);
        }

        /// <summary>
        /// CollisionTimeout 経過後にインパクトエフェクトをデスポーンします。
        /// </summary>
        void ImpactDespawn()
        {
            this.enabled = false; // 同一プールオブジェクトの他用途で干渉しないよう、スクリプトを無効化
            EmeraldObjectPool.Despawn(gameObject);
        }
    }
}
