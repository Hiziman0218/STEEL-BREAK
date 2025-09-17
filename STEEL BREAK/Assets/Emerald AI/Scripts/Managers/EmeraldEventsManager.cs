using UnityEngine;
using EmeraldAI.Utility;
using static UnityEngine.GraphicsBuffer;

namespace EmeraldAI
{
    /// <summary>
    /// 【EmeraldEventsManager】
    /// アニメーションイベントや外部スクリプトから呼び出すユーティリティ群。
    /// サウンド再生、ターゲット設定、派閥変更、UI更新、徘徊ポイント操作などを提供します。
    /// </summary>
    public class EmeraldEventsManager : MonoBehaviour
    {
        [Header("EmeraldSystem 参照（AI中枢の主要コンポーネント）")]
        EmeraldSystem EmeraldComponent;

        [Header("EmeraldMovement 参照（移動・徘徊・追跡の制御）")]
        EmeraldMovement MovementComponent;

        [Header("EmeraldUI 参照（ヘルスバー・名前表示の制御）")]
        EmeraldUI EmeraldUI;

        void Awake()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();
            MovementComponent = GetComponent<EmeraldMovement>();
            EmeraldUI = GetComponent<EmeraldUI>();
        }

        /// <summary>
        /// 引数の AudioClip を再生します。
        /// </summary>
        public void PlaySoundClip(AudioClip Clip)
        {
            EmeraldComponent.SoundComponent.PlaySoundClip(Clip);
        }

        /// <summary>
        /// アイドルサウンドをランダムに再生します（アニメーションイベントからも呼び出し可能）。
        /// </summary>
        public void PlayIdleSound()
        {
            EmeraldComponent.SoundComponent.PlayIdleSound();
        }

        /// <summary>
        /// 攻撃サウンドをランダムに再生します（アニメーションイベントからも呼び出し可能）。
        /// </summary>
        public void PlayAttackSound()
        {
            EmeraldComponent.SoundComponent.PlayAttackSound();
        }

        /// <summary>
        /// 警告サウンドをランダムに再生します（アニメーションイベントからも呼び出し可能）。
        /// </summary>
        public void PlayWarningSound()
        {
            EmeraldComponent.SoundComponent.PlayWarningSound();
        }

        /// <summary>
        /// ブロック（防御）サウンドをランダムに再生します。
        /// </summary>
        public void PlayBlockSound()
        {
            EmeraldComponent.SoundComponent.PlayBlockSound();
        }

        /// <summary>
        /// 被弾（負傷）サウンドをランダムに再生します。
        /// </summary>
        public void PlayInjuredSound()
        {
            EmeraldComponent.SoundComponent.PlayInjuredSound();
        }

        /// <summary>
        /// 死亡サウンドをランダムに再生します（アニメーションイベントからも呼び出し可能）。
        /// </summary>
        public void PlayDeathSound()
        {
            EmeraldComponent.SoundComponent.PlayDeathSound();
        }

        /// <summary>
        /// 歩行用フットステップ音を再生します（アニメーションイベントでの設定を推奨）。
        /// </summary>
        public void WalkFootstepSound()
        {
            EmeraldComponent.SoundComponent.WalkFootstepSound();
        }

        /// <summary>
        /// 走行用フットステップ音を再生します（アニメーションイベントでの設定を推奨）。
        /// </summary>
        public void RunFootstepSound()
        {
            EmeraldComponent.SoundComponent.RunFootstepSound();
        }

        /// <summary>
        /// 汎用サウンドリストからランダムな効果音を再生します。
        /// </summary>
        public void PlayRandomSoundEffect()
        {
            EmeraldComponent.SoundComponent.PlayRandomSoundEffect();
        }

        /// <summary>
        /// 汎用サウンドリストから、ID を指定して効果音を再生します。
        /// </summary>
        public void PlaySoundEffect(int SoundEffectID)
        {
            EmeraldComponent.SoundComponent.PlaySoundEffect(SoundEffectID);
        }

        /// <summary>
        /// このAIを即時キル（大ダメージ付与）します。
        /// </summary>
        public void KillAI()
        {
            if (!EmeraldComponent.AnimationComponent.IsDead)
            {
                EmeraldComponent.GetComponent<IDamageable>().Damage(9999999);
            }
        }

