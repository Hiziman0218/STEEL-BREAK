using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;
using UnityEngine.Audio;

namespace EmeraldAI
{
    [RequireComponent(typeof(AudioSource))]
    public class BulletProjectile : MonoBehaviour, IAvoidable
    {
        #region 変数
        [Header("現在のアビリティデータ（BulletProjectileAbility）")]
        BulletProjectileAbility CurrentAbilityData;

        [Header("所有者の EmeraldSystem 参照")]
        EmeraldSystem EmeraldComponent;

        [Header("現在のターゲット（Transform）")]
        Transform CurrentTarget;

        public Transform AbilityTarget { get => CurrentTarget; set => CurrentTarget = value; }

        [Header("初期ターゲット位置（ダメージ位置のキャッシュ）")]
        Vector3 InitialTargetPosition;

        [Header("ターゲット側の ICombat 参照（ダメージ位置取得に使用）")]
        ICombat StartingICombat;

        [Header("自分自身の AudioSource 参照")]
        AudioSource m_AudioSource;

        [Header("移動音などのサウンドエフェクト（Resources からロード）")]
        GameObject m_SoundEffect;

        [Header("このプロジェクタイルの所有者（発射元）")]
        GameObject Owner;

        [Header("衝突判定を無視するコライダー一覧（LBD 内部コライダー等）")]
        List<Collider> IgnoredColliders = new List<Collider>();

        [Header("発射（初期化）された時間（秒）")]
        float StartTime;

        [Header("初期化が完了したか（内部フラグ）")]
        bool Initialized;

        [Header("ターゲットとの角度（内部参照用）")]
        float TargetAngle;
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
            m_SoundEffect = Resources.Load("Emerald Sound") as GameObject;
        }

        /// <summary>
        /// 渡された情報でプロジェクタイルを初期化します。
        /// </summary>
        /// <param name="owner">このプロジェクタイルの所有者。</param>
        /// <param name="currentTarget">このプロジェクタイルの現在のターゲット。</param>
        /// <param name="abilityData">このプロジェクタイルのアビリティデータ。</param>
        public void Initialize(GameObject owner, Transform currentTarget, BulletProjectileAbility abilityData)
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

            GetLBDColliders(); // Owner の LBD コンポーネントを参照し、内部コライダーを無視対象に設定する。

            Initialized = false;
            InitializeProjectile(); // プロジェクタイルの設定を初期化。

