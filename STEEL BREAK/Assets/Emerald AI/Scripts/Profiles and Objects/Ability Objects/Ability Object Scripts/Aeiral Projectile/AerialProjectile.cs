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
    public class AerialProjectile : MonoBehaviour, IAvoidable
    {
        #region 変数
        [Header("初期化が完了したか（内部フラグ）")]
        bool Initialized;

        [Header("ホーミングの最小距離に到達したか（内部フラグ）")]
        bool MinHomingDistMet;

        [Header("現在のアビリティデータ（AerialProjectileAbility）")]
        AerialProjectileAbility CurrentAbilityData;

        [Header("現在のターゲット（Transform）")]
        Transform CurrentTarget;

        public Transform AbilityTarget { get => CurrentTarget; set => CurrentTarget = value; }

        [Header("初期ターゲットの位置（ダメージ位置をキャッシュ）")]
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

        [Header("衝突判定を無視するコライダー一覧（LBD 内部コライダー等）")]
        List<Collider> IgnoredColliders = new List<Collider>();

        [Header("自分自身の Rigidbody 参照")]
        Rigidbody m_Rigidbody;

        [Header("このプロジェクタイルの所有者（発射元）")]
        GameObject Owner;

        [Header("発射（初期化）された時間（秒）")]
        float StartTime;

        [Header("ホーミング経過時間（秒）")]
        float HomingTimer;

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

            // すでにコライダーが付いている場合はそれを使用する
            m_Collider = GetComponent<Collider>();
            if (m_Collider != null)
            {
                m_Collider.isTrigger = true;
                m_Collider.enabled = false;
            }
            // 付いていない場合は SphereCollider を生成する
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

            // Awake 時に全エフェクトオブジェクトを走査し、Projectile Module で使用できるようキャッシュする
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
        public void Initialize(GameObject owner, Transform currentTarget, AerialProjectileAbility abilityData)
        {
            StartCoroutine(InitializeInternal(owner, currentTarget, abilityData));
        }

        IEnumerator InitializeInternal(GameObject owner, Transform currentTarget, AerialProjectileAbility abilityData)
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

            // Owner の LBD コンポーネントを取得し、内部コライダーを無視対象に設定する
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
            Initialized = false;
            MinHomingDistMet = false;
            HomingTimer = 0;

            if (CurrentAbilityData.ProjectileSettings.EffectsToDisable.Count > 0) SetEffectsState(true); // EffectsToDisable の名前と一致するエフェクトを有効化
            InitializeAerialProjectile();

            yield return new WaitForSeconds(CurrentAbilityData.ProjectileSettings.LaunchProjectileDelay);

            if (CurrentTarget != null && CurrentAbilityData.AerialProjectileSettings.AimSource == AbilityData.AerialProjectileData.AimSources.TargetPosition && CurrentTarget.transform.localScale != Vector3.one * 0.003f)
                transform.LookAt(StartingICombat.DamagePosition());

            StartTime = Time.time;
            if (CurrentAbilityData.ProjectileSettings.TravelSound != null) m_AudioSource.PlayOneShot(CurrentAbilityData.ProjectileSettings.TravelSound);
            CurrentAbilityData.ProjectileSettings.SpawnLaunchProjectileEffect(Owner, transform.position);
            Initialized = true;
        }

        void InitializeAerialProjectile()
        {
            // プロジェクタイル用の目的地を、現在位置の周囲でランダムに（角度もランダムで）生成
            if (CurrentAbilityData.AerialProjectileSettings.Enabled)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 20))
                {
                    InitialTargetPosition = hit.point;
                }
            }

            if (CurrentAbilityData.AerialProjectileSettings.Enabled && CurrentAbilityData.AerialProjectileSettings.AimSource == EmeraldAI.AbilityData.AerialProjectileData.AimSources.TargetPosition)
            {
                transform.LookAt(StartingICombat.DamagePosition());
            }
            else
            {
                /*
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 10, Color.red, 5);

                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 30))
                {
                    transform.LookAt(hit.point);
                }
                */
            }

            CurrentAbilityData.ProjectileSettings.SpawnEffect(Owner, transform.position);
        }

        /// <summary>
        /// プロジェクタイルが生成されてからの時間の追跡と移動処理を行います。
        /// </summary>
        void Update()
        {
            ProjectileTimeout(); // アクティブになってからの時間を追跡
            MoveProjectile(); // 現在のターゲット方向へ移動
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
            if (!Initialized) return;

            var step = CurrentAbilityData.AerialProjectileSettings.ProjectileSpeed * Time.deltaTime;

            if (CurrentTarget != null)
            {
                float DistFromTarget = (Vector3.Distance(transform.position, StartingICombat.DamagePosition()));

                if (!MinHomingDistMet && DistFromTarget < CurrentAbilityData.HomingSettings.MinimumHomingDistance) MinHomingDistMet = true;

                if (CurrentAbilityData.HomingSettings.Enabled && HomingTimer < CurrentAbilityData.HomingSettings.HomingSeconds && !MinHomingDistMet)
                {
                    Vector3 ForwardMovement = Vector3.MoveTowards(transform.position, transform.position + transform.forward, step);
                    transform.position = ForwardMovement;
                    float DistFormOwner = Vector3.Distance(Owner.transform.position, transform.position);

                    if (DistFormOwner > 1f || DistFromTarget < 1)
                    {
                        HomingTimer += Time.deltaTime;
                        RotateTowardsTarget();
                    }
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, step);
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, step);
            }
        }

        /// <summary>
        /// 現在のターゲットの位置へ向けて回転させます。
        /// </summary>
        void RotateTowardsTarget()
        {
            // 回転すべき方向を算出
            Vector3 targetDirection = StartingICombat.DamagePosition() - transform.position;

            // ステップサイズ（= 速度 × フレーム時間）
            float singleStep = Time.deltaTime * CurrentAbilityData.HomingSettings.HomingSpeed;

            // forward ベクトルを、ターゲット方向へ 1 ステップ分だけ回転
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

            // 1 ステップ分近づいた回転を適用
            transform.rotation = Quaternion.LookRotation(newDirection);
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
            m_Collider.enabled = false; // 予期しない衝突を防ぐため、コライダーを無効化
            Initialized = false; // 初期化フラグを下ろしてプロジェクタイルの動作を停止
            CurrentAbilityData.ProjectileSettings.SpawnImpactEffect(Owner, transform.position); // インパクトエフェクトとサウンドを再生

            DamageTarget(TargetHit); // ヒット対象にダメージを与える（LBD があれば部位ダメージ、なければ IDamageable へ）
            Invoke(nameof(ImpactDespawn), CurrentAbilityData.ColliderSettings.CollisionTimeout); // CollisionTimeout 後にデスポーン（演出終了猶予）

            if (CurrentAbilityData.ProjectileSettings.EffectsToDisable.Count > 0)
            {
                if (!CurrentAbilityData.AerialProjectileSettings.AttachToTarget) SetEffectsState(false); // EffectsToDisable の名前と一致するエフェクトを無効化
                else if (TargetHit.activeSelf) StartCoroutine(SetEffectsStateDelay(false)); // （遅延ありで）エフェクトを無効化
            }
        }

        /// <summary>
        /// SetEffectsState 呼び出しを遅延させます。
        /// </summary>
        IEnumerator SetEffectsStateDelay(bool State)
        {
            if (!State) yield return new WaitForSeconds(0.5f);
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

            // AI の DetectionLayerMask に含まれるレイヤー、または対象に LBD がある場合のみダメージ可
            if (!m_LocationBasedDamageArea && ((1 << Target.layer) & EmeraldComponent.DetectionComponent.DetectionLayerMask) == 0) return;

            // テレポート中の対象は除外
            if (m_LocationBasedDamageArea != null && m_LocationBasedDamageArea.EmeraldComponent.transform.localScale == Vector3.one * 0.003f || Target.transform.localScale == Vector3.one * 0.003f) return;

            var m_ICombat = Target.GetComponentInParent<ICombat>();

            // フレンドリー（味方）へのダメージやスタンは不可
            bool IsFriendlyTarget = EmeraldAPI.Faction.GetTargetFactionName(m_ICombat.TargetTransform()) == EmeraldAPI.Faction.GetTargetFactionName(EmeraldComponent.transform) || EmeraldAPI.Faction.GetTargetFactionRelation(EmeraldComponent, m_ICombat.TargetTransform()) == "Friendly";
            if (!IsFriendlyTarget)
            {
                // スタンが有効なら、確率でスタンを与える
                if (CurrentAbilityData.StunnedSettings.Enabled && CurrentAbilityData.StunnedSettings.RollForStun())
                {
                    if (m_ICombat != null) m_ICombat.TriggerStun(CurrentAbilityData.StunnedSettings.StunLength);
                }

                // ダメージが無効化されていれば終了
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

            if (CurrentAbilityData.AerialProjectileSettings.AttachToTarget)
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
