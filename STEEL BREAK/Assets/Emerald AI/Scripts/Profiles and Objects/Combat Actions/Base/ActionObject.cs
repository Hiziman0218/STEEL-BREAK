using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// モジュール式のアクションコンポーネント。
    /// 継承してカスタム AI アクションを作成するための基底 ScriptableObject です。
    /// </summary>
    [System.Serializable]
    public class EmeraldAction : ScriptableObject
    {
        #region Emerald Action Variables
        [Header("このアクションに『入れる／開始できる』状態（AnimationStateTypes のフラグ）")]
        [Tooltip("このアクションに遷移して開始できる状態を指定します。")]
        public AnimationStateTypes EnterConditions = AnimationStateTypes.None;

        [Header("このアクションを『終了／キャンセル』できる状態（AnimationStateTypes のフラグ）")]
        [Tooltip("このアクションを終了またはキャンセルできる状態を指定します。")]
        public AnimationStateTypes ExitConditions = AnimationStateTypes.None;

        [Header("クールダウンが経過することを許可する状態（AnimationStateTypes のフラグ）")]
        [Tooltip("このアクションのクールダウンタイマーが進むことを許可する状態を指定します。")]
        public AnimationStateTypes CooldownConditions = AnimationStateTypes.None;

        [Header("クールダウンの長さ（秒）。EnterConditions が満たされると再使用可能になるまでの時間")]
        [Range(0.25f, 30f)]
        [Tooltip("クールダウンの長さ（秒）。EnterConditions が満たされると再使用可能になるまでの時間です。")]
        public float CooldownLength = 2;

        [Header("クールダウンを使用するか（true で有効）")]
        [Tooltip("このアクションでクールダウンを使用するかどうか。")]
        public bool UseCooldown = true;
        #endregion

        #region Editor Variables
        [Header("インスペクターの『設定を隠す』フラグ（エディタ用）")]
        [HideInInspector] public bool HideSettingsFoldout;

        [Header("インスペクターの『デフォルト設定』セクション折りたたみ状態（エディタ用）")]
        [HideInInspector] public bool DefaultSettingsFoldout = true;

        [Header("インスペクターの『カスタム設定』セクション折りたたみ状態（エディタ用）")]
        [HideInInspector] public bool CustomSettingsFoldout = true;

        [Header("インスペクターの『情報』セクション折りたたみ状態（エディタ用）")]
        [HideInInspector] public bool InfoSettingsFoldout = true;

        [Header("アクション名（エディタ表示用、内部テキスト）")]
        [HideInInspector] public string ActionName;

        [Header("アクションの説明文（Info セクションから編集可能）")]
        [HideInInspector] public string ActionDescription = "アクションの説明は Info セクションから編集できます。";
        #endregion

        /// <summary>
        /// アクションの初期化。Emerald AI 本体や ActionsClass の内部値を設定する際に使用します。
        /// </summary>
        public virtual void InitializeAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass) { }

        /// <summary>
        /// EmeraldAction を継続的に更新します。Update 相当の処理を、このアクションのスコープ内で行うためのフックです（EmeraldComponent と ActionClass の情報を使用）。
        /// </summary>
        public virtual void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass) { }

        /// <summary>
        /// 内部条件（例：AI の死亡など）によりアクションをキャンセルできるタイミングで呼び出されます。
        /// </summary>
        public virtual void CancelAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            EmeraldComponent.AnimationComponent.ResetTriggers(0);
            ActionClass.IsActive = false;
        }
    }
}
