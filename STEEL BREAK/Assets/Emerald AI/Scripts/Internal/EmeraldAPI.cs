using EmeraldAI.Utility;                 // EmeraldAI のユーティリティ名前空間を使用
using System.Collections;               // IEnumerator などのコレクション基盤
using System.Collections.Generic;       // List<T> などのジェネリックコレクション
using System.Linq;                      // LINQ 拡張
using UnityEngine;                      // UnityEngine 基本 API
using UnityEngine.AI;                   // NavMesh / NavMeshAgent など

namespace EmeraldAI
{
    /// <summary>
    /// 【概要】
    /// Emerald AI の実用 API を静的クラス経由で簡便に呼び出すための集約クラス。
    /// API のカテゴリごとに入れ子クラスを用意（Detection / Behaviors / Combat / Health / Faction / Movement / Animation / Sound / UI / Items / Internal）。
    /// 各 API の呼び出しには対象 AI の EmeraldSystem コンポーネントを渡す必要があります。
    /// </summary>
    // ▼このクラスは「Emerald AI 用の静的 API 集約クラス」
    public static class EmeraldAPI
    {
        /// <summary>
        /// 【Detection 関連の公開 API 群】
        /// </summary>
        public class Detection
        {
            /// <summary>
            /// 【フォロー対象を割り当て】
            /// AI に新しいフォロー対象 Transform を割り当てます。AI の BehaviorType に応じて Pet/Companion 的挙動になります。
            /// </summary>
            /// <param name="EmeraldComponent">この API を実行する AI の EmeraldSystem。</param>
            /// <param name="Target">フォロー対象となる Transform。</param>
            /// <param name="CopyFactionData">対象が AI の場合、その派閥データをコピーして同様の敵味方判断を行うか。</param>
            public static void SetTargetToFollow(EmeraldSystem EmeraldComponent, Transform Target, bool CopyFactionData = true)
            {
                EmeraldComponent.DetectionComponent.SetTargetToFollow(Target, CopyFactionData); // 内部 Detection へ委譲
            }

            /// <summary>
            /// 【召喚対象の初期化】
            /// 召喚された AI に対して、保護・追従すべきターゲットを初期化します。
            /// </summary>
            /// <param name="EmeraldComponent">召喚 AI の EmeraldSystem。</param>
            /// <param name="Target">召喚 AI が追従・保護するターゲット。</param>
            public static void InitializeSummonTarget(EmeraldSystem EmeraldComponent, Transform Target)
            {
                EmeraldComponent.DetectionComponent.InitializeSummonTarget(Target, true);
            }

            /// <summary>
            /// 【フォロー対象の解除】
            /// 現在の TargetToFollow をクリアし、Companion/Pet 状態も解除します。
            /// </summary>
            /// <param name="EmeraldComponent">この API を実行する AI の EmeraldSystem。</param>
            public static void ClearTargetToFollow(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.DetectionComponent.ClearTargetToFollow();
            }

            /// <summary>
            /// 【プレイヤー検知の有無を確認】
            /// プレイヤーが AI の検知半径内に居るかどうかを true/false で返します
            /// （敵対/非敵対に関わらず、CombatTarget または LookAtTarget がプレイヤータグであれば true）。
            /// </summary>
            /// <param name="EmeraldComponent">この API を実行する AI の EmeraldSystem。</param>
            public static bool CheckForPlayerDetection(EmeraldSystem EmeraldComponent)
            {
                return EmeraldComponent.CombatTarget != null && EmeraldComponent.CombatTarget.CompareTag(EmeraldComponent.DetectionComponent.PlayerTag) || EmeraldComponent.LookAtTarget != null && EmeraldComponent.LookAtTarget.CompareTag(EmeraldComponent.DetectionComponent.PlayerTag);
            }

            /// <summary>
            /// 【無視ターゲットの全クリア】
            /// EmeraldDetection の静的 IgnoredTargetsList を空にします。
            /// </summary>
            public static void ClearAllIgnoredTargets()
            {
                EmeraldDetection.IgnoredTargetsList.Clear();
            }

            /// <summary>
            /// 【無視ターゲットの追加】
            /// 指定 Transform を EmeraldDetection の静的 IgnoredTargetsList へ追加します。
            /// </summary>
            public static void SetIgnoredTarget(Transform TargetTransform)
            {
                if (!EmeraldDetection.IgnoredTargetsList.Contains(TargetTransform))
                {
                    EmeraldDetection.IgnoredTargetsList.Add(TargetTransform);
                }
            }

            /// <summary>
            /// 【無視ターゲットの削除】
            /// 指定 Transform を EmeraldDetection の静的 IgnoredTargetsList から削除します。
            /// </summary>
            public static void ClearIgnoredTarget(Transform TargetTransform)
            {
                if (!EmeraldDetection.IgnoredTargetsList.Contains(TargetTransform))
                {
                    Debug.Log("The TargetTransform did not exist in the EmeraldAISystem IgnoreTargetsList list."); // ログ文は原文のまま（実行挙動不変）
                    return;
                }

                EmeraldDetection.IgnoredTargetsList.Remove(TargetTransform);
            }
        }

        /// <summary>
        /// 【Behaviors 関連の公開 API 群】
        /// </summary>
        public class Behaviors
        {
            /// <summary>
            /// 【ビヘイビアの切り替え】
            /// 指定の BehaviorType に切り替えます。内部の BehaviorState は既定の "Non Combat" に戻し、新しい挙動に基づき状態が更新されます。
            /// </summary>
            /// <param name="EmeraldComponent">この API を実行する AI の EmeraldSystem。</param>
            /// <param name="BehaviorType">切り替え先のビヘイビアタイプ。</param>
            public static void ChangeBehavior(EmeraldSystem EmeraldComponent, EmeraldBehaviors.BehaviorTypes BehaviorType)
            {
                EmeraldComponent.BehaviorsComponent.CurrentBehaviorType = BehaviorType;
                EmeraldComponent.BehaviorsComponent.BehaviorState = "Non Combat";
            }
        }

