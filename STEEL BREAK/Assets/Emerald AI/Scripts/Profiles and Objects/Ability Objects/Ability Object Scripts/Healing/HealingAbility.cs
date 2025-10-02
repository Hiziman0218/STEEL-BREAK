using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    /// <summary>
    /// 【HealingAbility】
    /// 回復系アビリティの定義用 ScriptableObject。
    /// ・チャージ演出／生成演出
    /// ・対象（自身／単体／範囲）、遅延、回復量、持続回復 などの一括管理
    /// </summary>
    [CreateAssetMenu(fileName = "回復アビリティ", menuName = "Emerald AI/アビリティ/回復アビリティ")]
    public class HealingAbility : EmeraldAbilityObject
    {
        [Header("チャージ時の設定（エフェクト等）")]
        public AbilityData.ChargeSettingsData ChargeSettings;

        [Header("生成直前の設定（エフェクト等）")]
        public AbilityData.CreateSettingsData CreateSettings;

        [Header("回復設定（対象/遅延/半径/エフェクト/持続回復 等）")]
        public AbilityData.HealingData HealingSettings;

        /// <summary>
        /// アビリティのチャージ処理（チャージエフェクトの再生など）。
        /// </summary>
        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        /// <summary>
        /// アビリティの実行処理：ターゲット取得、生成エフェクト再生、回復シーケンス開始。
        /// </summary>
        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(StartHeals(Owner, AttackTransform));
        }

        /// <summary>
        /// 遅延後に回復効果を発動。対象タイプに応じて回復初期化を分岐。
        /// </summary>
        IEnumerator StartHeals(GameObject Owner, Transform AttackTransform = null)
        {
            yield return new WaitForSeconds(HealingSettings.Delay);

            Vector3 EffectPosition = new Vector3(AttackTransform.position.x, AttackTransform.position.y, AttackTransform.position.z) + Vector3.up * HealingSettings.EffectHeightOffset;
            GameObject SpawnedEffect = HealingSettings.SpawnHealingEffect(Owner, EffectPosition, HealingSettings.HealingEffect, HealingSettings.HealingEffectTimeoutSeconds, HealingSettings.HealingSoundsList);

            if (HealingSettings.TargetType == AbilityData.HealingData.TargetTypes.Area) IntitailizeAreaHealing(Owner.GetComponent<EmeraldSystem>(), AttackTransform);
            else if (HealingSettings.TargetType == AbilityData.HealingData.TargetTypes.Self) IntitailizeSelfHealing(Owner.GetComponent<EmeraldSystem>(), AttackTransform);
            else if (HealingSettings.TargetType == AbilityData.HealingData.TargetTypes.Target) IntitailizeTargetHealing(Owner.GetComponent<EmeraldSystem>(), AttackTransform);
        }

        /// <summary>
        /// 範囲回復の初期化：自分と周囲の味方（半径内）を回復対象リストへ追加。
        /// </summary>
        void IntitailizeAreaHealing(EmeraldSystem OwnerEmeraldComponent, Transform AttackTransform)
        {
            OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Clear();
            OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Add(OwnerEmeraldComponent); // 詠唱者（自分）を回復対象に含める

            for (int i = 0; i < OwnerEmeraldComponent.DetectionComponent.NearbyAllies.Count; i++)
            {
                // 範囲（Radius）内の対象のみ回復対象に含める
                if (Vector3.Distance(OwnerEmeraldComponent.transform.position, OwnerEmeraldComponent.DetectionComponent.NearbyAllies[i].transform.position) < HealingSettings.Radius)
                {
                    OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Add(OwnerEmeraldComponent.DetectionComponent.NearbyAllies[i]);
                }
            }

            for (int i = 0; i < OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Count; i++)
            {
                EmeraldSystem TargetEmeraldComponent = OwnerEmeraldComponent.DetectionComponent.LowHealthAllies[i].GetComponent<EmeraldSystem>();

                if (TargetEmeraldComponent != null)
                {
                    // 所有者と「友好」関係、または同一派閥名の対象のみ回復する
                    if (EmeraldAPI.Faction.GetTargetFactionRelation(OwnerEmeraldComponent, OwnerEmeraldComponent.DetectionComponent.LowHealthAllies[i].transform) == "Friendly" ||
                        EmeraldAPI.Faction.GetTargetFactionName(OwnerEmeraldComponent.DetectionComponent.LowHealthAllies[i].transform) == EmeraldAPI.Faction.GetTargetFactionName(OwnerEmeraldComponent.transform))
                    {
                        if (HealingSettings.HealTargetEffect != null)
                        {
                            EmeraldObjectPool.SpawnEffect(HealingSettings.HealTargetEffect, OwnerEmeraldComponent.DetectionComponent.LowHealthAllies[i].GetComponent<ICombat>().DamagePosition(), OwnerEmeraldComponent.DetectionComponent.LowHealthAllies[i].transform.rotation, HealingSettings.HealTargetEffectTimeoutSeconds);
                        }

                        HealTarget(TargetEmeraldComponent);
                    }
                }
            }
        }

        /// <summary>
        /// 自己回復の初期化：自分に対して回復エフェクトと回復処理を適用。
        /// </summary>
        void IntitailizeSelfHealing(EmeraldSystem OwnerEmeraldComponent, Transform AttackTransform)
        {
            if (HealingSettings.HealTargetEffect != null)
            {
                EmeraldObjectPool.SpawnEffect(HealingSettings.HealTargetEffect, OwnerEmeraldComponent.GetComponent<ICombat>().DamagePosition(), OwnerEmeraldComponent.transform.rotation, HealingSettings.HealTargetEffectTimeoutSeconds);
            }

            HealTarget(OwnerEmeraldComponent);
        }

        /// <summary>
        /// 単体回復の初期化：生存している周囲の味方から最も体力割合が低い対象を選んで回復。
        /// </summary>
        void IntitailizeTargetHealing(EmeraldSystem OwnerEmeraldComponent, Transform AttackTransform)
        {
            OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Clear();

            for (int i = 0; i < OwnerEmeraldComponent.DetectionComponent.NearbyAllies.Count; i++)
            {
                // 死亡していないAIのみ対象
                if (!OwnerEmeraldComponent.DetectionComponent.NearbyAllies[i].AnimationComponent.IsDead)
                {
                    OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Add(OwnerEmeraldComponent.DetectionComponent.NearbyAllies[i]);
                }
            }

            // 近くの味方の中から、体力割合が最も低い対象を 1 体だけ回復
            if (OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Count > 0)
            {
                EmeraldSystem TargetEmeraldComponent = null;
                float lowestHealth = float.MaxValue;

                foreach (var Ally in OwnerEmeraldComponent.DetectionComponent.LowHealthAllies)
                {
                    float AllyHealth = (float)Ally.HealthComponent.CurrentHealth / (float)Ally.HealthComponent.StartingHealth;
                    if (AllyHealth < lowestHealth)
                    {
                        lowestHealth = AllyHealth;
                        TargetEmeraldComponent = Ally;
                    }
                }

                if (TargetEmeraldComponent && HealingSettings.HealTargetEffect != null)
                {
                    EmeraldObjectPool.SpawnEffect(HealingSettings.HealTargetEffect, TargetEmeraldComponent.GetComponent<ICombat>().DamagePosition(), TargetEmeraldComponent.transform.rotation, HealingSettings.HealTargetEffectTimeoutSeconds);
                }

                if (TargetEmeraldComponent)
                {
                    HealTarget(TargetEmeraldComponent);
                }
            }
        }

        /// <summary>
        /// 回復半径内の各味方ターゲットに対して回復処理を実行。
        /// </summary>
        void HealTarget(EmeraldSystem TargetEmeraldComponent)
        {
            HealAI(TargetEmeraldComponent, HealingSettings.BaseHealAmount);

            if (HealingSettings.HealingType == AbilityData.HealingData.HealingTypes.OverTime)
            {
                HealAIOverTime(TargetEmeraldComponent);
            }
        }

        /// <summary>
        /// 即時回復：HealAmount 分だけ瞬時に回復。回復アビリティからも使用。
        /// </summary>
        public void HealAI(EmeraldSystem TargetEmeraldComponent, int HealAmount)
        {
            EmeraldHealth HealthRef = TargetEmeraldComponent.HealthComponent;
            HealthRef.CurrentHealth = HealthRef.CurrentHealth + HealAmount;
            // 回復量が開始体力（StartingHealth）を超えないように調整
            if (HealthRef.CurrentHealth >= HealthRef.StartingHealth) HealthRef.CurrentHealth = HealthRef.StartingHealth;
            CombatTextSystem.Instance.CreateCombatTextAI(HealAmount, TargetEmeraldComponent.CombatComponent.DamagePosition(), false, true);
            HealthRef.UpdateHealingReceived();
            HealthRef.UpdateHealTick();
        }

        /// <summary>
        /// 持続回復：一定間隔（TickRate）で HealsPerTick を回復。回復アビリティからも使用。
        /// </summary>
        public void HealAIOverTime(EmeraldSystem TargetEmeraldComponent)
        {
            TargetEmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(HealAIOverTimeInternal(TargetEmeraldComponent));
        }

        /// <summary>
        /// 持続回復の内部処理。規定時間（HealOverTimeLength）まで、Tick ごとに回復と演出を適用。
        /// </summary>
        IEnumerator HealAIOverTimeInternal(EmeraldSystem TargetEmeraldComponent)
        {
            float t = 0;
            float LapsedTime = 0;
            EmeraldHealth HealthRef = TargetEmeraldComponent.HealthComponent;

            HealthRef.UpdateHealingReceived();

            while (LapsedTime <= HealingSettings.HealOverTimeLength)
            {
                t += Time.deltaTime;
                LapsedTime += Time.deltaTime;

                if (t >= HealingSettings.TickRate)
                {
                    HealthRef.CurrentHealth = HealthRef.CurrentHealth + HealingSettings.HealsPerTick;
                    Vector3 RefPosition = TargetEmeraldComponent.CombatComponent.DamagePosition();
                    CombatTextSystem.Instance.CreateCombatTextAI(HealingSettings.HealsPerTick, RefPosition, false, true);
                    HealingSettings.SpawnHealingEffect(TargetEmeraldComponent.gameObject, RefPosition, HealingSettings.HealTargetEffect, HealingSettings.HealTargetEffectTimeoutSeconds, HealingSettings.HealTickSounds);
                    HealthRef.UpdateHealTick();
                    t = 0;
                }

                // 回復対象が死亡したら持続回復を停止
                if (TargetEmeraldComponent.HealthComponent.CurrentHealth <= 0) yield break;

                // 回復量が開始体力（StartingHealth）を超えないように調整
                if (HealthRef.CurrentHealth >= HealthRef.StartingHealth) HealthRef.CurrentHealth = HealthRef.StartingHealth;

                yield return null;
            }
        }
    }
}