            StartTime = Time.time;
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
            if (EmeraldComponent.CombatComponent.TargetAngle < EmeraldComponent.MovementComponent.CombatAngleToTurn) transform.LookAt(InitialTargetPosition);
            Initialized = true;
            StartCoroutine(MoveProjectile());
        }

        /// <summary>
        /// 弾丸の移動と、一定間隔での衝突判定（Raycast）を行います。
        /// </summary>
        IEnumerator MoveProjectile()
        {
            bool Collided = false;
            float collisionCheckTimer = 0;
            Vector3 EndPosition = Vector3.zero;

            // BulletSpread 設定に基づくランダムな拡散（精度のばらつき）を付与
            Vector3 Accuracy = new Vector3(
                Random.Range(-CurrentAbilityData.BulletProjectileSettings.BulletSpreadX, CurrentAbilityData.BulletProjectileSettings.BulletSpreadX) * 0.001f,
                Random.Range(-CurrentAbilityData.BulletProjectileSettings.BulletSpreadY, CurrentAbilityData.BulletProjectileSettings.BulletSpreadY) * 0.001f,
                0);

            Vector3 lastCheckPosition = transform.position;

            while (!Collided)
            {
                // 毎フレーム弾丸を移動
                float step = CurrentAbilityData.BulletProjectileSettings.BulletSpeed * Time.deltaTime;
                Vector3 nextPosition = Vector3.MoveTowards(
                    transform.position,
                    transform.position + (transform.forward + Accuracy),
                    step);

                transform.position = nextPosition;

                collisionCheckTimer += Time.deltaTime;

                // 一定時間ごとに単発の衝突チェックを行う
                if (collisionCheckTimer >= CurrentAbilityData.BulletProjectileSettings.CollisionCheckSpeed)
                {
                    // 直前チェック位置から現在位置までの Raycast を実行
                    Vector3 travelVector = transform.position - lastCheckPosition;
                    float travelDistance = travelVector.magnitude;

                    RaycastHit hit;
                    if (Physics.Raycast(lastCheckPosition, travelVector.normalized, out hit, travelDistance, ~CurrentAbilityData.BulletProjectileSettings.IgnoreLayers))
                    {
                        EndPosition = hit.point + (transform.forward * 0.1f);
                        Impact(hit.collider.gameObject, hit.point, hit.normal);
                        Collided = true;
                    }

                    // タイマをリセットし、基準位置を更新
                    collisionCheckTimer = 0f;
                    lastCheckPosition = transform.position;
                }

                // 衝突があれば移動終了
                if (Collided) break;

                // なければ次フレームへ
                yield return null;
            }

            // 最後のわずかな距離を移動して、Trail などが衝突点まで到達できるようにする
            Vector3 startingPosition = transform.position;
            float t = 0f;
            bool complete = false;

            while (!complete)
            {
                float distance = Vector3.Distance(transform.position, EndPosition);
                t += Time.deltaTime * CurrentAbilityData.BulletProjectileSettings.BulletSpeed;
                transform.position = Vector3.Lerp(startingPosition, EndPosition, t);

                if (distance <= 0)
                    complete = true;

                yield return null;
            }
        }

        /// <summary>
        /// プロジェクタイル生成後の経過時間を追跡します。
        /// </summary>
        void Update()
        {
            ProjectileTimeout(); // アクティブになってからの時間を追跡
        }

        /// <summary>
        /// アクティブからの時間を追跡し、BulletObjectTimeout に達したらデスポーンします。
        /// </summary>
        void ProjectileTimeout()
        {
            if (!Initialized) return;

            float TimeAlive = Time.time - StartTime;
            if (TimeAlive > CurrentAbilityData.BulletProjectileSettings.BulletObjectTimeout)
            {
                this.enabled = false;
                EmeraldObjectPool.Despawn(gameObject);
            }
        }

        /// <summary>
        /// 衝突時の処理（位置・法線を用いた着弾演出の再生、ダメージ、デスポーン予約）。
        /// </summary>
        void Impact(GameObject TargetHit, Vector3 HitPosition, Vector3 HitNormal)
        {
            if (!this.enabled) return; // スクリプトが有効なときのみ衝突判定を許可

            Initialized = false; // 初期化フラグを下ろしてプロジェクタイルの動作を停止
            BulletImpact(TargetHit, HitPosition, HitNormal);
            DamageTarget(TargetHit); // ヒット対象にダメージを与える（LBD があれば部位ダメージ、なければ IDamageable へ）
            Invoke(nameof(ImpactDespawn), CurrentAbilityData.BulletProjectileSettings.BulletCollisionTimeout);
        }

        /// <summary>
        /// 対象の表面タグに応じて適切な着弾エフェクトを再生（該当がなければデフォルト）。
        /// </summary>
        void BulletImpact(GameObject TargetHit, Vector3 HitPosition, Vector3 HitNormal)
        {
            if (CurrentAbilityData.BulletProjectileSettings.BulletImpactData.Count == 0)
            {
                CurrentAbilityData.BulletProjectileSettings.SpawnDefaultBulletImpact(TargetHit, HitPosition, HitNormal);
                return;
            }

            for (int i = 0; i < CurrentAbilityData.BulletProjectileSettings.BulletImpactData.Count; i++)
            {
                if (TargetHit.CompareTag(CurrentAbilityData.BulletProjectileSettings.BulletImpactData[i].SurfaceTag))
                {
                    CurrentAbilityData.BulletProjectileSettings.SpawnBulletImpact(TargetHit, CurrentAbilityData.BulletProjectileSettings.BulletImpactData[i], HitPosition, HitNormal);
                    return;
                }
            }

            CurrentAbilityData.BulletProjectileSettings.SpawnDefaultBulletImpact(TargetHit, HitPosition, HitNormal);
        }

        /// <summary>
        /// このコンポーネントが無効化されたとき、進行中のコルーチンを停止します。
        /// </summary>
        void OnDisable()
        {
            StopAllCoroutines();
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
        }

        /// <summary>
        /// BulletCollisionTimeout 経過後に着弾エフェクトをデスポーンします。
        /// </summary>
        void ImpactDespawn()
        {
            this.enabled = false; // 同一プールオブジェクトの他用途で干渉しないよう、スクリプトを無効化
            EmeraldObjectPool.Despawn(gameObject);
        }
    }
}