        /// <summary>
        /// 【Combat 関連の公開 API 群】
        /// </summary>
        public class Combat
        {
            /// <summary>
            /// 【ノックバック】
            /// ノックバックモジュール同様のパラメータでターゲット AI をノックバックさせます（ターゲットは Emerald AI である必要があります）。
            /// </summary>
            /// <param name="Direction">ノックバック方向。</param>
            /// <param name="Target">ノックバック対象の Transform。</param>
            /// <param name="TargetICombat">対象の ICombat 実装。</param>
            /// <param name="KnockbackDistance">ノックバック距離（どれだけ押し戻すか）。</param>
            /// <param name="KnockbackDuration">到達に要する秒数（補間時間）。</param>
            /// <param name="MovementDelay">ノックバック後、移動を再開するまでの遅延秒数。</param>
            public static void KnockbackAI(Vector3 Direction, Transform Target, ICombat TargetICombat, float KnockbackDistance = 2.5f, float KnockbackDuration = 0.25f, float MovementDelay = 0.25f)
            {
                Target.GetComponent<MonoBehaviour>().StartCoroutine(KnockbackSequence(Direction, Target, TargetICombat, KnockbackDistance, KnockbackDuration, MovementDelay));

                IEnumerator KnockbackSequence(Vector3 Direction, Transform Target, ICombat TargetICombat, float KnockbackDistance, float KnockbackDuration, float MovementDelay)
                {
                    // ブロック/回避中はノックバックを無効化
                    if (TargetICombat.IsBlocking() || TargetICombat.IsDodging())
                    {
                        yield break;
                    }

                    NavMeshAgent navMeshAgent = Target.GetComponent<NavMeshAgent>();                   // 対象の NavMeshAgent
                    EmeraldSystem TargetEmeraldComponent = Target.GetComponent<EmeraldSystem>();       // 対象の EmeraldSystem

                    Vector3 destination = Target.position + Direction * KnockbackDistance;             // 目標座標（平面）
                    destination.y = Target.position.y;

                    // 既に死亡していれば中断
                    if (TargetEmeraldComponent && TargetEmeraldComponent.AnimationComponent.IsDead)
                    {
                        yield break;
                    }

                    // 進行方向に遮蔽がある場合はノックバックしない
                    Ray ray = new Ray(Target.position + Vector3.up * 1f, Direction * KnockbackDistance);
                    //Debug.DrawRay(Target.position + Vector3.up * 1f, Direction * KnockbackDistance, Color.red, 10f);
                    if (Physics.Raycast(ray, out RaycastHit hit, KnockbackDistance, TargetEmeraldComponent.MovementComponent.AlignmentLayerMask))
                    {
                        yield break;
                    }

                    Vector3 start = Target.position;                                                  // 開始位置
                    float elapsed = 0f;                                                               // 経過時間
                    float t = 0f;                                                                      // 補間係数

                    while (t < 1f)
                    {
                        t = elapsed / KnockbackDuration;                                              // 0→1 へ進行
                        Vector3 flatPos = Vector3.Lerp(start, destination, t);                        // 水平方向の補間
                        Vector3 groundPos = GetGroundedPosition(flatPos);                             // 地形に合わせて高さ調整

                        if (navMeshAgent) navMeshAgent.Warp(groundPos);                               // ワープで位置反映（物理衝突を避ける）

                        elapsed += Time.deltaTime;

                        // ノックバック中に死亡したら中断
                        if (TargetEmeraldComponent && TargetEmeraldComponent.AnimationComponent.IsDead)
                        {
                            yield break;
                        }

                        yield return null;
                    }

                    if (navMeshAgent) navMeshAgent.isStopped = true;                                  // 一時停止
                    yield return new WaitForSeconds(MovementDelay);                                   // 遅延
                    if (navMeshAgent) navMeshAgent.isStopped = false;                                 // 再開

                    /// <summary>
                    /// 【地形に沿った位置へ調整】
                    /// レイキャストで地面に揃えた Y を返す。
                    /// </summary>
                    Vector3 GetGroundedPosition(Vector3 position)
                    {
                        Ray ray = new Ray(position + Vector3.up * 1f, Vector3.down);
                        if (Physics.Raycast(ray, out RaycastHit hit, 2f, TargetEmeraldComponent.MovementComponent.AlignmentLayerMask))
                        {
                            position.y = hit.point.y;
                        }
                        return position;
                    }
                }
            }

            /// <summary>
            /// 【アビリティの強制トリガー】
            /// AI の現在アビリティと攻撃距離を上書きし（条件が満たされれば）直ちに使用させます。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="AbilityObject">使用させる EmeraldAbilityObject。</param>
            /// <param name="AttackDistance">アビリティ発動に必要な新しい距離。</param>
            /// <param name="AttackAnimationIndex">アニメーションプロファイルの攻撃リストにおける攻撃アニメ Index。</param>
            public static void TriggerAbility(EmeraldSystem EmeraldComponent, EmeraldAbilityObject AbilityObject, int AttackDistance, int AttackAnimationIndex)
            {
                EmeraldComponent.CombatComponent.CurrentAnimationIndex = AttackAnimationIndex;
                EmeraldComponent.CombatComponent.CurrentEmeraldAIAbility = AbilityObject;
                EmeraldComponent.CombatComponent.CancelAllCombatActions();
                EmeraldComponent.CombatComponent.AttackDistance = AttackDistance;
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = AttackDistance;
                EmeraldComponent.AnimationComponent.PlayAttackAnimation();
            }

            /// <summary>
            /// 【即時キル】
            /// この AI を即座に死亡状態にします。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void KillAI(EmeraldSystem EmeraldComponent)
            {
                if (!EmeraldComponent.AnimationComponent.IsDead)
                {
                    EmeraldComponent.HealthComponent.KillAI();
                }
            }

            /// <summary>
            /// 【AI のリセット】
            /// 再利用できるように AI を初期状態へ戻します（通常は死亡後に呼ばれますが、有効化時にも自動で呼ばれます）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void ResetAI(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.HealthComponent.InstantlyRefillAIHealth();
                EmeraldCombatManager.EnableComponents(EmeraldComponent);
                EmeraldComponent.AnimationComponent.IsDead = false;
                EmeraldCombatManager.DisableRagdoll(EmeraldComponent);
            }

            /// <summary>
            /// 【ターゲットまでの距離取得】
            /// AI と現在の戦闘ターゲットの距離を返します（CombatTarget が null の場合は -1 を返す）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static float GetDistanceFromTarget(EmeraldSystem EmeraldComponent)
            {
                if (EmeraldComponent.CombatTarget != null)
                {
                    return EmeraldComponent.CombatComponent.DistanceFromTarget;
                }
                else
                {
                    Debug.Log("This AI's Combat Target is null");
                    return -1;
                }
            }

