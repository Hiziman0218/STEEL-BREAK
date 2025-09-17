using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【EmeraldCombatManager】
    /// Emerald AI の戦闘に関する全機能を扱う静的マネージャ。
    /// </summary>
    public static class EmeraldCombatManager
    {
        /// <summary>
        /// すべての攻撃（アビリティ/攻撃アニメ）を生成する処理を行います。
        /// </summary>
        public static void GenerateAttack(EmeraldSystem EmeraldComponent, AttackClass SentAttackClass, bool OverridePickType = false)
        {
            if (SentAttackClass.AttackDataList.Count > 0)
            {
                List<AttackClass.AttackData> AvailableAttacks = new List<AttackClass.AttackData>();

                // 攻撃リストを走査して、クールダウンが完了している攻撃を収集します。
                // 注: Ordered（順番）方式の場合、この判定は無視されます。
                for (int i = 0; i < SentAttackClass.AttackDataList.Count; i++)
                {
                    SentAttackClass.AttackDataList[i].CooldownIgnored = false; // クールダウン再チェック前にリセット

                    float CooldownTime = 0;
                    if (SentAttackClass.AttackDataList[i].AbilityObject != null) CooldownTime = (SentAttackClass.AttackDataList[i].CooldownTimeStamp + SentAttackClass.AttackDataList[i].AbilityObject.CooldownSettings.CooldownLength);

                    if (Time.time >= CooldownTime || SentAttackClass.AttackDataList[i].CooldownTimeStamp == 0 || SentAttackClass.AttackDataList[i].AbilityObject != null && !SentAttackClass.AttackDataList[i].AbilityObject.CooldownSettings.Enabled)
                    {
                        if (!AvailableAttacks.Contains(SentAttackClass.AttackDataList[i]))
                        {
                            if (SentAttackClass.AttackDataList[i].AbilityObject == null)
                            {
                                AvailableAttacks.Add(SentAttackClass.AttackDataList[i]);
                            }
                            else if (!SentAttackClass.AttackDataList[i].AbilityObject.ConditionSettings.Enabled ||
                                SentAttackClass.AttackDataList[i].AbilityObject.ConditionSettings.Enabled && !SentAttackClass.AttackDataList[i].AbilityObject.ConditionSettings.HighPriority && CheckAbilityConditions(EmeraldComponent, SentAttackClass.AttackDataList[i]))
                            {
                                AvailableAttacks.Add(SentAttackClass.AttackDataList[i]);
                            }
                        }
                    }
                }

                // 利用可能な攻撃が1つもない場合は、エラー回避のためにクールダウンを無視してランダムな攻撃を1つ選びます。
                if (AvailableAttacks.Count == 0)
                {
                    SetAttackValues(EmeraldComponent, SentAttackClass.AttackDataList[Random.Range(0, SentAttackClass.AttackDataList.Count)], true);
                    Debug.Log("AI '" + EmeraldComponent.gameObject.name + "' の全攻撃がクールダウン中のため、攻撃リストからランダムな攻撃を使用しました。これを避けるには、攻撃の種類を増やすか、アビリティのクールダウン時間を短くしてください。");
                    return;
                }

                if (SentAttackClass.AttackPickType == AttackPickTypes.Odds) // 利用可能な各攻撃の抽選率（重み）を用いて選択
                {
                    List<float> OddsList = new List<float>();
                    for (int i = 0; i < AvailableAttacks.Count; i++)
                    {
                        OddsList.Add(AvailableAttacks[i].AttackOdds);
                    }
                    int OddsIndex = (int)GenerateProbability(OddsList.ToArray());
                    SetAttackValues(EmeraldComponent, AvailableAttacks[OddsIndex], false);
                }
                else if (SentAttackClass.AttackPickType == AttackPickTypes.Order) // リストの順番通りに選択
                {
                    float CooldownTime = 0;
                    AttackClass.AttackData AbilityDataRef = SentAttackClass.AttackDataList[SentAttackClass.AttackListIndex];

                    if (AbilityDataRef.AbilityObject == null || AbilityDataRef.AbilityObject != null && !AbilityDataRef.AbilityObject.CooldownSettings.Enabled && !AbilityDataRef.AbilityObject.ConditionSettings.Enabled)
                    {
                        SetAttackValues(EmeraldComponent, SentAttackClass.AttackDataList[SentAttackClass.AttackListIndex], false);
                        SentAttackClass.AttackListIndex++;
                        if (SentAttackClass.AttackListIndex == SentAttackClass.AttackDataList.Count) SentAttackClass.AttackListIndex = 0;
                    }
                    else
                    {
                        // 次に利用可能な攻撃を見つけるためのループを開始
                        bool attackFound = false;
                        while (!attackFound)
                        {
                            AbilityDataRef = SentAttackClass.AttackDataList[SentAttackClass.AttackListIndex];
                            if (AbilityDataRef.AbilityObject != null)
                                CooldownTime = (AbilityDataRef.CooldownTimeStamp + AbilityDataRef.AbilityObject.CooldownSettings.CooldownLength);

                            // AbilityObject が存在しない場合は、攻撃アニメーションを再生
                            if (AbilityDataRef.AbilityObject == null)
                            {
                                SetAttackValues(EmeraldComponent, AbilityDataRef, false);
                                SentAttackClass.AttackListIndex++;
                                if (SentAttackClass.AttackListIndex == SentAttackClass.AttackDataList.Count) SentAttackClass.AttackListIndex = 0;
                                attackFound = true;
                            }
                            else if (Time.time >= CooldownTime && !AbilityDataRef.AbilityObject.ConditionSettings.Enabled || AbilityDataRef.CooldownTimeStamp == 0) // クールダウン条件をチェック
                            {
                                // Condition Module の条件をチェック
                                if (!AbilityDataRef.AbilityObject.ConditionSettings.Enabled || AbilityDataRef.AbilityObject.ConditionSettings.Enabled && !AbilityDataRef.AbilityObject.ConditionSettings.HighPriority && CheckAbilityConditions(EmeraldComponent, AbilityDataRef))
                                {
                                    // アビリティが使用可能なら、攻撃値をセットしてループを抜ける
                                    SetAttackValues(EmeraldComponent, AbilityDataRef, false);
                                    SentAttackClass.AttackListIndex++;
                                    if (SentAttackClass.AttackListIndex == SentAttackClass.AttackDataList.Count) SentAttackClass.AttackListIndex = 0;
                                    attackFound = true;
                                }
                                else
                                {
                                    // 使用不可なら次の攻撃へ
                                    SentAttackClass.AttackListIndex++;
                                    if (SentAttackClass.AttackListIndex == SentAttackClass.AttackDataList.Count) SentAttackClass.AttackListIndex = 0;
                                }
                            }
                            else
                            {
                                // 使用不可なら次の攻撃へ
                                SentAttackClass.AttackListIndex++;
                                if (SentAttackClass.AttackListIndex == SentAttackClass.AttackDataList.Count) SentAttackClass.AttackListIndex = 0;
                            }
                        }
                    }
                }
                else if (SentAttackClass.AttackPickType == AttackPickTypes.Random) // リストからランダム選択
                {
                    int RandomIndex = Random.Range(0, AvailableAttacks.Count);
                    SetAttackValues(EmeraldComponent, AvailableAttacks[RandomIndex], false);
                }

                // 現在の選択方式を上書きして最も近い攻撃を選択します。
                // 現状、これはAIの攻撃リストからランダム攻撃を強制するために使用されています。
                if (OverridePickType)
                {
                    int RandomIndex = Random.Range(0, AvailableAttacks.Count);
                    SetAttackValues(EmeraldComponent, AvailableAttacks[RandomIndex], false);
                }

                CheckConditionalAbiitities(EmeraldComponent, SentAttackClass, OverridePickType);
            }
        }

        /// <summary>
        /// 条件付きアビリティをチェックし、条件を満たすものを AvailableConditionAbilities に追加します。
        /// </summary>
        public static void CheckConditionalAbiitities(EmeraldSystem EmeraldComponent, AttackClass SentAttackClass, bool OverridePickType = false)
        {
            EmeraldComponent.CombatComponent.AvailableConditionAbilities.Clear();

            foreach (var attackData in SentAttackClass.AttackDataList)
            {
                // Condition Module を使用しているアビリティのみ対象
                if (attackData.AbilityObject != null && attackData.AbilityObject.ConditionSettings.Enabled)
                {
                    // クールダウン中でないもののみ対象
                    if ((Time.time >= attackData.CooldownTimeStamp + attackData.AbilityObject.CooldownSettings.CooldownLength || attackData.CooldownTimeStamp == 0))
                    {
                        // 最後に、条件チェックをパスしたものを AvailableConditionAbilities に追加
                        if (CheckAbilityConditions(EmeraldComponent, attackData)) EmeraldComponent.CombatComponent.AvailableConditionAbilities.Add(attackData);
                    }
                }
            }

            if (EmeraldComponent.CombatComponent.AvailableConditionAbilities.Count > 0)
            {
                EmeraldComponent.CombatComponent.CancelAllCombatActions();
                EmeraldComponent.CombatComponent.AdjustCooldowns();
                int RandomUseableAbility = Random.Range(0, EmeraldComponent.CombatComponent.AvailableConditionAbilities.Count);
                SetAttackValues(EmeraldComponent, EmeraldComponent.CombatComponent.AvailableConditionAbilities[RandomUseableAbility], false);
            }
        }

        /// <summary>
        /// 渡されたアビリティについて、各条件をチェックして使用可能か判定します。
        /// </summary>
        static bool CheckAbilityConditions(EmeraldSystem EmeraldComponent, AttackClass.AttackData AttackData)
        {
            if (AttackData.AbilityObject.ConditionSettings.ConditionType == ConditionTypes.SelfLowHealth)
            {
                return ((float)EmeraldComponent.HealthComponent.CurrentHealth / (float)EmeraldComponent.HealthComponent.StartingHealth) <= (AttackData.AbilityObject.ConditionSettings.LowHealthPercentage * 0.01f);
            }
            else if (AttackData.AbilityObject.ConditionSettings.ConditionType == ConditionTypes.AllyLowHealth)
            {
                if (EmeraldComponent.DetectionComponent.NearbyAllies.Count > 0)
                {
                    for (int j = 0; j < EmeraldComponent.DetectionComponent.NearbyAllies.Count; j++)
                    {
                        bool HasLowHealth = ((float)EmeraldComponent.DetectionComponent.NearbyAllies[j].HealthComponent.CurrentHealth / (float)EmeraldComponent.DetectionComponent.NearbyAllies[j].HealthComponent.StartingHealth) <= (AttackData.AbilityObject.ConditionSettings.LowHealthPercentage * 0.01f);
                        if (HasLowHealth && !EmeraldComponent.DetectionComponent.NearbyAllies[j].AnimationComponent.IsDead)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (AttackData.AbilityObject.ConditionSettings.ConditionType == ConditionTypes.NoCurrentSummons)
            {
                return EmeraldComponent.DetectionComponent.CurrentFollowers.Count == 0;
            }
            else if (AttackData.AbilityObject.ConditionSettings.ConditionType == ConditionTypes.DistanceFromTarget)
            {
                if (AttackData.AbilityObject.ConditionSettings.ValueCompareType == AbilityData.ConditionData.ValueCompareTypes.LessThan)
                {
                    return EmeraldComponent.CombatComponent.DistanceFromTarget <= AttackData.AbilityObject.ConditionSettings.DistanceFromTarget;
                }
                else if (AttackData.AbilityObject.ConditionSettings.ValueCompareType == AbilityData.ConditionData.ValueCompareTypes.GreaterThan)
                {
                    return EmeraldComponent.CombatComponent.DistanceFromTarget >= AttackData.AbilityObject.ConditionSettings.DistanceFromTarget;
                }
            }

            return false;
        }

        /// <summary>
        /// 現在選ばれている攻撃のデータをセットします。
        /// </summary>
        static void SetAttackValues(EmeraldSystem EmeraldComponent, AttackClass.AttackData AttackData, bool CooldownIgnored)
        {
            EmeraldComponent.CombatComponent.CurrentAttackData = AttackData;

            AttackData.CooldownIgnored = CooldownIgnored;

            EmeraldComponent.CombatComponent.CurrentAnimationIndex = AttackData.AttackAnimation;
            EmeraldComponent.CombatComponent.CurrentEmeraldAIAbility = AttackData.AbilityObject;

            EmeraldComponent.CombatComponent.AttackDistance = AttackData.AttackDistance;
            // 攻撃距離は検知距離を超えないようにする
            if (EmeraldComponent.CombatComponent.AttackDistance > EmeraldComponent.DetectionComponent.DetectionRadius) EmeraldComponent.CombatComponent.AttackDistance = EmeraldComponent.DetectionComponent.DetectionRadius;
            EmeraldComponent.CombatComponent.TooCloseDistance = AttackData.TooCloseDistance;
            if (EmeraldComponent.CombatComponent.CombatState) EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
        }

        /// <summary>
        /// 現在の攻撃クラスと AI のピック方式に従って、次の攻撃を生成します。
        /// </summary>
        public static void GenerateNextAttack(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
                GenerateAttack(EmeraldComponent, EmeraldComponent.CombatComponent.Type1Attacks);
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
                GenerateAttack(EmeraldComponent, EmeraldComponent.CombatComponent.Type2Attacks);

            EmeraldComponent.AnimationComponent.AttackingTracker = false;
        }

        /// <summary>
        /// 現在生成済みの攻撃を、攻撃リスト内で最も近い攻撃に上書きします。
        /// </summary>
        public static void GenerateClosestAttack(EmeraldSystem EmeraldComponent)
        {
            EmeraldComponent.AnimationComponent.AttackingTracker = false;

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
                GenerateAttack(EmeraldComponent, EmeraldComponent.CombatComponent.Type1Attacks, true);
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
                GenerateAttack(EmeraldComponent, EmeraldComponent.CombatComponent.Type2Attacks, true);
        }

        /// <summary>
        /// EmeraldAttackEvent（アニメーションイベント）から渡された AttackTransformName に基づき、
        /// 現在の攻撃・武器の Transform を更新します。
        /// 攻撃Transformリスト内から一致する名前を検索し、見つからない場合は HeadTransform を使用します。
        /// </summary>
        public static void UpdateAttackTransforms(EmeraldSystem EmeraldComponent, string AttackTransformName)
        {
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                Transform AttackTransform = EmeraldComponent.CombatComponent.WeaponType1AttackTransforms.Find(x => x != null && x.name == AttackTransformName);
                if (AttackTransform != null)
                {
                    EmeraldComponent.CombatComponent.CurrentAttackTransform = AttackTransform;
                }
                else
                {
                    EmeraldComponent.CombatComponent.CurrentAttackTransform = EmeraldComponent.DetectionComponent.HeadTransform;
                }
            }
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
            {
                Transform AttackTransform = EmeraldComponent.CombatComponent.WeaponType2AttackTransforms.Find(x => x.name == AttackTransformName);
                if (AttackTransform != null)
                {
                    EmeraldComponent.CombatComponent.CurrentAttackTransform = AttackTransform;
                }
                else
                {
                    EmeraldComponent.CombatComponent.CurrentAttackTransform = EmeraldComponent.DetectionComponent.HeadTransform;
                }
            }
        }

        /// <summary>
        /// EmeraldChargeAttack（アニメーションイベント）から渡された AttackTransformName に基づき、
        /// 武器の Transform を返します。見つからない場合、イベントは発火しません。
        /// </summary>
        public static Transform GetAttackTransform(EmeraldSystem EmeraldComponent, string AttackTransformName)
        {
            Transform WeaponTransform = null;

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                WeaponTransform = EmeraldComponent.CombatComponent.WeaponType1AttackTransforms.Find(x => x != null && x.name == AttackTransformName);
            }
            else
            {
                WeaponTransform = EmeraldComponent.CombatComponent.WeaponType2AttackTransforms.Find(x => x.name == AttackTransformName);
            }

            return WeaponTransform;
        }

        /// <summary>
        /// 武器タイプの切替タイミングを再生成します。
        /// </summary>
        public static void ResetWeaponSwapTime(EmeraldSystem EmeraldComponent)
        {
            EmeraldComponent.CombatComponent.SwitchWeaponTime = Random.Range((float)EmeraldComponent.CombatComponent.SwitchWeaponTimeMin, (float)EmeraldComponent.CombatComponent.SwitchWeaponTimeMax + 1);
            EmeraldComponent.CombatComponent.SwitchWeaponTimer = 0;
        }

        /// <summary>
        /// AI を戦闘状態（Combat State）に移行させます。
        /// </summary>
        public static void ActivateCombatState(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.CombatComponent.CombatState)
                return;

            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            EmeraldComponent.CombatComponent.CombatState = true;
            EmeraldComponent.AIAnimator.SetBool("Idle Active", false);
            EmeraldComponent.AIAnimator.SetBool("Combat State Active", true);
            EmeraldComponent.MovementComponent.CurrentMovementState = EmeraldMovement.MovementStates.Run;
        }

        /// <summary>
        /// AI の各コンポーネントを無効化します（AI が死亡したときに呼び出されます）。
        /// </summary>
        public static void DisableComponents(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.SoundDetectorComponent != null) EmeraldComponent.SoundDetectorComponent.enabled = false;

            if (EmeraldComponent.CoverComponent != null) EmeraldComponent.CoverComponent.enabled = false;

            if (EmeraldComponent.OptimizationComponent != null && EmeraldComponent.OptimizationComponent.m_VisibilityCheck != null)
            {
                EmeraldComponent.OptimizationComponent.enabled = false;
                EmeraldComponent.OptimizationComponent.m_VisibilityCheck.enabled = false;
            }

            EmeraldComponent.CombatComponent.ExitCombat();
            EmeraldComponent.AIBoxCollider.enabled = false;
            EmeraldComponent.DetectionComponent.enabled = false;
            EmeraldComponent.AnimationComponent.enabled = false;
            EmeraldComponent.CombatComponent.enabled = false;
            EmeraldComponent.MovementComponent.enabled = false;
            EmeraldComponent.BehaviorsComponent.enabled = false;
            EmeraldComponent.m_NavMeshAgent.ResetPath();
            EmeraldComponent.m_NavMeshAgent.enabled = false;
            EmeraldComponent.StartCoroutine(AlignAIOnDeath(EmeraldComponent)); // 死亡時にアライメントを合わせます（アライメント機能が無効でも実行）
        }

        /// <summary>
        /// AI が死亡した際に地形へ整列させます。
        /// </summary>
        static IEnumerator AlignAIOnDeath(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1 && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type1Animations.DeathList.Count == 0 ||
                EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2 && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type2Animations.DeathList.Count == 0)
                yield break;

            Vector3 SurfaceNormal = Vector3.zero;

            while (EmeraldComponent.AIAnimator.enabled)
            {
                RaycastHit HitDown;
                if (Physics.Raycast(new Vector3(EmeraldComponent.transform.position.x, EmeraldComponent.transform.position.y + 0.25f, EmeraldComponent.transform.position.z), -Vector3.up, out HitDown, 2f, EmeraldComponent.MovementComponent.AlignmentLayerMask))
                {
                    if (HitDown.transform != EmeraldComponent.transform)
                    {
                        float m_MaxNormalAngle = EmeraldComponent.MovementComponent.MaxNormalAngle * 0.01f;
                        SurfaceNormal = HitDown.normal;
                        SurfaceNormal.x = Mathf.Clamp(SurfaceNormal.x, -m_MaxNormalAngle, m_MaxNormalAngle);
                        SurfaceNormal.z = Mathf.Clamp(SurfaceNormal.z, -m_MaxNormalAngle, m_MaxNormalAngle);
                    }
                }

                EmeraldComponent.transform.rotation = Quaternion.Slerp(EmeraldComponent.transform.rotation, Quaternion.FromToRotation(EmeraldComponent.transform.up, SurfaceNormal) * EmeraldComponent.transform.rotation, Time.deltaTime * 5);
                yield return null;
            }
        }


        /// <summary>
        /// AI の各コンポーネントを有効化します（AI をリセットしたときに呼び出されます）。
        /// </summary>
        public static void EnableComponents(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.SoundDetectorComponent != null) EmeraldComponent.SoundDetectorComponent.enabled = true;

            if (EmeraldComponent.CoverComponent != null) EmeraldComponent.CoverComponent.enabled = true;

            if (EmeraldComponent.OptimizationComponent != null && EmeraldComponent.OptimizationComponent.m_VisibilityCheck != null)
            {
                EmeraldComponent.OptimizationComponent.enabled = false;
                EmeraldComponent.OptimizationComponent.m_VisibilityCheck.enabled = true;
            }

            if (EmeraldComponent.InverseKinematicsComponent != null) EmeraldComponent.InverseKinematicsComponent.EnableInverseKinematics();

            EmeraldComponent.m_NavMeshAgent.enabled = true;
            EmeraldComponent.m_NavMeshAgent.isStopped = false;
            EmeraldComponent.AIBoxCollider.enabled = true;
            EmeraldComponent.DetectionComponent.enabled = true;
            EmeraldComponent.AnimationComponent.enabled = true;
            EmeraldComponent.CombatComponent.enabled = true;
            EmeraldComponent.MovementComponent.enabled = true;
            EmeraldComponent.BehaviorsComponent.enabled = true;
            EmeraldComponent.AIAnimator.enabled = true;
            EmeraldComponent.AIAnimator.Rebind();
            EmeraldComponent.AnimationComponent.ResetSettings();
            if (EmeraldComponent.LBDComponent != null) EmeraldComponent.LBDComponent.ResetLBDComponents();
            if (EmeraldComponent.ItemsComponent != null) EmeraldComponent.ItemsComponent.ResetSettings();
        }

        /// <summary>
        /// 検出された場合に、コライダーとリジッドボディを無効化します。
        /// </summary>
        public static void DisableRagdoll(EmeraldSystem EmeraldComponent)
        {
            // LocationBasedDamage コンポーネントが検出された場合は、コライダーを保持する必要があるためリターン
            if (EmeraldComponent.GetComponent<LocationBasedDamage>() != null)
                return;

            foreach (Rigidbody R in EmeraldComponent.GetComponentsInChildren<Rigidbody>())
            {
                R.isKinematic = true;
            }

            if (EmeraldComponent.LBDComponent != null)
                return;

            foreach (Collider C in EmeraldComponent.GetComponentsInChildren<Collider>())
            {
                C.enabled = false;
            }

            EmeraldComponent.GetComponent<BoxCollider>().enabled = true;
        }


        /// <summary>
        /// 検出された場合に、AI 内部のコライダーとリジッドボディを有効化します。
        /// </summary>
        public static void EnableRagdoll(EmeraldSystem EmeraldComponent)
        {
            // 現在の武器タイプの死亡アニメーションリストにアニメがある場合は、ラグドールを有効化しない
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1 && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type1Animations.DeathList.Count > 0 ||
                EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2 && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type2Animations.DeathList.Count > 0)
                return;

            EmeraldComponent.AIAnimator.enabled = false;

            if (EmeraldComponent.LBDComponent == null)
            {
                foreach (Collider C in EmeraldComponent.GetComponentsInChildren<Collider>())
                {
                    if (C.transform != EmeraldComponent.transform)
                    {
                        C.enabled = true;
                    }
                }

                foreach (Rigidbody R in EmeraldComponent.GetComponentsInChildren<Rigidbody>())
                {
                    R.isKinematic = false;
                    R.useGravity = true;
                }
            }
            else
            {
                for (int i = 0; i < EmeraldComponent.LBDComponent.ColliderList.Count; i++)
                {
                    if (EmeraldComponent.LBDComponent.ColliderList[i].ColliderObject != null)
                        EmeraldComponent.LBDComponent.ColliderList[i].ColliderObject.enabled = true;
                }

                for (int i = 0; i < EmeraldComponent.LBDComponent.ColliderList.Count; i++)
                {
                    Rigidbody ColliderRigidbody = EmeraldComponent.LBDComponent.ColliderList[i].ColliderObject.GetComponent<Rigidbody>();

                    if (ColliderRigidbody != null)
                    {
                        ColliderRigidbody.isKinematic = false;
                        ColliderRigidbody.useGravity = true;
                    }

                    EmeraldComponent.LBDComponent.ColliderList[i].ColliderObject.enabled = true;
                }
            }

            EmeraldComponent.StartCoroutine(AddRagdollForceInternal(EmeraldComponent)); // ラグドールの有効化後、力を加える
        }

        static IEnumerator AddRagdollForceInternal(EmeraldSystem EmeraldComponent)
        {
            float t = 0;
            // 最後にAIへダメージを与えたターゲットの反対方向へ、ラグドールへ力を加えるために使用
            Transform LastAttacker = EmeraldComponent.CombatComponent.LastAttacker;
            float Force = EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount;
            Rigidbody m_Rigidbody = null;

            // デフォルトのラグドールへの力は HeadTransform を使用
            if (EmeraldComponent.CombatComponent.RagdollTransform == null) m_Rigidbody = EmeraldComponent.DetectionComponent.HeadTransform.GetComponent<Rigidbody>();
            // RagdollTransform が指定されている場合は、LBD エリア経由でダメージを受けたため、その Transform を使用
            else m_Rigidbody = EmeraldComponent.CombatComponent.RagdollTransform.GetComponent<Rigidbody>();

            if (m_Rigidbody != null && LastAttacker != null)
            {
                while (t < 0.2f)
                {
                    t += Time.fixedDeltaTime;
                    m_Rigidbody.AddForce((EmeraldComponent.transform.position - LastAttacker.position).normalized * Force + (Vector3.up * Force * 0.05f), ForceMode.Acceleration);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// AI が攻撃をトリガーできる条件を満たしているかどうかを返します。
        /// </summary>
        public static bool AllowedToAttack(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.DetectionComponent.TargetObstructed || EmeraldComponent.CombatComponent.DeathDelayActive || EmeraldComponent.MovementComponent.DefaultMovementPaused || EmeraldComponent.AnimationComponent.InternalHit || EmeraldComponent.AnimationComponent.InternalDodge || EmeraldComponent.AnimationComponent.InternalBlock)
            {
                return false;
            }
            else if (!WithinStoppingDistanceOfTarget(EmeraldComponent) || !WithinDistanceOfTarget(EmeraldComponent))
            {
                return false;
            }
            else if (EmeraldComponent.AIAnimator.GetBool("Hit") || EmeraldComponent.AIAnimator.GetBool("Strafe Active") || EmeraldComponent.AIAnimator.GetBool("Dodge Triggered") || EmeraldComponent.AIAnimator.GetBool("Blocking") || EmeraldComponent.AnimationComponent.IsSwitchingWeapons ||
                EmeraldComponent.AnimationComponent.IsBackingUp || EmeraldComponent.AnimationComponent.IsBlocking || EmeraldComponent.AnimationComponent.IsAttacking || EmeraldComponent.AnimationComponent.IsRecoiling || EmeraldComponent.AnimationComponent.IsStrafing || EmeraldComponent.AnimationComponent.IsDodging || EmeraldComponent.AnimationComponent.IsGettingHit)
            {
                return false;
            }
            else if (!EmeraldComponent.CombatComponent.TargetWithinAngleLimit() || EmeraldComponent.CurrentTargetInfo.CurrentIDamageable.Health <= 0)
            {
                return false;
            }

            // すべてのチェックを通過したので、攻撃を許可します。
            return true;
        }

        public static void CheckAttackHeight(EmeraldSystem EmeraldComponent)
        {
            if (!EmeraldComponent.CombatComponent.CurrentEmeraldAIAbility) return;

            // 近接アビリティの攻撃可否を、高さ差（MaxAttackHeight）で制限します。
            MeleeAbility m_MeleeAbility = EmeraldComponent.CombatComponent.CurrentEmeraldAIAbility as MeleeAbility;

            if (m_MeleeAbility && EmeraldComponent.CombatTarget)
            {
                float heightDifference = Mathf.Abs(EmeraldComponent.transform.position.y - EmeraldComponent.CombatTarget.position.y);
                if (heightDifference > m_MeleeAbility.MeleeSettings.MaxAttackHeight) EmeraldComponent.CombatComponent.TargetOutOfHeightRange = true;
                else EmeraldComponent.CombatComponent.TargetOutOfHeightRange = false;
            }
            else
            {
                EmeraldComponent.CombatComponent.TargetOutOfHeightRange = false;
            }
        }

        /// <summary>
        /// 現在のターゲットに対して、NavMesh の remainingDistance を用いて「停止距離内か」を判定します。
        /// </summary>
        static bool WithinStoppingDistanceOfTarget(EmeraldSystem EmeraldComponent)
        {
            return (EmeraldComponent.m_NavMeshAgent.remainingDistance <= EmeraldComponent.m_NavMeshAgent.stoppingDistance);
        }

        /// <summary>
        /// Vector3.Distance を用いて「停止距離内か」を判定します。
        /// </summary>
        static bool WithinDistanceOfTarget(EmeraldSystem EmeraldComponent)
        {
            return (EmeraldComponent.CombatComponent.DistanceFromTarget <= EmeraldComponent.m_NavMeshAgent.stoppingDistance);
        }

        /// <summary>
        /// AI と現在のターゲットの高さ差（Y軸のみ）を返します。
        /// </summary>
        public static float GetTargetHeight(EmeraldSystem EmeraldComponent)
        {
            Vector3 m_TargetPos = EmeraldComponent.CombatTarget.position;
            m_TargetPos.x = 0;
            m_TargetPos.z = 0;
            Vector3 m_CurrentPos = EmeraldComponent.transform.position;
            m_CurrentPos.x = 0;
            m_CurrentPos.z = 0;
            return Vector3.Distance(m_TargetPos, m_CurrentPos);
        }

        /// <summary>
        /// 重み付きランダム（Odds）方式のための確率を生成します。
        /// </summary>
        public static float GenerateProbability(float[] probs)
        {
            float total = 0;

            foreach (float elem in probs)
            {
                total += elem;
            }

            float randomPoint = Random.value * total;

            for (int i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i])
                {
                    return i;
                }
                else
                {
                    randomPoint -= probs[i];
                }
            }
            return probs.Length - 1;
        }

        /// <summary>
        /// 渡された Transform に対する現在の角度を返します。
        /// </summary>
        public static float TransformAngle(EmeraldSystem EmeraldComponent, Transform Target)
        {
            if (Target == null)
                return 180;

            Vector3 Direction = new Vector3(Target.position.x, 0, Target.position.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
            float angle = Vector3.Angle(EmeraldComponent.transform.forward, Direction);
            float RotationDifference = EmeraldComponent.transform.localEulerAngles.x;
            RotationDifference = (RotationDifference > 180) ? RotationDifference - 360 : RotationDifference;
            float AdjustedAngle = Mathf.Abs(angle) - Mathf.Abs(RotationDifference);
            return AdjustedAngle;
        }

        /// <summary>
        /// 現在のターゲットに対する角度を返します。
        /// </summary>
        public static float TargetAngle(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.CombatTarget == null)
                return 360;
            Vector3 Direction = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
            float angle = Vector3.Angle(EmeraldComponent.transform.forward, Direction);
            float RotationDifference = EmeraldComponent.transform.localEulerAngles.x;
            RotationDifference = (RotationDifference > 180) ? RotationDifference - 360 : RotationDifference;
            return Mathf.Abs(angle) - Mathf.Abs(RotationDifference);
        }

        /// <summary>
        /// この AI と現在のターゲットとの距離を返します。
        /// </summary>
        public static float GetDistanceFromTarget(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.CombatTarget == null) return 0;

            Vector3 CurrentTargetPos = EmeraldComponent.CombatTarget.position;
            CurrentTargetPos.y = 0;
            Vector3 CurrentPos = EmeraldComponent.transform.position;
            CurrentPos.y = 0;
            return Vector3.Distance(CurrentTargetPos, CurrentPos);
        }

        /// <summary>
        /// この AI と現在の LookAt ターゲットとの距離を返します。
        /// </summary>
        public static float GetDistanceFromLookTarget(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.LookAtTarget == null) return 0;

            Vector3 CurrentTargetPos = EmeraldComponent.LookAtTarget.position;
            CurrentTargetPos.y = 0;
            Vector3 CurrentPos = EmeraldComponent.transform.position;
            CurrentPos.y = 0;
            return Vector3.Distance(CurrentTargetPos, CurrentPos);
        }
    }
}