        /// <summary>
        /// 次に再生するアイドルアニメーションを手動で指定します（ランダム生成を上書き）。
        /// 例：スケジュール機能で特定地点のアイドルアニメを指定したい場合など。
        /// ※番号は 1～6、かつ Idle Animation リストに存在する必要があります。
        /// ランダム生成へ戻すには DisableOverrideIdleAnimation() を呼びます。
        /// </summary>
        public void OverrideIdleAnimation(int IdleIndex)
        {
            EmeraldComponent.AnimationComponent.m_IdleAnimaionIndexOverride = true;
            EmeraldComponent.AIAnimator.SetInteger("Idle Index", IdleIndex);
        }

        /// <summary>
        /// OverrideIdleAnimation の機能を無効化し、ランダム生成へ戻します。
        /// </summary>
        public void DisableOverrideIdleAnimation()
        {
            EmeraldComponent.AnimationComponent.m_IdleAnimaionIndexOverride = false;
        }

        /// <summary>
        /// AI の徘徊タイプ（Wander Type）を変更します。
        /// </summary>
        public void ChangeWanderType(EmeraldMovement.WanderTypes NewWanderType)
        {
            EmeraldComponent.MovementComponent.ChangeWanderType(NewWanderType);
        }

        /// <summary>
        /// 静的リスト EmeraldDetection.IgnoredTargetsList をすべてクリアします。
        /// </summary>
        public void ClearAllIgnoredTargets()
        {
            EmeraldDetection.IgnoredTargetsList.Clear();
        }

        /// <summary>
        /// 指定の Transform を IgnoredTargetsList に追加します（無視対象として設定）。
        /// </summary>
        public void SetIgnoredTarget(Transform TargetTransform)
        {
            if (!EmeraldDetection.IgnoredTargetsList.Contains(TargetTransform))
            {
                EmeraldDetection.IgnoredTargetsList.Add(TargetTransform);
            }
        }

        /// <summary>
        /// 指定の Transform を IgnoredTargetsList から削除します。
        /// </summary>
        public void ClearIgnoredTarget(Transform TargetTransform)
        {
            if (!EmeraldDetection.IgnoredTargetsList.Contains(TargetTransform))
            {
                Debug.Log("指定された TargetTransform は EmeraldAISystem の IgnoreTargetsList に存在しません。");
                return;
            }

            EmeraldDetection.IgnoredTargetsList.Remove(TargetTransform);
        }

        /// <summary>
        /// 現在のターゲットとの距離を返します（ターゲットが null の場合は -1 を返します）。
        /// </summary>
        public float GetDistanceFromTarget()
        {
            if (EmeraldComponent.CombatTarget != null)
            {
                return EmeraldComponent.CombatComponent.DistanceFromTarget;
            }
            else
            {
                Debug.Log("このAIの Current Target は null です。");
                return -1;
            }
        }

        /// <summary>
        /// 現在の戦闘ターゲット（Transform）を返します。
        /// </summary>
        public Transform GetCombatTarget()
        {
            return EmeraldComponent.CombatTarget;
        }