            /// <summary>
            /// 【戦闘ターゲット取得】
            /// 現在の戦闘ターゲット Transform を返します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static Transform GetCombatTarget(EmeraldSystem EmeraldComponent)
            {
                return EmeraldComponent.CombatTarget;
            }

            /// <summary>
            /// 【戦闘ターゲットの設定（検知半径内のみ）】
            /// 検知半径内にいる指定ターゲットを戦闘ターゲットに設定します（Aggressive のみ有効）。半径外は無視されます。
            /// 無制限距離で設定したい場合は OverrideCombatTarget を使用してください。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="Target">設定したいターゲット。</param>
            public static void SetCombatTarget(EmeraldSystem EmeraldComponent, Transform Target)
            {
                if (EmeraldComponent.BehaviorsComponent.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive) return;

                if (Target != null)
                {
                    EmeraldComponent.DetectionComponent.SetDetectedTarget(Target);
                    EmeraldComponent.m_NavMeshAgent.ResetPath();
                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
                    EmeraldComponent.m_NavMeshAgent.destination = Target.position;
                }
                else if (Target == null)
                {
                    Debug.Log("The SetCombatTarget paramter is null. Ensure that the target exists before calling this function.");
                }
            }

            /// <summary>
            /// 【戦闘ターゲットの上書き（距離制限なし）】
            /// 距離制限を無視して指定ターゲットを戦闘ターゲットに設定します（Aggressive のみ有効）。
            /// 攻撃距離外であれば、AI はターゲットへ接近してから攻撃します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="Target">設定したいターゲット。</param>
            public static void OverrideCombatTarget(EmeraldSystem EmeraldComponent, Transform Target)
            {
                if (EmeraldComponent.BehaviorsComponent.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive) return;

                if (Target != null)
                {
                    EmeraldComponent.DetectionComponent.SetDetectedTarget(Target);
                    EmeraldComponent.m_NavMeshAgent.ResetPath();
                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
                    EmeraldComponent.m_NavMeshAgent.destination = Target.position;
                    EmeraldComponent.BehaviorsComponent.InfititeChase = true; // 無限追跡を有効
                }
                else if (Target == null)
                {
                    Debug.Log("The OverrideCombatTarget paramter is null. Ensure that the target exists before calling this function.");
                }
            }

            /// <summary>
            /// 【逃走】
            /// 指定ターゲットから逃げるように AI を切り替えます（Behavior を Coward に）。以後は手動リセットまで Coward のままです。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="FleeTarget">逃走の基準となるターゲット。</param>
            public static void FleeFromTarget(EmeraldSystem EmeraldComponent, Transform FleeTarget)
            {
                if (FleeTarget != null)
                {
                    EmeraldComponent.BehaviorsComponent.CurrentBehaviorType = EmeraldBehaviors.BehaviorTypes.Coward;
                    EmeraldComponent.DetectionComponent.SetDetectedTarget(FleeTarget);
                }
                else if (FleeTarget == null)
                {
                    Debug.Log("The FleeTarget paramter is null. Ensure that the target exists before calling this function.");
                }
            }

            /// <summary>
            /// 【最寄りターゲット探索】
            /// 検知半径内から最も近いターゲットを検索して設定します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void SearchForClosestTarget(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetTypes.Closest);
                EmeraldComponent.DetectionComponent.SetDetectedTarget(EmeraldComponent.CombatTarget);
            }

