using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    /// <summary>
    /// 【SummonAbility】
    /// 召喚アビリティの定義用 ScriptableObject。
    /// ・チャージ演出／生成演出
    /// ・召喚対象（自分／ターゲット周辺）、召喚数、半径、ディレイ、効果音 などを一括管理
    /// </summary>
    [CreateAssetMenu(fileName = "召喚アビリティ", menuName = "Emerald AI/アビリティ/召喚アビリティ")]
    public class SummonAbility : EmeraldAbilityObject
    {
        [Header("チャージ時の設定（エフェクト等）")]
        public AbilityData.ChargeSettingsData ChargeSettings;

        [Header("生成直前の設定（エフェクト等）")]
        public AbilityData.CreateSettingsData CreateSettings;

        [Header("召喚設定（召喚数/半径/位置/エフェクト/サウンド/時間制限 等）")]
        public AbilityData.SummonData SummonSettings;

        /// <summary>
        /// アビリティのチャージ処理（チャージエフェクトの再生など）。
        /// </summary>
        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        /// <summary>
        /// アビリティの実行処理：生成エフェクト再生後、召喚の初期化コルーチンを開始。
        /// </summary>
        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(IntitailizeSummon(Owner, OwnerMonoBehaviour, AttackTransform));
        }

        /// <summary>
        /// 召喚の初期化：ディレイ後、召喚演出を再生し、設定数ぶん AI を円周上に生成。
        /// </summary>
        IEnumerator IntitailizeSummon(GameObject Owner, MonoBehaviour OwnerMonoBehaviour, Transform AttackTransform = null)
        {
            yield return new WaitForSeconds(SummonSettings.SummonDelay);

            Vector3 EffectPosition = new Vector3(AttackTransform.position.x, AttackTransform.position.y, AttackTransform.position.z);
            SummonSettings.SpawnEffect(Owner, EffectPosition, SummonSettings.CastEffect, SummonSettings.CastEffectTimeoutSeconds, SummonSettings.CastSounds, false);

            for (int i = 0; i < SummonSettings.SummonAmount; i++)
            {
                // 各召喚体の角度を算出
                float angle = i * Mathf.PI * 2f / SummonSettings.SummonAmount;

                // 召喚基準位置の決定（設定に依存）
                Vector3 SummonPosition = Vector3.zero;
                if (SummonSettings.SummonPosition == AbilityData.SummonData.SummonPositions.Self) SummonPosition = Owner.transform.position;
                else if (SummonSettings.SummonPosition == AbilityData.SummonData.SummonPositions.Target)
                {
                    EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
                    if (EmeraldComponent.CombatTarget != null) SummonPosition = EmeraldComponent.CombatTarget.position;
                    else SummonPosition = Owner.transform.position; // 何らかの理由で CombatTarget が null の場合、詠唱者の周囲にフォールバック
                }

                // 角度から位置を算出（円周上に配置）
                Vector3 SpawnPosition = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * SummonSettings.SummonRadius + SummonPosition;

                // プレハブ一覧からランダムに 1 体選択
                int RandomIndex = Random.Range(0, SummonSettings.AIPrefabs.Count);

                // AI プレハブを生成。多数同時再生を避けるため、最初の 1 体以外は召喚サウンドをスキップ
                if (i == 0) OwnerMonoBehaviour.StartCoroutine(SummonAlly(Owner, RandomIndex, SpawnPosition, false));
                else OwnerMonoBehaviour.StartCoroutine(SummonAlly(Owner, RandomIndex, SpawnPosition, true));
            }
        }

        /// <summary>
        /// 詠唱者のために AI を 1 体召喚します。
        /// </summary>
        IEnumerator SummonAlly(GameObject Owner, int RandomIndex, Vector3 SpawnPosition, bool SkipSummonSound)
        {
            yield return new WaitForSeconds(SummonSettings.SummonDelay);

            EmeraldSystem AllyEmeraldComponent = EmeraldObjectPool.Spawn(SummonSettings.AIPrefabs[RandomIndex], SpawnPosition, Quaternion.identity).GetComponent<EmeraldSystem>();

            yield return new WaitForSeconds(0.01f);

            EmeraldAPI.Detection.InitializeSummonTarget(AllyEmeraldComponent, Owner.transform);

            SummonSettings.SpawnEffect(Owner, AllyEmeraldComponent.GetComponent<ICombat>().DamagePosition() + (Vector3.up * SummonSettings.SummonEffectHeightOffset), SummonSettings.SummonEffect, SummonSettings.SummonEffectTimeoutSeconds, SummonSettings.SummonSounds, SkipSummonSound);

            if (SummonSettings.IsTimedSummon)
            {
                yield return new WaitForSeconds(SummonSettings.SummonLength);
                EmeraldAPI.Combat.KillAI(AllyEmeraldComponent);
            }

            if (SummonSettings.IsTimedSummon)
            {
                yield return new WaitForSeconds(SummonSettings.DespawnLength);
                EmeraldObjectPool.Despawn(AllyEmeraldComponent.gameObject);
            }
            else
            {
                yield return new WaitUntil(() => AllyEmeraldComponent.AnimationComponent.IsDead);
                yield return new WaitForSeconds(SummonSettings.DespawnLength);
                EmeraldObjectPool.Despawn(AllyEmeraldComponent.gameObject);
            }
        }
    }
}