        /// <summary>
        /// 検知半径内に限り、指定のターゲットを戦闘ターゲットとして設定します。
        /// （無制限に設定したい場合は OverrideCombatTarget を使用）
        /// </summary>
        public void SetCombatTarget(Transform Target)
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
                Debug.Log("SetCombatTarget の引数が null です。呼び出し前に対象が存在することを確認してください。");
            }
        }

        /// <summary>
        /// 距離制限を無視して、指定のターゲットを戦闘ターゲットに設定します。
        /// 攻撃距離外であれば、AI はその位置まで移動して攻撃します。
        /// </summary>
        public void OverrideCombatTarget(Transform Target)
        {
            if (EmeraldComponent.BehaviorsComponent.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive) return;

            if (Target != null)
            {
                EmeraldComponent.DetectionComponent.SetDetectedTarget(Target);
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
                EmeraldComponent.m_NavMeshAgent.destination = Target.position;
                EmeraldComponent.BehaviorsComponent.InfititeChase = true;
            }
            else if (Target == null)
            {
                Debug.Log("OverrideCombatTarget の引数が null です。呼び出し前に対象が存在することを確認してください。");
            }
        }

        /// <summary>
        /// 指定のターゲットから逃走するように AI の挙動を上書きします。
        /// </summary>
        public void FleeFromTarget(Transform FleeTarget)
        {
            if (FleeTarget != null)
            {
                EmeraldComponent.BehaviorsComponent.CurrentBehaviorType = EmeraldBehaviors.BehaviorTypes.Coward;
                EmeraldComponent.CombatTarget = FleeTarget;
                EmeraldComponent.DetectionComponent.GetTargetInfo(EmeraldComponent.CombatTarget, true);
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                EmeraldCombatManager.ActivateCombatState(EmeraldComponent);
            }
            else if (FleeTarget == null)
            {
                Debug.Log("FleeTarget の引数が null です。呼び出し前に対象が存在することを確認してください。");
            }
        }

        /// <summary>
        /// 追従対象（フォロワーのターゲット）を設定します。
        /// </summary>
        public void SetFollowerTarget(Transform Target)
        {
            EmeraldComponent.DetectionComponent.SetTargetToFollow(Target);
        }

        /// <summary>
        /// AI を手懐けて、指定のターゲットのコンパニオンにします。
        /// ※驚愕型の挙動（Cautious）かつ Brave/Foolhardy の自信タイプで、Aggressive になる前に成功させる必要があります。
        /// </summary>
        public void TameAI(Transform Target)
        {
            EmeraldComponent.CombatComponent.ClearTarget();
            EmeraldComponent.DetectionComponent.SetTargetToFollow(Target);
        }

        /// <summary>
        /// 最後にこのAIへ攻撃した Transform を返します。
        /// </summary>
        public Transform GetLastAttacker()
        {
            return EmeraldComponent.CombatComponent.LastAttacker;
        }

        /// <summary>
        /// ヘルスバーの色を更新します。
        /// </summary>
        public void UpdateUIHealthBarColor(Color NewColor)
        {
            if (EmeraldUI.AutoCreateHealthBars == YesOrNo.Yes)
            {
                GameObject HealthBarChild = EmeraldUI.HealthBar.transform.Find("AI Health Bar Background").gameObject;
                UnityEngine.UI.Image HealthBarRef = HealthBarChild.transform.Find("AI Health Bar").GetComponent<UnityEngine.UI.Image>();
                HealthBarRef.color = NewColor;
                UnityEngine.UI.Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<UnityEngine.UI.Image>();
                HealthBarBackgroundImageRef.color = EmeraldUI.HealthBarBackgroundColor;
            }
        }

        /// <summary>
        /// ヘルスバー背景色を更新します。
        /// </summary>
        public void UpdateUIHealthBarBackgroundColor(Color NewColor)
        {
            if (EmeraldUI.AutoCreateHealthBars == YesOrNo.Yes)
            {
                GameObject HealthBarChild = EmeraldUI.HealthBar.transform.Find("AI Health Bar Background").gameObject;
                UnityEngine.UI.Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<UnityEngine.UI.Image>();
                HealthBarBackgroundImageRef.color = NewColor;
            }
        }

        /// <summary>
        /// 表示名の文字色を更新します。
        /// </summary>
        public void UpdateUINameColor(Color NewColor)
        {
            if (EmeraldUI.AutoCreateHealthBars == YesOrNo.Yes && EmeraldUI.DisplayAIName == YesOrNo.Yes)
            {
                EmeraldUI.AINameUI.color = NewColor;
            }
        }

        /// <summary>
        /// 表示名のテキストを更新します。
        /// </summary>
        public void UpdateUINameText(string NewName)
        {
            if (EmeraldUI.AutoCreateHealthBars == YesOrNo.Yes && EmeraldUI.DisplayAIName == YesOrNo.Yes)
            {
                EmeraldUI.AINameUI.text = NewName;
            }
        }

        /// <summary>
        /// ダイナミック徘徊の基準位置を、現在のAI位置へ更新します。
        /// </summary>
        public void UpdateDynamicWanderPosition()
        {
            MovementComponent.StartingDestination = this.transform.position;
        }

        /// <summary>
        /// ダイナミック徘徊の基準位置を、指定の Transform 位置へ設定します。
        /// 呼び出し時に Wander Type は自動的に Dynamic へ切り替わります。
        /// </summary>
        public void SetDynamicWanderPosition(Transform Destination)
        {
            MovementComponent.ChangeWanderType(EmeraldMovement.WanderTypes.Dynamic);
            MovementComponent.StartingDestination = Destination.position;
        }

        /// <summary>
        /// 初期位置（StartingDestination）を現在位置へ更新します。
        /// </summary>
        public void UpdateStartingPosition()
        {
            MovementComponent.StartingDestination = this.transform.position;
        }

        /// <summary>
        /// Transform の位置を使って目的地を設定します。
        /// </summary>
        public void SetDestination(Transform Destination)
        {
            //EmeraldComponent.MovementComponent.SetDestinationPosition(Destination.position);
        }

        /// <summary>
        /// Vector3 の位置を使って目的地を設定します。
        /// </summary>
        public void SetDestinationPosition(Vector3 DestinationPosition)
        {
            //EmeraldComponent.MovementComponent.SetDestinationPosition(DestinationPosition);
        }

        /// <summary>
        /// 現在位置を基準に、指定半径内で新しい移動先を生成します。
        /// </summary>
        public void GenerateNewWaypointCurrentPosition(int Radius)
        {
            Vector3 NewDestination = transform.position + new Vector3(Random.insideUnitSphere.y, 0, Random.insideUnitSphere.z) * Radius;
            RaycastHit HitDown;
            if (Physics.Raycast(new Vector3(NewDestination.x, NewDestination.y + 5, NewDestination.z), -transform.up, out HitDown, 10, MovementComponent.DynamicWanderLayerMask, QueryTriggerInteraction.Ignore))
            {
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(NewDestination, out hit, 4, EmeraldComponent.m_NavMeshAgent.areaMask))
                {
                    EmeraldComponent.m_NavMeshAgent.SetDestination(NewDestination);
                }
            }
        }

        /// <summary>
        /// ウェイポイントを AI の Waypoint リストへ追加します。
        /// </summary>
        public void AddWaypoint(Transform Waypoint)
        {
            MovementComponent.WaypointsList.Add(Waypoint.position);
        }

        /// <summary>
        /// 指定インデックスのウェイポイントを AI の Waypoint リストから削除します。
        /// </summary>
        public void RemoveWaypoint(int WaypointIndex)
        {
            MovementComponent.WaypointsList.RemoveAt(WaypointIndex);
        }

        /// <summary>
        /// すべてのウェイポイントをクリアします。
        /// クリア後はエラー回避のため Wander Type を Stationary に切り替えます。
        /// 再びウェイポイントを使いたい場合は、ChangeWanderType（本クラス内）で Waypoint に戻してください。
        /// </summary>
        public void ClearAllWaypoints()
        {
            MovementComponent.WanderType = EmeraldMovement.WanderTypes.Stationary;
            MovementComponent.WaypointsList.Clear();
        }

        /// <summary>
        /// AI の移動を停止します（例：会話中など）。
        /// </summary>
        public void StopMovement()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = true;
        }

        /// <summary>
        /// StopMovement の後、移動を再開します。
        /// </summary>
        public void ResumeMovement()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = false;
        }

        /// <summary>
        /// コンパニオンAIの追従を停止します。
        /// </summary>
        public void StopFollowing()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = true;
        }

        /// <summary>
        /// コンパニオンAIの追従を再開します。
        /// </summary>
        public void ResumeFollowing()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = false;
        }

        /// <summary>
        /// コンパニオンAIに、指定位置の警備を開始させます。
        /// </summary>
        public void StartCompanionGuardPosition(Vector3 PositionToGuard)
        {
            EmeraldComponent.MovementComponent.DefaultMovementPaused = true;
            EmeraldComponent.m_NavMeshAgent.SetDestination(PositionToGuard);
        }

        /// <summary>
        /// コンパニオンAIの警備を中止し、フォロワーへ戻します。
        /// </summary>
        public void CancelCompanionGuardPosition()
        {
            EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
        }

        /// <summary>
        /// 攻撃範囲内から最も近い新規ターゲットを探索します。
        /// </summary>
        public void SearchForClosestTarget()
        {
            EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetTypes.Closest);
            EmeraldComponent.DetectionComponent.SetDetectedTarget(EmeraldComponent.CombatTarget);
        }

        /// <summary>
        /// 攻撃範囲内からランダムな新規ターゲットを探索します。
        /// </summary>
        public void SearchForRandomTarget()
        {
            EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetTypes.Random);
        }

        /// <summary>
        /// 指定の派閥の関係レベルを変更します（AI の Faction Relations リストに存在する必要があります）。
        /// </summary>
        /// <param name="Faction">変更したい派閥名</param>
        /// <param name="RelationType">設定する関係レベル（Enemy/Neutral/Friendly）</param>
        public void SetFactionLevel(string Faction, RelationTypes RelationType)
        {
            EmeraldFactionData FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

            for (int i = 0; i < EmeraldComponent.DetectionComponent.FactionRelationsList.Count; i++)
            {
                if (EmeraldComponent.DetectionComponent.FactionRelationsList[i].FactionIndex == FactionData.FactionNameList.IndexOf(Faction))
                {
                    EmeraldComponent.DetectionComponent.FactionRelationsList[i].RelationType = RelationType;
                }
                else
                {
                    Debug.Log("派閥 '" + Faction + "' はこのAIの Faction Relations リストに存在しません。Emerald Detection エディタの Faction Settings から追加してください。");
                }
            }
        }

        /// <summary>
        /// 派閥とその関係レベルを、AI の Faction Relations リストへ追加します。
        /// ※追加する派閥は Faction Manager のリストに存在している必要があります。
        /// </summary>
        /// <param name="Faction">追加したい派閥名</param>
        /// <param name="RelationType">設定する関係レベル（Enemy/Neutral/Friendly）</param>
        public void AddFactionRelation(string Faction, RelationTypes RelationType)
        {
            EmeraldFactionData FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

            if (!EmeraldDetection.FactionData.FactionNameList.Contains(Faction))
            {
                Debug.Log("派閥 '" + Faction + "' は Faction Manager に存在しません。先に Faction Manager で追加してください。");
                return;
            }

            for (int i = 0; i < EmeraldComponent.DetectionComponent.FactionRelationsList.Count; i++)
            {
                if (EmeraldComponent.DetectionComponent.FactionRelationsList[i].FactionIndex == FactionData.FactionNameList.IndexOf(Faction))
                {
                    Debug.Log("このAIには既に派閥 '" + Faction + "' が含まれています。既存の派閥を変更する場合は SetFactionLevel(string Faction, RelationTypes RelationType) を使用してください。");
                    return;
                }
            }

            EmeraldComponent.DetectionComponent.FactionRelationsList.Add(new FactionClass(FactionData.FactionNameList.IndexOf(Faction), (int)RelationType));
        }

        /// <summary>
        /// 指定ターゲットとの関係（Enemy/Neutral/Friendly）を文字列で返します。
        /// 派閥が見つからない場合、または有効なターゲットでない場合は "Invalid Target" が返ります。
        /// </summary>
        public string GetTargetRelation(Transform Target)
        {
            return EmeraldComponent.DetectionComponent.GetTargetFactionRelation(Target);
        }

        /// <summary>
        /// AI の派閥を変更します（指定名は Faction Manager のリストに存在している必要があります）。
        /// </summary>
        public void ChangeFaction(string FactionName)
        {
            EmeraldFactionData FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

            if (FactionData.FactionNameList.Contains(FactionName))
            {
                EmeraldComponent.DetectionComponent.CurrentFaction = FactionData.FactionNameList.IndexOf(FactionName);
            }
            else
            {
                Debug.Log("派閥が見つかりませんでした。");
            }
        }

        /// <summary>
        /// プレイヤーが検知半径内にいるかを真偽で返します（敵対かどうかは無関係）。
        /// </summary>
        public bool CheckForPlayerDetection()
        {
            return EmeraldComponent.CombatTarget != null && EmeraldComponent.CombatTarget.CompareTag(EmeraldComponent.DetectionComponent.PlayerTag) || EmeraldComponent.LookAtTarget != null && EmeraldComponent.LookAtTarget.CompareTag(EmeraldComponent.DetectionComponent.PlayerTag);
        }

        /// <summary>
        /// 渡されたAIターゲット（自身でも可）の派閥名を返します。
        /// </summary>
        public string GetTargetFactionName(Transform Target)
        {
            return EmeraldComponent.DetectionComponent.GetTargetFactionName(Target);
        }

        /// <summary>
        /// テスト用：Unity コンソールへ任意メッセージを出力します。
        /// </summary>
        public void DebugLogMessage(string Message)
        {
            Debug.Log(Message);
        }

        /// <summary>
        /// 渡された GameObject を有効化します。
        /// </summary>
        public void EnableObject(GameObject Object)
        {
            Object.SetActive(true);
        }

        /// <summary>
        /// 渡された GameObject を無効化します。
        /// </summary>
        public void DisableObject(GameObject Object)
        {
            Object.SetActive(false);
        }

        /// <summary>
        /// AI をデフォルト状態へリセットします（リスポーン時などに有用）。
        /// </summary>
        public void ResetAI()
        {
            EmeraldComponent.ResetAI();
        }
    }
}
