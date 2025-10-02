using System.Collections;
using UnityEngine;
using EmeraldAI.Utility;
using UnityEngine.AI;

namespace EmeraldAI
{
    /// <summary>
    /// 【TeleportAbility】
    /// テレポート系アビリティの定義用 ScriptableObject。
    /// ・チャージ演出／生成演出
    /// ・消失演出 → テレポート先へワープ → 再出現演出
    /// ・ターゲット背後へ半径内でランダム配置、NavMesh へサンプル調整
    /// を行います。
    /// </summary>
    [CreateAssetMenu(fileName = "テレポート アビリティ", menuName = "Emerald AI/アビリティ/テレポート アビリティ")]
    public class TeleportAbility : EmeraldAbilityObject
    {
        [Header("チャージ時の設定（エフェクト等）")]
        public AbilityData.ChargeSettingsData ChargeSettings;

        [Header("生成直前の設定（エフェクト等）")]
        public AbilityData.CreateSettingsData CreateSettings;

        [Header("テレポート設定（消失/再出現/遅延/半径 等）")]
        public AbilityData.TeleportData TeleportSettings;

        /// <summary>
        /// アビリティのチャージ処理（チャージエフェクトの再生など）。
        /// </summary>
        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        /// <summary>
        /// アビリティの実行処理：ターゲット取得、生成エフェクト再生、テレポート初期化コルーチン開始。
        /// </summary>
        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(InitializeTeleport(Owner, Target));
        }

        /// <summary>
        /// テレポート手順：
        /// 1) 消失エフェクト → NavMesh/アニメータ一時停止 → 極小スケール化（視認不可）
        /// 2) TeleportTime 経過後、ワープ先を算出し Warp
        /// 3) 再出現インジケータ → 再出現エフェクト → 向き補正 → 復帰
        /// </summary>
        IEnumerator InitializeTeleport(GameObject Owner, Transform Target)
        {
            AbilityData.SpawnEffectAndSound(Owner, Owner.GetComponent<ICombat>().DamagePosition(), TeleportSettings.DisappearEffect, TeleportSettings.DisappearEffectTimeoutSeconds, TeleportSettings.DisappearSoundsList);
            EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            EmeraldComponent.m_NavMeshAgent.enabled = false;
            EmeraldComponent.AIAnimator.speed = 0.2f;

            yield return new WaitForSeconds(0.01f);
            Vector3 StartingScale = Owner.transform.localScale;
            Owner.transform.localScale = Vector3.one * 0.003f; // Unity の不具合により scale=0 は不可。0.003 は見えないほど小さい値。

            void ResetSettings()
            {
                Owner.transform.localScale = StartingScale;
                EmeraldComponent.m_NavMeshAgent.enabled = true;
                EmeraldComponent.AIAnimator.speed = 1;
            }

            // TeleportTime 経過後に再出現処理を行う。
            yield return new WaitForSeconds(TeleportSettings.TeleportTime - 0.5f);
            if (EmeraldComponent.CombatComponent == null) { ResetSettings(); yield break; } // テレポート中にターゲットを見失った場合はキャンセル。

            Vector3 TeleportPosition = GetTeleportPosition(Owner);
            EmeraldComponent.m_NavMeshAgent.Warp(TeleportPosition);

            yield return new WaitForSeconds(0.1f);
            if (TeleportSettings.ReappearTriggersAvoidable) EmeraldComponent.AnimationComponent.AttackTriggered = true;

            AbilityData.SpawnEffectAndSound(Owner, Owner.GetComponent<ICombat>().DamagePosition() + Vector3.up * StartingScale.y, TeleportSettings.ReappearIndicatorEffect, TeleportSettings.ReappearIndicatorEffectTimeoutSeconds, TeleportSettings.ReappearIndicatorSoundsList);
            yield return new WaitForSeconds(TeleportSettings.ReappearDelay);

            AbilityData.SpawnEffectAndSound(Owner, Owner.GetComponent<ICombat>().DamagePosition() + Vector3.up * StartingScale.y, TeleportSettings.ReappearEffect, TeleportSettings.ReappearEffectTimeoutSeconds, TeleportSettings.ReappearSoundsList);

            EmeraldComponent.MovementComponent.InstantlyRotateTowards(Target.position);

            ResetSettings();
            if (TeleportSettings.ReappearTriggersAvoidable)
            {
                yield return new WaitForSeconds(0.1f);
                EmeraldComponent.AnimationComponent.AttackTriggered = false;
            }
        }

        /// <summary>
        /// テレポート先の算出：
        /// ・設定半径内で、ターゲット背後の 180 度範囲にランダム配置
        /// ・足元の地面を Raycast で取得して高さを補正
        /// ・NavMesh 上へサンプルして最終位置を調整
        /// </summary>
        Vector3 GetTeleportPosition(GameObject Owner)
        {
            // 指定ターゲットの背後 180 度内で、設定半径に応じたランダム位置を生成
            float RandomDegree = Random.Range(0f, 1f);
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);
            if (Target == null) Target = Owner.transform;
            Vector3 TargetPosition = Target.position;
            Vector3 TeleportPosition = Owner.transform.position;
            bool TeleportRight = Random.Range(0f, 1f) <= 0.5f;

            if (TeleportRight)
            {
                TeleportPosition = TargetPosition + ((TargetPosition - Owner.transform.position).normalized + (Vector3.Lerp(Owner.transform.right, Owner.transform.forward, RandomDegree) * TeleportSettings.TeleportRadius));
            }
            else
            {
                TeleportPosition = TargetPosition + ((TargetPosition - Owner.transform.position).normalized + (Vector3.Lerp(-Owner.transform.right, Owner.transform.forward, RandomDegree) * TeleportSettings.TeleportRadius));
            }

            // 下方向に Raycast し、地面の高さへ調整
            RaycastHit hit;
            if (Physics.Raycast(TeleportPosition, Owner.transform.TransformDirection(Vector3.down), out hit, 10))
            {
                TeleportPosition = new Vector3(TeleportPosition.x, hit.point.y, TeleportPosition.z);
            }

            // NavMesh 上の有効位置へスナップ
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(TeleportPosition, out navMeshHit, 10f, NavMesh.AllAreas))
            {
                TeleportPosition = navMeshHit.position;
            }

            return TeleportPosition;
        }
    }
}
