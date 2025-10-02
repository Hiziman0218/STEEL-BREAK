using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Rigidbody))]
    public class GroundProjectile : MonoBehaviour, IAvoidable
    {
        #region 変数
        [Header("現在のアビリティデータ（GroundProjectileAbility）")]
        GroundProjectileAbility CurrentAbilityData;

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

        [Header("ホーミング経過時間（秒）")]
        float HomingTimer;

        [Header("初期化が完了したか（内部フラグ）")]
        bool Initialized;

        [Header("移動距離（開始位置からの距離）")]
        float TravelDistance;

        [Header("発射時の開始位置（距離計測用）")]
        Vector3 StartingPosition;

        [Header("ホーミングの最小距離に到達したか（内部フラグ）")]
        bool MinHomingDistMet;

        [Header("現在の接地面の法線（Surface Normal）")]
        Vector3 SurfaceNormal;

        [Header("目標方向への回転（内部用）")]
        Quaternion qTarget;

        [Header("地面方向への回転（内部用）")]
        Quaternion qGround;

        [Header("ターゲット方向ベクトル（水平面に投影）")]
        Vector3 TargetDirection;

        [Header("プロジェクタイルに関連する見た目/エフェクトのキャッシュ")]
        public List<ProjectileEffectsClass> m_ProjectileObjects = new List<ProjectileEffectsClass>();
        #endregion

        /// <summary>
        /// 【Awake】
        /// プロジェクタイルを最初に使用するときに、必要なコンポーネントや設定を初期化します。
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
        /// 【Initialize】
        /// 渡された情報でプロジェクタイルを初期化します。
        /// </summary>
        /// <param name="owner">このプロジェクタイルの所有者。</param>
        /// <param name="currentTarget">このプロジェクタイルの現在のターゲット。</param>
        /// <param name="abilityData">このプロジェクタイルのアビリティデータ。</param>
        public void Initialize(GameObject owner, Transform currentTarget, GroundProjectileAbility abilityData)
        {
            StartCoroutine(InitializeInternal(owner, currentTarget, abilityData));
        }

        IEnumerator InitializeInternal(GameObject owner, Transform currentTarget, GroundProjectileAbility abilityData)
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
            m_Collider.enabled = true;

            // Owner の LBD コンポーネントを取得し、内部コライダーを無視対象に設定する。
            LocationBasedDamage LBDComponent = Owner.GetComponent<LocationBasedDamage>();
            if (LBDComponent)
            {
                IgnoredColliders.Clear();
                for (int i = 0; i < LBDComponent.ColliderList.Count; i++)
                {
                    IgnoredColliders.Add(LBDComponent.ColliderList[i].ColliderObject);
                }
            }

            if (m_SphereCollider != null)
            {
                m_SphereCollider.radius = abilityData.ColliderSettings.ColliderRadius;
                m_SphereCollider.center = Vector3.forward * abilityData.ColliderSettings.ZOffet;
            }

            gameObject.layer = CurrentAbilityData.ColliderSettings.ProjectileLayer;
            CurrentAbilityData.GroundProjectileSettings.AllignmentLayers &= ~(1 << CurrentAbilityData.ColliderSettings.ProjectileLayer); // 誤って ProjectileLayer が AllignmentLayers に含まれている場合は除外する
            Initialized = false;
            MinHomingDistMet = false;
            HomingTimer = 0;
            StartTime = Time.time;

            // 回転とターゲット方向の変数をリセット（未リセットだと初期の進行方向が不正になるため）
            TargetDirection = Vector3.zero;
            qGround = new Quaternion();
            qTarget = new Quaternion();

            SetEffectsState(true); // EffectsToDisable の名前と一致するエフェクトを有効化
            InitializeProjectile(); // プロジェクタイルの初期設定

            yield return new WaitForSeconds(CurrentAbilityData.ProjectileSettings.LaunchProjectileDelay);

            if (CurrentAbilityData.ProjectileSettings.TravelSound != null) m_AudioSource.PlayOneShot(CurrentAbilityData.ProjectileSettings.TravelSound);
            CurrentAbilityData.ProjectileSettings.SpawnLaunchProjectileEffect(Owner, transform.position);
            Initialized = true;
        }

        void InitializeProjectile()
        {
            CurrentAbilityData.ProjectileSettings.SpawnEffect(Owner, transform.position);
            StartingPosition = transform.position;
        }

        /// <summary>
        /// 【Update】
        /// プロジェクタイル生成後の経過時間を追跡し、移動処理を行います。
        /// </summary>
        void Update()
        {
            ProjectileTimeout(); // アクティブになってからの時間を追跡
            MoveGroundProjectile(); // 現在のターゲット方向へ移動
        }

        /// <summary>
        /// 【ProjectileTimeout】
        /// アクティブからの時間を追跡し、ProjectileTimeoutSeconds に達したら
        /// インパクトエフェクトを生成してデスポーンします。
        /// </summary>
        void ProjectileTimeout()
        {
            float TimeAlive = Time.time - StartTime;
            if (TimeAlive > CurrentAbilityData.ProjectileSettings.ProjectileTimeoutSeconds && m_Collider.enabled)
            {
                StopGroundProjectile();
            }
        }

        /// <summary>
        /// 【MoveGroundProjectile】
        /// アビリティが初期化済みのときに、地形になじませながらターゲットへ移動させます。
        /// </summary>
        void MoveGroundProjectile()
        {
            if (Initialized)
            {
                TravelDistance = Vector3.Distance(StartingPosition, transform.position);

                if (TravelDistance < CurrentAbilityData.GroundProjectileSettings.MaxTravelDistance || CurrentTarget == null)
                {
                    var step = CurrentAbilityData.GroundProjectileSettings.ProjectileSpeed * Time.deltaTime;
                    float DistFromTarget = (Vector3.Distance(transform.position, StartingICombat.DamagePosition()));
                    float DistFormOwner = Vector3.Distance(Owner.transform.position, transform.position);
                    if (!MinHomingDistMet && DistFromTarget < CurrentAbilityData.HomingSettings.MinimumHomingDistance) MinHomingDistMet = true;

                    if (CurrentAbilityData.HomingSettings.Enabled && HomingTimer < CurrentAbilityData.HomingSettings.HomingSeconds && !MinHomingDistMet && CurrentTarget.transform.localScale != Vector3.one * 0.003f)
                    {
                        if (DistFormOwner > 2f || DistFromTarget < 1)
                        {
                            HomingTimer += Time.deltaTime;
                            GetSurfaceNormal();
                            TargetDirection = StartingICombat.DamagePosition() - transform.position;
                            TargetDirection.y = 0;
                            SetMovementAndRotation(step);
                        }
                        else
                        {
                            GetSurfaceNormal();
                            SetMovementAndRotation(step);
                        }
                    }
                    else
                    {
                        GetSurfaceNormal();
                        SetMovementAndRotation(step);
                    }
                }
                else
                {
                    StopGroundProjectile();
                }
            }
        }

        /// <summary>
        /// 【SetMovementAndRotation】
        /// 地形の法線に合わせた回転と、必要に応じた目標方向への回転を適用しつつ前進させます。
        /// ターゲット方向はホーミング有効時のみ使用されます。
        /// </summary>
        void SetMovementAndRotation(float step)
        {
            qGround = Quaternion.Slerp(qGround, Quaternion.FromToRotation(Vector3.up, SurfaceNormal), Time.deltaTime * CurrentAbilityData.GroundProjectileSettings.AlignmentSpeed);
            if (TargetDirection != Vector3.zero) qTarget = Quaternion.Slerp(qTarget, Quaternion.LookRotation(TargetDirection, Vector3.up), Time.deltaTime * CurrentAbilityData.HomingSettings.HomingSpeed);
            transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, step);
            transform.rotation = Quaternion.Slerp(transform.rotation, qGround * qTarget, Time.deltaTime * CurrentAbilityData.GroundProjectileSettings.AlignmentSpeed);

            float SurfaceAngle = Vector3.Angle(SurfaceNormal, Vector3.up);
            if (SurfaceAngle >= CurrentAbilityData.GroundProjectileSettings.KillAngle) StopGroundProjectile();
        }

        /// <summary>
        /// 【GetSurfaceNormal】
        /// プロジェクタイル中心からレイを飛ばし、現在の接地面の法線を取得して整形します。
        /// </summary>
        public Vector3 GetSurfaceNormal()
        {
            RaycastHit HitDown;
            if (Physics.Raycast(transform.position + Vector3.up, -Vector3.up, out HitDown, 2f, CurrentAbilityData.GroundProjectileSettings.AllignmentLayers))
            {
                if (HitDown.transform != this.transform)
                {
                    float MaxNormalAngle = CurrentAbilityData.GroundProjectileSettings.MaxAlignmentAngle * 0.01f;
                    transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, HitDown.point.y + CurrentAbilityData.GroundProjectileSettings.HeightOffset, Time.deltaTime * 20), transform.position.z);
                    SurfaceNormal = HitDown.normal;
                    SurfaceNormal.x = Mathf.Clamp(SurfaceNormal.x, -MaxNormalAngle, MaxNormalAngle);
                    SurfaceNormal.z = Mathf.Clamp(SurfaceNormal.z, -MaxNormalAngle, MaxNormalAngle);
                }
            }
            else
            {
                // 地面が検出できない場合は停止（崖やエッジ上での移動を防止するため）
                StopGroundProjectile();
            }

            RaycastHit HitForward;
            if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), transform.forward, out HitForward, CurrentAbilityData.ColliderSettings.ColliderRadius, CurrentAbilityData.GroundProjectileSettings.AllignmentLayers))
            {
                if (HitForward.transform != this.transform)
                {
                    float SurfaceAngle = Vector3.Angle(HitForward.normal, Vector3.up);
                    if (SurfaceAngle >= CurrentAbilityData.GroundProjectileSettings.KillAngle)
                    {
                        //CurrentAbilityData.ProjectileSettings.SpawnImpactEffect(Owner, transform.position); // インパクトエフェクトとサウンドを再生
                        StopGroundProjectile();
                    }
                }
            }

            return SurfaceNormal;
        }

        /// <summary>
        /// 【OnTriggerEnter】
        /// 自分自身や他のプロジェクタイル以外との接触時に、着弾処理を行います。
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & CurrentAbilityData.ColliderSettings.CollidableLayers) != 0 && other.gameObject != Owner && !IgnoredColliders.Contains(other)) Impact(other.gameObject);
        }

        /// <summary>
        /// 【Impact】
        /// 衝突時の一連の処理を実行します。
        /// </summary>
        void Impact(GameObject TargetHit)
        {
            m_Collider.enabled = false; // 予期しない衝突を防ぐため、コライダーを無効化
            Initialized = false; // 初期化フラグを下ろしてプロジェクタイルの動作を停止
            CurrentAbilityData.ProjectileSettings.SpawnImpactEffect(Owner, transform.position); // インパクトエフェクトとサウンドを再生
            DamageTarget(TargetHit); // ヒット対象にダメージを与える（LBD があれば部位ダメージ、なければ IDamageable へ）
            Invoke(nameof(ImpactDespawn), CurrentAbilityData.ColliderSettings.CollisionTimeout); // CollisionTimeout 後にデスポーン（演出終了猶予）
            SetEffectsState(false); // EffectsToDisable の名前と一致するエフェクトを無効化
        }

        void StopGroundProjectile()
        {
            m_Collider.enabled = false; // 予期しない衝突を防ぐため、コライダーを無効化
            Initialized = false; // 初期化フラグを下ろしてプロジェクタイルの動作を停止
            m_AudioSource.Stop();
            Invoke(nameof(ImpactDespawn), CurrentAbilityData.ColliderSettings.CollisionTimeout); // CollisionTimeout 後にデスポーン（演出終了猶予）
            SetEffectsState(false); // EffectsToDisable の名前と一致するエフェクトを無効化
        }

        /// <summary>
        /// 【DamageTarget】
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

            // ノックバックが有効なら、確率でノックバック
            if (CurrentAbilityData.KnockbackSettings.Enabled && CurrentAbilityData.KnockbackSettings.RollForKnockback())
            {
                Vector3 Direction = transform.forward;
                if (m_ICombat != null) Owner.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(CurrentAbilityData.KnockbackSettings.KnockbackSequence(Direction, m_ICombat.TargetTransform(), m_ICombat));
            }

            // スタンが有効なら、確率でスタン
            if (CurrentAbilityData.StunnedSettings.Enabled && CurrentAbilityData.StunnedSettings.RollForStun())
            {
                if (m_ICombat != null) m_ICombat.TriggerStun(CurrentAbilityData.StunnedSettings.StunLength);
            }

            // ダメージが無効化されている場合は終了
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
        }

        /// <summary>
        /// 【SetEffectsState】
        /// EffectsToDisable の名前と一致するエフェクトの有効/無効を切り替えます。
        /// </summary>
        void SetEffectsState(bool State)
        {
            if (CurrentAbilityData.ProjectileSettings.EffectsToDisable.Count > 0)
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
        }

        /// <summary>
        /// 【ImpactDespawn】
        /// CollisionTimeout に達した後、インパクトエフェクトをデスポーンします。
        /// </summary>
        void ImpactDespawn()
        {
            EmeraldObjectPool.Despawn(gameObject);
        }
    }
}
