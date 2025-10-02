using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【AnimationProfile】
    /// Emerald AI 用のアニメーション設定をまとめた ScriptableObject。
    /// インスペクター上での視認性向上のため、各メンバーに [Header] を付与し、日本語説明を追記しています。
    /// </summary>
    [CreateAssetMenu(fileName = "アニメーションプロファイル", menuName = "Emerald AI/アニメーション/アニメーションプロファイル")]
    public class AnimationProfile : ScriptableObject
    {
        [Header("対象AIのアニメーション制御コンポーネント参照 (EmeraldAnimation)")]
        public EmeraldAnimation EmeraldAnimationComponent;

        [Header("このプロファイルに使用する RuntimeAnimatorController")]
        public RuntimeAnimatorController AIAnimator;

        [Header("AnimatorController を自動生成済みか (内部状態)")]
        public bool AnimatorControllerGenerated;

        [Header("アニメーションが最新状態へ更新済みか (内部フラグ)")]
        public bool AnimationsUpdated;

        [Header("アニメーションクリップ一覧に変更があったか (内部フラグ)")]
        public bool AnimationListsChanged = false;

        [Header("RuntimeAnimatorController が未設定/見失い状態か (警告用)")]
        public bool MissingRuntimeController;

        [Header("このプロファイルのアセットファイルパス (エディタ管理用)")]
        public string FilePath;

        [Header("Animator の Culling Mode (常に更新/画面外で停止 など)")]
        public AnimatorCullingMode AnimatorCullingMode = AnimatorCullingMode.AlwaysAnimate;

        [Header("非戦闘: Walk セクションの折りたたみ表示フラグ")]
        public bool WalkFoldout;

        [Header("非戦闘: Run セクションの折りたたみ表示フラグ")]
        public bool RunFoldout;

        [Header("非戦闘: Turn セクションの折りたたみ表示フラグ")]
        public bool TurnFoldout;

        [Header("非戦闘: Death (死亡) セクションの折りたたみ表示フラグ")]
        public bool NonCombatDeathFoldout;

        [Header("非戦闘: アニメーション一覧セクションの折りたたみ表示フラグ")]
        public bool NonCombatAnimationsFoldout;

        [Header("非戦闘: Idle セクションの折りたたみ表示フラグ")]
        public bool NonCombatIdleFoldout;

        [Header("非戦闘: Hit (被弾) セクションの折りたたみ表示フラグ")]
        public bool NonCombatHitFoldout;

        [Header("非戦闘: Emotes (感情表現) セクションの折りたたみ表示フラグ")]
        public bool EmotesFoldout;

        [Header("タイプ1: Idle セクションの折りたたみ表示フラグ")]
        public bool Type1IdleFoldout;

        [Header("タイプ2: Idle セクションの折りたたみ表示フラグ")]
        public bool Type2IdleFoldout;

        [Header("タイプ1: Attacks (攻撃) セクションの折りたたみ表示フラグ")]
        public bool Type1AttacksFoldout;

        [Header("タイプ2: Attacks (攻撃) セクションの折りたたみ表示フラグ")]
        public bool Type2AttacksFoldout;

        [Header("タイプ1: Equips (装備/抜刀納刀など) セクションの折りたたみ表示フラグ")]
        public bool Type1EquipsFoldout;

        [Header("タイプ2: Equips (装備/抜刀納刀など) セクションの折りたたみ表示フラグ")]
        public bool Type2EquipsFoldout;

        [Header("タイプ1: Combat Animations (戦闘) セクションの折りたたみ表示フラグ")]
        public bool Type1CombatAnimationsFoldout;

        [Header("タイプ2: Combat Animations (戦闘) セクションの折りたたみ表示フラグ")]
        public bool Type2CombatAnimationsFoldout;

        [Header("タイプ1: Death (死亡) セクションの折りたたみ表示フラグ")]
        public bool Type1DeathFoldout;

        [Header("タイプ2: Death (死亡) セクションの折りたたみ表示フラグ")]
        public bool Type2DeathFoldout;

        [Header("タイプ1: Hit (被弾) セクションの折りたたみ表示フラグ")]
        public bool Type1HitFoldout;

        [Header("タイプ2: Hit (被弾) セクションの折りたたみ表示フラグ")]
        public bool Type2HitFoldout;

        [Header("タイプ1: Block (ガード) セクションの折りたたみ表示フラグ")]
        public bool Type1BlockFoldout;

        [Header("タイプ2: Block (ガード) セクションの折りたたみ表示フラグ")]
        public bool Type2BlockFoldout;

        [Header("タイプ1: Combat Walk (戦闘時歩行) セクションの折りたたみ表示フラグ")]
        public bool Type1CombatWalkFoldout;

        [Header("タイプ1: Combat Run (戦闘時走行) セクションの折りたたみ表示フラグ")]
        public bool Type1CombatRunFoldout;

        [Header("タイプ1: Combat Turn (戦闘時旋回) セクションの折りたたみ表示フラグ")]
        public bool Type1CombatTurnFoldout;

        [Header("タイプ2: Combat Walk (戦闘時歩行) セクションの折りたたみ表示フラグ")]
        public bool Type2CombatWalkFoldout;

        [Header("タイプ2: Combat Run (戦闘時走行) セクションの折りたたみ表示フラグ")]
        public bool Type2CombatRunFoldout;

        [Header("タイプ2: Combat Turn (戦闘時旋回) セクションの折りたたみ表示フラグ")]
        public bool Type2CombatTurnFoldout;

        [Header("タイプ1: Strafe (平行移動) セクションの折りたたみ表示フラグ")]
        public bool Type1StrafeFoldout;

        [Header("タイプ2: Strafe (平行移動) セクションの折りたたみ表示フラグ")]
        public bool Type2StrafeFoldout;

        [Header("タイプ1: Dodge (回避) セクションの折りたたみ表示フラグ")]
        public bool Type1DodgeFoldout;

        [Header("タイプ2: Dodge (回避) セクションの折りたたみ表示フラグ")]
        public bool Type2DodgeFoldout;

        [Header("タイプ1: Cover (遮蔽物行動) セクションの折りたたみ表示フラグ")]
        public bool Type1CoverFoldout;

        [Header("タイプ2: Cover (遮蔽物行動) セクションの折りたたみ表示フラグ")]
        public bool Type2CoverFoldout;

        [Header("アニメーションプロファイル全体セクションの折りたたみ表示フラグ")]
        public bool AnimationProfileFoldout;

        [Header("Animator 設定セクションの折りたたみ表示フラグ")]
        public bool AnimatorSettingsFoldout;

        [Header("タイプ1: Hit(被弾) アニメーションの再生条件 (Everything 等)")]
        public AnimationStateTypes Type1HitConditions = AnimationStateTypes.Everything;

        [Header("タイプ2: Hit(被弾) アニメーションの再生条件 (Everything 等)")]
        public AnimationStateTypes Type2HitConditions = AnimationStateTypes.Everything;

        [Header("タイプ1: Hit(被弾) アニメーションのクールダウン(秒)")]
        public float Type1HitAnimationCooldown = 0.1f;

        [Header("タイプ2: Hit(被弾) アニメーションのクールダウン(秒)")]
        public float Type2HitAnimationCooldown = 0.1f;

        [Header("エモート(感情表現) アニメーションのリスト")]
        public List<EmoteAnimationClass> EmoteAnimationList = new List<EmoteAnimationClass>();

        [Header("非戦闘アニメーション一式 (親クラス)")]
        [SerializeField]
        public AnimationParentClass NonCombatAnimations;

        [Header("タイプ1アニメーション一式 (親クラス)")]
        [SerializeField]
        public AnimationParentClass Type1Animations;

        [Header("タイプ2アニメーション一式 (親クラス)")]
        [SerializeField]
        public AnimationParentClass Type2Animations;
    }
}