            /// <summary>
            /// 【ランダムターゲット探索】
            /// 検知半径内からランダムにターゲットを検索します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void SearchForRandomTarget(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetTypes.Random);
            }

            /// <summary>
            /// 【最後に攻撃してきた相手】
            /// 直近でこの AI を攻撃した Transform を返します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static Transform GetLastAttacker(EmeraldSystem EmeraldComponent)
            {
                return EmeraldComponent.CombatComponent.LastAttacker;
            }
        }

        /// <summary>
        /// 【Health 関連の公開 API 群】
        /// </summary>
        public class Health
        {
            /// <summary>
            /// 【即時全回復】
            /// 死亡していない場合に限り、AI の体力を最大まで即時回復します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void InstantlyRefillAIHealth(EmeraldSystem EmeraldComponent)
            {
                if (!EmeraldComponent.AnimationComponent.IsDead)
                {
                    EmeraldComponent.HealthComponent.InstantlyRefillAIHealth();
                }
            }

            /// <summary>
            /// 【体力の更新】
            /// AI の現在体力と最大体力を更新します（死亡中は無効）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="MaxHealth">新しい最大体力。</param>
            /// <param name="CurrentHealth">新しい現在体力。</param>
            public static void UpdateHealth(EmeraldSystem EmeraldComponent, int MaxHealth, int CurrentHealth)
            {
                if (!EmeraldComponent.AnimationComponent.IsDead)
                {
                    EmeraldComponent.HealthComponent.UpdateHealth(MaxHealth, CurrentHealth);
                }
            }
        }

        /// <summary>
        /// 【Faction 関連の公開 API 群】
        /// </summary>
        public class Faction
        {
            /// <summary>
            /// 【派閥関係の変更】
            /// 指定の派閥に対する関係（敵/中立/友好）を変更します。対象の派閥は AI の派閥リスト内に存在している必要があります。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="Faction">変更対象の派閥名。</param>
            /// <param name="RelationType">設定する関係タイプ（Enemy/Neutral/Friendly）。</param>
            public static void SetFactionLevel(EmeraldSystem EmeraldComponent, string Faction, RelationTypes RelationType)
            {
                EmeraldFactionData FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

                for (int i = 0; i < EmeraldComponent.DetectionComponent.FactionRelationsList.Count; i++)
                {
                    if (EmeraldComponent.DetectionComponent.FactionRelationsList[i].FactionIndex == FactionData.FactionNameList.IndexOf(Faction))
                    {
                        EmeraldComponent.DetectionComponent.FactionRelationsList[i].RelationType = RelationType;
                        EmeraldComponent.DetectionComponent.SetupFactions();
                        return;
                    }
                    else
                    {
                        Debug.Log("The faction '" + Faction + "' does not exist in the AI's Faction Relations list. Please add it using the Faction Settings Foldout through the Emerald Detection editor of this AI.");
                    }
                }
            }

            /// <summary>
            /// 【派閥関係の追加】
            /// AI の派閥関係リストに派閥と関係タイプを追加します（派閥は Faction Manager に存在している必要あり）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="Faction">追加する派閥名。</param>
            /// <param name="RelationType">関係タイプ（Enemy/Neutral/Friendly）。</param>
            public static void AddFactionRelation(EmeraldSystem EmeraldComponent, string Faction, RelationTypes RelationType)
            {
                EmeraldFactionData FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

                if (!EmeraldDetection.FactionData.FactionNameList.Contains(Faction))
                {
                    Debug.Log("The faction '" + Faction + "' does not exist in the Faction Manager. Please add it using the Emerald Faction Manager.");
                    return;
                }

                for (int i = 0; i < EmeraldComponent.DetectionComponent.FactionRelationsList.Count; i++)
                {
                    if (EmeraldComponent.DetectionComponent.FactionRelationsList[i].FactionIndex == FactionData.FactionNameList.IndexOf(Faction))
                    {
                        Debug.Log("This AI already contains the faction '" + Faction + "'. If you would like to modify an AI's existing faction, please use SetFactionLevel(string Faction, RelationTypes RelationType) instead.");
                        return;
                    }
                }

                EmeraldComponent.DetectionComponent.FactionRelationsList.Add(new FactionClass(FactionData.FactionNameList.IndexOf(Faction), (int)RelationType));
            }

            /// <summary>
            /// 【対象の派閥関係（文字列）を取得】
            /// 渡したターゲットとこの AI の関係を "Enemy/Neutral/Friendly" の文字列で返します。
            /// 無効なターゲットまたは派閥が見つからない場合は "Invalid Target" を返します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="Target">判定したい Transform。</param>
            public static string GetTargetFactionRelation(EmeraldSystem EmeraldComponent, Transform Target)
            {
                IFaction m_IFaction = Target.GetComponent<IFaction>();

                if (m_IFaction != null)
                {
                    int ReceivedFaction = m_IFaction.GetFaction();
                    if (EmeraldComponent.DetectionComponent.AIFactionsList.Contains(ReceivedFaction))
                    {
                        var Faction = (RelationTypes)EmeraldComponent.DetectionComponent.FactionRelations[EmeraldComponent.DetectionComponent.AIFactionsList.IndexOf(ReceivedFaction)];
                        return Faction.ToString();
                    }
                    else return "Invalid Target";
                }
                else return "Invalid Target";
            }

            /// <summary>
            /// 【対象の派閥名を取得】
            /// 引数の AI（または自分自身の Transform を渡しても可）の派閥名を返します。
            /// </summary>
            public static string GetTargetFactionName(Transform Target)
            {
                IFaction m_IFaction = Target.GetComponent<IFaction>();

                if (m_IFaction != null)
                {
                    int ReceivedFaction = m_IFaction.GetFaction();
                    return EmeraldDetection.FactionData.FactionNameList[ReceivedFaction];
                }
                else return "Invalid Target";
            }

            /// <summary>
            /// 【AI の派閥を変更】
            /// 指定の派閥名に AI の所属派閥を切り替えます（派閥名は Faction Manager に存在している必要あり）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="FactionName">切り替え先の派閥名。</param>
            public static void ChangeFaction(EmeraldSystem EmeraldComponent, string FactionName)
            {
                EmeraldFactionData FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

                if (FactionData.FactionNameList.Contains(FactionName))
                {
                    EmeraldComponent.DetectionComponent.CurrentFaction = FactionData.FactionNameList.IndexOf(FactionName);
                }
                else
                {
                    Debug.Log("Faction not Found");
                }
            }

            /// <summary>
            /// 【派閥データのコピー】
            /// FactionDataToCopy の派閥情報（自派閥/関係/リスト）を EmeraldComponent へコピーします。
            /// </summary>
            /// <param name="EmeraldComponent">コピー先の EmeraldSystem。</param>
            /// <param name="FactionDataToCopy">コピー元の EmeraldSystem。</param>
            public static void CopyFactionData(EmeraldSystem EmeraldComponent, EmeraldSystem FactionDataToCopy)
            {
                EmeraldComponent.DetectionComponent.CurrentFaction = FactionDataToCopy.DetectionComponent.CurrentFaction;
                EmeraldComponent.DetectionComponent.AIFactionsList = FactionDataToCopy.DetectionComponent.AIFactionsList;
                EmeraldComponent.DetectionComponent.FactionRelations = FactionDataToCopy.DetectionComponent.FactionRelations;
                EmeraldComponent.DetectionComponent.FactionRelationsList = FactionDataToCopy.DetectionComponent.FactionRelationsList;
            }
        }

        /// <summary>
        /// 【Movement 関連の公開 API 群】
        /// </summary>
        public class Movement
        {
            /// <summary>
            /// 【徘徊タイプの変更】
            /// 徘徊タイプを切り替えます。Dynamic を指定した場合は AI の現在位置を動的徘徊の基準位置に更新します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="NewWanderType">新しい徘徊タイプ。</param>
            public static void ChangeWanderType(EmeraldSystem EmeraldComponent, EmeraldMovement.WanderTypes NewWanderType)
            {
                if (NewWanderType == EmeraldMovement.WanderTypes.Dynamic) UpdateDynamicWanderPosition(EmeraldComponent);
                EmeraldComponent.MovementComponent.ChangeWanderType(NewWanderType);
            }

            /// <summary>
            /// 【動的徘徊の基準位置を現在位置へ更新】
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void UpdateDynamicWanderPosition(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.StartingDestination = EmeraldComponent.transform.position;
            }

            /// <summary>
            /// 【動的徘徊の基準位置を任意座標に設定】
            /// 任意の場所を基準に動的徘徊させます（この呼び出しにより徘徊タイプは自動的に Dynamic に変更されます）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="DestinationPosition">基準とする座標。</param>
            public static void SetDynamicWanderPosition(EmeraldSystem EmeraldComponent, Vector3 DestinationPosition)
            {
                EmeraldComponent.MovementComponent.ChangeWanderType(EmeraldMovement.WanderTypes.Dynamic);
                EmeraldComponent.MovementComponent.StartingDestination = DestinationPosition;
            }

            /// <summary>
            /// 【開始位置を現在位置に更新】
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void UpdateStartingPosition(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.StartingDestination = EmeraldComponent.transform.position;
            }

            /// <summary>
            /// 【カスタム目的地の設定】
            /// 徘徊タイプを Custom に上書きし、任意の座標へ向かわせます（ポイント＆クリック移動、スケジュール等に有用）。
            /// 別の徘徊タイプへ戻すには ChangeWanderType を再度呼びます。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="DestinationPosition">目的地座標。</param>
            public static void SetCustomDestination(EmeraldSystem EmeraldComponent, Vector3 DestinationPosition)
            {
                EmeraldComponent.MovementComponent.ResetWanderSettings();
                if (EmeraldComponent.MovementComponent.WanderType != EmeraldMovement.WanderTypes.Custom) ChangeWanderType(EmeraldComponent, EmeraldMovement.WanderTypes.Custom);
                EmeraldComponent.m_NavMeshAgent.SetDestination(DestinationPosition);
            }

            /// <summary>
            /// 【目的地の設定】
            /// 単純に NavMeshAgent の目的地を設定し、徘徊設定をリセットします。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="DestinationPosition">目的地座標。</param>
            public static void SetDestination(EmeraldSystem EmeraldComponent, Vector3 DestinationPosition)
            {
                EmeraldComponent.m_NavMeshAgent.SetDestination(DestinationPosition);
                EmeraldComponent.MovementComponent.ResetWanderSettings();
            }

            /// <summary>
            /// 【現在位置を中心にランダム目的地を生成】
            /// 指定半径内でランダムな目的地を生成し、NavMesh 上で有効なら移動させます。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="Radius">探索半径。</param>
            public static void GenerateRandomDestination(EmeraldSystem EmeraldComponent, int Radius)
            {
                Vector3 NewDestination = EmeraldComponent.transform.position + new Vector3(Random.insideUnitSphere.y, 0, Random.insideUnitSphere.z) * Radius;
                RaycastHit HitDown;
                if (Physics.Raycast(new Vector3(NewDestination.x, NewDestination.y + 5, NewDestination.z), -EmeraldComponent.transform.up, out HitDown, 10, EmeraldComponent.MovementComponent.DynamicWanderLayerMask, QueryTriggerInteraction.Ignore))
                {
                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(NewDestination, out hit, 4, EmeraldComponent.m_NavMeshAgent.areaMask))
                    {
                        EmeraldComponent.m_NavMeshAgent.SetDestination(NewDestination);
                    }
                }
            }

            /// <summary>
            /// 【ウェイポイントの追加】
            /// AI のウェイポイントリストへ Transform の位置を追加します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="Waypoint">追加するウェイポイント Transform。</param>
            public static void AddWaypoint(EmeraldSystem EmeraldComponent, Transform Waypoint)
            {
                EmeraldComponent.MovementComponent.WaypointsList.Add(Waypoint.position);
            }

            /// <summary>
            /// 【ウェイポイントの削除】
            /// 指定インデックスのウェイポイントを削除します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="WaypointIndex">削除するインデックス。</param>
            public static void RemoveWaypoint(EmeraldSystem EmeraldComponent, int WaypointIndex)
            {
                EmeraldComponent.MovementComponent.WaypointsList.RemoveAt(WaypointIndex);
            }

            /// <summary>
            /// 【ウェイポイント全消去】
            /// すべてのウェイポイントをクリアして徘徊タイプを Stationary に設定します。
            /// 再度ウェイポイント徘徊させる場合は ChangeWanderType で Waypoints をセットし直してください。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void ClearAllWaypoints(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.WanderType = EmeraldMovement.WanderTypes.Stationary;
                EmeraldComponent.MovementComponent.WaypointsList.Clear();
            }

            /// <summary>
            /// 【非戦闘時の移動停止】
            /// 会話などの用途向けに、非戦闘時の移動を一時停止します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void StopMovement(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                EmeraldComponent.m_NavMeshAgent.isStopped = true;
                EmeraldComponent.MovementComponent.DefaultMovementPaused = true;
            }

            /// <summary>
            /// 【非戦闘時の移動再開】
            /// StopMovement 呼び出し後の移動を再開します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void ResumeMovement(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.DefaultMovement();
                EmeraldComponent.m_NavMeshAgent.isStopped = false;
                EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
            }

            /// <summary>
            /// 【フォローの一時停止】
            /// フォロー対象がある AI の追従を一時停止します（対象が無い場合はエラー）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void StopFollowing(EmeraldSystem EmeraldComponent)
            {
                if (EmeraldComponent.TargetToFollow == null)
                {
                    Debug.LogError("The '" + EmeraldComponent.gameObject.name + "' does not have a Current Follow Target. Please have one before calling this function.");
                    return;
                }

                EmeraldComponent.m_NavMeshAgent.ResetPath();
                EmeraldComponent.m_NavMeshAgent.isStopped = true;
                EmeraldComponent.MovementComponent.DefaultMovementPaused = true;
            }

            /// <summary>
            /// 【フォローの再開】
            /// フォロー対象がある AI の追従を再開します（対象が無い場合はエラー）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void ResumeFollowing(EmeraldSystem EmeraldComponent)
            {
                if (EmeraldComponent.TargetToFollow == null)
                {
                    Debug.LogError("The '" + EmeraldComponent.gameObject.name + "' does not have a Current Follow Target. Please have one before calling this function.");
                    return;
                }

                EmeraldComponent.m_NavMeshAgent.isStopped = false;
                EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
            }

            /// <summary>
            /// 【コンパニオンの警備開始】
            /// フォロー対象を持つ AI に、指定座標を警備させます。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="PositionToGuard">警備させる座標。</param>
            public static void StartCompanionGuardPosition(EmeraldSystem EmeraldComponent, Vector3 PositionToGuard)
            {
                EmeraldComponent.MovementComponent.DefaultMovementPaused = true;
                EmeraldComponent.m_NavMeshAgent.SetDestination(PositionToGuard);
            }

            /// <summary>
            /// 【コンパニオンの警備終了】
            /// 警備を停止し、フォロワーへ戻るようにします。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void StopCompanionGuardPosition(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
            }

            /// <summary>
            /// 【指定座標へ回頭】
            /// AI をターゲット座標へ向けて回頭させます（回頭中は移動不可）。必要であれば先に StopMovement を呼び出してください。
            /// 停止角は Movement コンポーネントで設定した Turning Angle に基づきます。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="TargetPosition">回頭先の座標（オブジェクト/プレイヤー/AI/任意位置）。</param>
            public static void RotateTowardsPosition(EmeraldSystem EmeraldComponent, Vector3 TargetPosition)
            {
                EmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(RotateTowardsPositionInternal(EmeraldComponent, TargetPosition));
            }

            static IEnumerator RotateTowardsPositionInternal(EmeraldSystem EmeraldComponent, Vector3 TargetPosition)
            {
                if (EmeraldComponent.MovementComponent.RotateTowardsTarget) yield break; // すでに回頭中なら二重実行しない

                EmeraldComponent.MovementComponent.RotateTowardsTarget = true; // 状態フラグ（回頭中は特定機能を停止）
                EmeraldComponent.MovementComponent.LockTurning = false;        // ターンアニメの引っかかり防止用フラグ
                EmeraldComponent.m_NavMeshAgent.isStopped = true;              // NavMesh を一時停止
                yield return new WaitForSeconds(0.1f);                         // フラグ反映のための小さな遅延

                while (!EmeraldComponent.MovementComponent.LockTurning)
                {
                    Vector3 Direction = new Vector3(TargetPosition.x, 0, TargetPosition.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                    EmeraldComponent.MovementComponent.UpdateRotations(Direction);   // 目標方向へ回頭更新
                    EmeraldComponent.AnimationComponent.CalculateTurnAnimations(true); // ターンアニメ計算
                    yield return null;
                }

                EmeraldComponent.MovementComponent.RotateTowardsTarget = false; // 回頭終了
                EmeraldComponent.m_NavMeshAgent.isStopped = false;              // NavMesh 再開
            }
        }

        /// <summary>
        /// 【Animation 関連の公開 API 群】
        /// </summary>
        public class Animation
        {
            /// <summary>
            /// 【エモート再生（単発）】
            /// エモートアニメ（Emote Animation List の Index 指定）を再生します。戦闘中は無効。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="EmoteAnimationID">エモートアニメの ID（リストのインデックス）。</param>
            public static void PlayEmoteAnimation(EmeraldSystem EmeraldComponent, int EmoteAnimationID)
            {
                EmeraldComponent.AnimationComponent.PlayEmoteAnimation(EmoteAnimationID);
            }

            /// <summary>
            /// 【エモート再生（ループ）】
            /// エモートアニメをループ再生します。停止するには StopLoopEmoteAnimation を呼びます。戦闘中は無効。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="EmoteAnimationID">エモートアニメの ID。</param>
            public static void LoopEmoteAnimation(EmeraldSystem EmeraldComponent, int EmoteAnimationID)
            {
                EmeraldComponent.AnimationComponent.LoopEmoteAnimation(EmoteAnimationID);
            }

            /// <summary>
            /// 【エモートループの停止】
            /// ループ再生中のエモートを停止します。戦闘中は無効。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="EmoteAnimationID">エモートアニメの ID。</param>
            public static void StopLoopEmoteAnimation(EmeraldSystem EmeraldComponent, int EmoteAnimationID)
            {
                EmeraldComponent.AnimationComponent.StopLoopEmoteAnimation(EmoteAnimationID);
            }

            /// <summary>
            /// 【アイドルアニメの明示指定】
            /// ランダム生成の代わりに、次回のアイドルアニメを明示的に指定します（1〜6）。
            /// スケジュールなど特定場所での演出に有用です。解除するには DisableOverrideIdleAnimation を呼びます。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="IdleIndex">指定するアイドルアニメ番号（1〜6）。</param>
            public static void OverrideIdleAnimation(EmeraldSystem EmeraldComponent, int IdleIndex)
            {
                EmeraldComponent.AnimationComponent.m_IdleAnimaionIndexOverride = true;
                EmeraldComponent.AIAnimator.SetInteger("Idle Index", IdleIndex);
            }

            /// <summary>
            /// 【アイドルアニメ指定の解除】
            /// OverrideIdleAnimation を無効化し、再びランダム生成へ戻します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void DisableOverrideIdleAnimation(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.AnimationComponent.m_IdleAnimaionIndexOverride = false;
            }
        }

        /// <summary>
        /// 【Sound 関連の公開 API 群】
        /// </summary>
        public class Sound
        {
            /// <summary>
            /// 【任意の AudioClip を再生】
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="Clip">再生する AudioClip。</param>
            public static void PlaySoundClip(EmeraldSystem EmeraldComponent, AudioClip Clip)
            {
                EmeraldComponent.SoundComponent.PlaySoundClip(Clip);
            }

            /// <summary>
            /// 【待機（アイドル）サウンドを再生】
            /// アイドルサウンドリストからランダムに再生（アニメーションイベントから呼ぶことも可能）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void PlayIdleSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayIdleSound();
            }

            /// <summary>
            /// 【攻撃サウンドを再生】
            /// 攻撃サウンドリストからランダムに再生（アニメーションイベントから呼ぶことも可能）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void PlayAttackSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayAttackSound();
            }

            /// <summary>
            /// 【警告サウンドを再生】
            /// 警告サウンドリストからランダムに再生。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void PlayWarningSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayWarningSound();
            }

            /// <summary>
            /// 【ブロックサウンドを再生】
            /// ブロックサウンドリストからランダムに再生。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void PlayBlockSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayBlockSound();
            }

            /// <summary>
            /// 【被弾サウンドを再生】
            /// 負傷サウンドリストからランダムに再生。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void PlayInjuredSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayInjuredSound();
            }

            /// <summary>
            /// 【死亡サウンドを再生】
            /// 死亡サウンドリストからランダムに再生（アニメーションイベントからも可）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void PlayDeathSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayDeathSound();
            }

            /// <summary>
            /// 【歩行フットステップ音を再生】
            /// フットステップ（歩行）サウンドリストから再生（アニメーションイベントで呼ぶ前提）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void WalkFootstepSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.WalkFootstepSound();
            }

            /// <summary>
            /// 【走行フットステップ音を再生】
            /// フットステップ（走行）サウンドリストから再生（アニメーションイベントで呼ぶ前提）。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void RunFootstepSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.RunFootstepSound();
            }

            /// <summary>
            /// 【汎用効果音をランダム再生】
            /// General Sounds リストからランダムに再生。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void PlayRandomSoundEffect(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayRandomSoundEffect();
            }

            /// <summary>
            /// 【効果音 ID 指定で再生】
            /// General Sounds から SoundEffectID を指定して再生。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="SoundEffectID">再生したい効果音の ID（リストのインデックス）。</param>
            public static void PlaySoundEffect(EmeraldSystem EmeraldComponent, int SoundEffectID)
            {
                EmeraldComponent.SoundComponent.PlaySoundEffect(SoundEffectID);
            }
        }

        /// <summary>
        /// 【UI 関連の公開 API 群（EmeraldUI が必要）】
        /// </summary>
        public class UI
        {
            /// <summary>
            /// 【UI の体力バー色を更新】
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="NewColor">新しいバー色。</param>
            public static void UpdateUIHealthBarColor(EmeraldSystem EmeraldComponent, Color NewColor)
            {
                CheckForUIComponent(EmeraldComponent); // EmeraldUI の存在確認

                if (EmeraldComponent.UIComponent.AutoCreateHealthBars == YesOrNo.Yes)
                {
                    GameObject HealthBarChild = EmeraldComponent.UIComponent.HealthBar.transform.Find("AI Health Bar Background").gameObject;
                    UnityEngine.UI.Image HealthBarRef = HealthBarChild.transform.Find("AI Health Bar").GetComponent<UnityEngine.UI.Image>();
                    HealthBarRef.color = NewColor;
                    UnityEngine.UI.Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<UnityEngine.UI.Image>();
                    HealthBarBackgroundImageRef.color = EmeraldComponent.UIComponent.HealthBarBackgroundColor;
                }
            }

            /// <summary>
            /// 【UI の体力バー背景色を更新】
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="NewColor">新しい背景色。</param>
            public static void UpdateUIHealthBarBackgroundColor(EmeraldSystem EmeraldComponent, Color NewColor)
            {
                CheckForUIComponent(EmeraldComponent); // EmeraldUI の存在確認

                if (EmeraldComponent.UIComponent.AutoCreateHealthBars == YesOrNo.Yes)
                {
                    GameObject HealthBarChild = EmeraldComponent.UIComponent.HealthBar.transform.Find("AI Health Bar Background").gameObject;
                    UnityEngine.UI.Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<UnityEngine.UI.Image>();
                    HealthBarBackgroundImageRef.color = NewColor;
                }
            }

            /// <summary>
            /// 【UI の名前表示色を更新】
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="NewColor">新しい文字色。</param>
            public static void UpdateUINameColor(EmeraldSystem EmeraldComponent, Color NewColor)
            {
                CheckForUIComponent(EmeraldComponent); // EmeraldUI の存在確認

                if (EmeraldComponent.UIComponent.AutoCreateHealthBars == YesOrNo.Yes && EmeraldComponent.UIComponent.DisplayAIName == YesOrNo.Yes)
                {
                    EmeraldComponent.UIComponent.AINameUI.color = NewColor;
                }
            }

            /// <summary>
            /// 【UI の名前テキストを更新】
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="NewName">新しい名前文字列。</param>
            public static void UpdateUINameText(EmeraldSystem EmeraldComponent, string NewName)
            {
                CheckForUIComponent(EmeraldComponent); // EmeraldUI の存在確認

                if (EmeraldComponent.UIComponent.AutoCreateHealthBars == YesOrNo.Yes && EmeraldComponent.UIComponent.DisplayAIName == YesOrNo.Yes)
                {
                    EmeraldComponent.UIComponent.AINameUI.text = NewName;
                }
            }

            /// <summary>
            /// 【UI コンポーネントの存在確認】
            /// EmeraldUI が未アタッチならエラーを出して中断します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            static void CheckForUIComponent(EmeraldSystem EmeraldComponent)
            {
                if (EmeraldComponent.UIComponent == null)
                {
                    Debug.LogError("The '" + EmeraldComponent.name + "' AI does not have a EmeraldUI component. Please attach one to said AI before calling this function.");
                    return;
                }
            }
        }

        /// <summary>
        /// 【Items 関連の公開 API 群（EmeraldItems が必要）】
        /// </summary>
        public class Items
        {
            /// <summary>
            /// 【アイテム有効化】
            /// アイテム ID を指定して、AI のアイテムリストから有効化します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="ItemID">有効化したいアイテム ID。</param>
            public static void EnableItem(EmeraldSystem EmeraldComponent, int ItemID)
            {
                CheckForItemsComponent(EmeraldComponent); // EmeraldItems の存在確認
                EmeraldComponent.ItemsComponent.EnableItem(ItemID);
            }

            /// <summary>
            /// 【アイテム無効化】
            /// アイテム ID を指定して無効化します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            /// <param name="ItemID">無効化したいアイテム ID。</param>
            public static void DisableItem(EmeraldSystem EmeraldComponent, int ItemID)
            {
                CheckForItemsComponent(EmeraldComponent); // EmeraldItems の存在確認
                EmeraldComponent.ItemsComponent.DisableItem(ItemID);
            }

            /// <summary>
            /// 【全アイテムを無効化】
            /// AI のアイテムリストに含まれるすべてを無効化します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void DisableAllItems(EmeraldSystem EmeraldComponent)
            {
                CheckForItemsComponent(EmeraldComponent); // EmeraldItems の存在確認
                EmeraldComponent.ItemsComponent.DisableAllItems();
            }

            /// <summary>
            /// 【装備状態のリセット】
            /// 装備中のアイテム設定を初期状態に戻します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void ResetSettings(EmeraldSystem EmeraldComponent)
            {
                CheckForItemsComponent(EmeraldComponent); // EmeraldItems の存在確認
                EmeraldComponent.ItemsComponent.ResetSettings();
            }

            /// <summary>
            /// 【Items コンポーネントの存在確認】
            /// 未アタッチならエラーを出して中断します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            static void CheckForItemsComponent(EmeraldSystem EmeraldComponent)
            {
                if (EmeraldComponent.ItemsComponent == null)
                {
                    Debug.LogError("The '" + EmeraldComponent.name + "' AI does not have a EmeraldItems component. Please attach one to said AI before calling this function.");
                    return;
                }
            }
        }

        /// <summary>
        /// 【内部処理向け（直接の使用は非推奨）】
        /// 特定の内部メカニクスで使用される API 群。直接の使用は推奨されません。
        /// </summary>
        public class Internal
        {
            /// <summary>
            /// 【150度の扇形内で遮蔽物のない位置を探索】
            /// 正面±75°の範囲で最大 10 ステップ探索し、現在ターゲットとの間に遮蔽が無い位置を返します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static Vector3 FindUnobstructedPosition(EmeraldSystem EmeraldComponent)
            {
                if (EmeraldComponent.CoverComponent == null) return EmeraldComponent.transform.position;

                int steps = 10;

                for (int i = 0; i < steps; i++)
                {
                    int Distance = Random.Range(3, 5);
                    float t = i / (float)(steps - 1);
                    float angle = Mathf.Lerp(-75, 75, t);
                    Vector3 PotentialPosition = EmeraldComponent.transform.position;
                    if (EmeraldComponent.CoverComponent.CurrentCoverNode) PotentialPosition = Quaternion.AngleAxis(angle, EmeraldComponent.transform.up) * EmeraldComponent.CoverComponent.CurrentCoverNode.transform.forward * Distance;
                    else PotentialPosition = Quaternion.AngleAxis(angle, EmeraldComponent.transform.up) * EmeraldComponent.transform.forward * Distance;
                    Vector3 AIPosition = EmeraldComponent.DetectionComponent.HeadTransform.position;

                    // 近傍にカバーノード等がある場合はスキップ（密集回避）
                    Vector3 PositionToCheck = PotentialPosition + AIPosition;
                    PositionToCheck.y = EmeraldComponent.transform.position.y;
                    Collider[] hitColliders = Physics.OverlapSphere(PositionToCheck, 1.5f, EmeraldComponent.CoverComponent.CoverNodeLayerMask);

                    // デバッグ用可視化（コメントアウト）
                    //if (hitColliders.Length > 0) Debug.Log(EmeraldComponent.gameObject.name + "  -  " + hitColliders.Length);
                    //Debug.DrawRay(EmeraldComponent.DetectionComponent.HeadTransform.position, PotentialPosition, Color.yellow, Distance);
                    //Debug.DrawLine(PotentialPosition + AIPosition, PotentialPosition + AIPosition + EmeraldComponent.transform.up * 1, Color.cyan, Distance);
                    //Debug.DrawLine(PotentialPosition + AIPosition, EmeraldComponent.CurrentTargetInfo.CurrentICombat.DamagePosition(), Color.black, Distance);

                    if (hitColliders.Length == 0 && !Physics.Raycast(PotentialPosition + AIPosition, EmeraldComponent.CurrentTargetInfo.CurrentICombat.DamagePosition(), ~EmeraldComponent.DetectionComponent.ObstructionDetectionLayerMask))
                    {
                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(PotentialPosition + AIPosition, out hit, 2, NavMesh.AllAreas))
                        {
                            return hit.position;
                        }

                        return PotentialPosition + AIPosition;
                    }
                }

                // 見つからない場合は、正面付近のランダム位置を返す
                return EmeraldComponent.transform.position + EmeraldComponent.transform.right * Random.Range(-2, 3) + EmeraldComponent.transform.forward * 2f;
            }

            /// <summary>
            /// 【ターゲット周辺のランダム位置を生成して移動】
            /// 戦闘中の回り込みなどに使用。周囲に遮蔽物がなければ NavMesh 経由で移動します。
            /// </summary>
            /// <param name="EmeraldComponent">AI の EmeraldSystem。</param>
            public static void GenerateRandomPositionWithinRadius(EmeraldSystem EmeraldComponent)
            {
                // CombatTarget が直前にクリアされた等のケースを考慮
                if (EmeraldComponent.CombatTarget == null) return;

                EmeraldComponent.CombatComponent.CancelAllCombatActions();
                EmeraldComponent.MovementComponent.StopBackingUp();
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.5f;
                int Radius = Random.Range(2, 5);

                Vector3 Dir = (EmeraldComponent.CombatTarget.position - EmeraldComponent.transform.position).normalized;
                int OffsetAmount = Random.Range(1, 3);
                var DirectionOffset = Quaternion.Euler(0, OffsetAmount == 1 ? -50 : 50, 0) * Dir;
                Vector3 GeneratedDestination = EmeraldComponent.CombatTarget.position + (DirectionOffset * Radius);

                RaycastHit HitDown;
                if (Physics.Raycast(GeneratedDestination + Vector3.up * 2, -Vector3.up, out HitDown, 5f))
                {
                    GeneratedDestination.y = HitDown.point.y;
                }

                EmeraldComponent.m_NavMeshAgent.destination = GeneratedDestination;
                EmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(Moving(EmeraldComponent));
            }

            static IEnumerator Moving(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.DefaultMovementPaused = true;
                yield return new WaitForSeconds(0.25f);
                float CanSeeTargetTimer = 0;

                while (EmeraldComponent.m_NavMeshAgent.enabled && !EmeraldComponent.AnimationComponent.IsDead && EmeraldComponent.m_NavMeshAgent.remainingDistance >= 0.75f)
                {
                    if (EmeraldComponent.DetectionComponent.ObstructionType == EmeraldDetection.ObstructedTypes.None)
                    {
                        CanSeeTargetTimer += Time.deltaTime;

                        if (CanSeeTargetTimer > 1.5f || EmeraldComponent.CombatComponent.DistanceFromTarget < 2)
                        {
                            EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
                            break;
                        }
                    }

                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.5f;
                    Vector3 Direction = new Vector3(EmeraldComponent.m_NavMeshAgent.steeringTarget.x, 0, EmeraldComponent.m_NavMeshAgent.steeringTarget.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                    EmeraldComponent.MovementComponent.UpdateRotations(Direction);
                    yield return null;
                }

                if (!EmeraldComponent.m_NavMeshAgent.enabled || EmeraldComponent.AnimationComponent.IsDead)
                {
                    EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
                    yield break;
                }

                yield return new WaitForSeconds(0.5f);

                float t = 0;

                while (t < 1f && EmeraldComponent.CombatTarget != null)
                {
                    t += Time.deltaTime;

                    Vector3 Direction = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                    EmeraldComponent.MovementComponent.UpdateRotations(Direction);

                    yield return null;
                }

                EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
            }
        }
    }
}
