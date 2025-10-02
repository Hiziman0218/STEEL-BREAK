// ===============================================================
// ファイル名 : EmeraldAbilityObject.cs
// 目的     : Emerald AI 2025 の「アビリティ定義オブジェクト」基底クラス（ScriptableObject）
// 注意     : ご主人様の指示により実行ロジックは一切変更せず、コメントとEditor用属性（Header/Tooltip）のみ追加
// ポリシー : ・すべてのメンバー変数に [Header("…")] を付与
//            ・Tooltip は日本語で記述
//            ・クラス宣言の直前に日本語の用途注釈
//            ・可能な範囲で行コメント（過度な冗長化は避けつつ可読性重視）
//            ・本ファイルには Debug.Log 系のログ出力は存在しない（＝ログ注釈対象なし）
// ===============================================================

using System.Collections;          // コルーチン等（本クラスでは直接未使用だが慣例的に参照）
using System.Collections.Generic;  // List などのコレクション（将来拡張含む）
using UnityEngine;                 // UnityEngine（ScriptableObject/Texture2D など）
using System.Linq;                 // LINQ（ターゲット選択ロジック拡張などで利用可能）

namespace EmeraldAI
{
    /// <summary>
    /// 【EmeraldAbilityObject】
    /// Emerald AI の「アビリティ」を表す ScriptableObject の基底クラス。
    /// ・アビリティ名/説明/アイコンなどのメタ情報
    /// ・クールダウンや発動条件（モジュールデータ）
    /// ・詠唱（Charge）と発動（Invoke）の仮想メソッド
    /// ・ターゲット取得ユーティリティ
    /// を提供する。派生クラスで各アビリティ固有の挙動を実装する。
    /// </summary>
    public class EmeraldAbilityObject : ScriptableObject
    {
        // ===== メタ情報（インスペクタ表示用） =====
        [Header("アビリティ名（インスペクタ表示/任意UI表示用）")]
        [Tooltip("このアビリティの表示名。ゲーム内UIなどでも使用可能です。")]
        public string AbilityName = "New Ability";           // 既定名

        [Header("アビリティの説明文（ツールチップ/ヘルプなど）")]
        [Tooltip("このアビリティの説明文。プレイヤー向けヘルプやデバッグ用途にも利用できます。")]
        public string AbilityDescription = "Ability Description";  // 既定説明

        [Header("アビリティのアイコン（UI表示用）")]
        [Tooltip("インベントリ/ホットバー/クールダウン表示などで使うアイコン画像。")]
        public Texture2D AbilityIcon;                        // アイコン

        // ===== エディタ用フォールドアウト（折り畳み）フラグ =====
        [Header("【Editor】情報セクションの折り畳み状態")]
        [Tooltip("インスペクタ上の『情報（Info）』セクションの折り畳み状態を保持します。")]
        public bool InfoSettingsFoldout = true;              // 既定で展開

        [Header("【Editor】派生設定セクションの折り畳み状態")]
        [Tooltip("インスペクタ上の『Derived（派生）』セクションの折り畳み状態を保持します。")]
        public bool DerivedSettingsFoldout;                  // 既定で非展開

        [Header("【Editor】モジュール設定セクションの折り畳み状態")]
        [Tooltip("インスペクタ上の『Modular（モジュール）』セクションの折り畳み状態を保持します。")]
        public bool ModularSettingsFoldout;                  // 既定で非展開

        [Header("【Editor】非表示設定セクションの折り畳み状態")]
        [Tooltip("インスペクタ上の『Hide（非表示）』セクションの折り畳み状態を保持します。")]
        public bool HideSettingsFoldout;                     // 既定で非展開

        /// <summary>
        /// 【内部利用】クールダウン管理用のモジュールデータ。
        /// カスタムアビリティにクールダウンが必要な場合に推奨されるが、必須ではない（任意）。
        /// </summary>
        [Header("クールダウン設定（任意：内部利用を推奨）")]
        [Tooltip("アビリティのクールダウン制御に使用する設定。カスタムアビリティでクールダウンが必要な場合に利用します（任意）。")]
        public AbilityData.CooldownData CooldownSettings;    // クールダウン設定

        [Header("発動条件（HP/距離/召喚数などの条件式）")]
        [Tooltip("アビリティを発動するための条件設定。自己低HP/味方低HP/対象距離/召喚個数0などを指定できます。")]
        public AbilityData.ConditionData ConditionSettings;  // 発動条件

        // ===== 仮想メソッド：派生クラスで実装 =====

        /// <summary>
        /// 詠唱（チャージ）処理。アニメーション/エフェクト/SEの準備などを派生先で実装。
        /// </summary>
        /// <param name="Owner">このアビリティを所有するAI/プレイヤー。</param>
        /// <param name="AttackTransform">発射点やエフェクト起点（任意）。</param>
        public virtual void ChargeAbility(GameObject Owner, Transform AttackTransform = null) { }  // 既定は何もしない

        /// <summary>
        /// 実際のアビリティ発動処理。弾/エフェクト/回復/召喚などを派生先で実装。
        /// </summary>
        /// <param name="Owner">このアビリティを所有するAI/プレイヤー。</param>
        /// <param name="AttackTransform">発射点やエフェクト起点（任意）。</param>
        public virtual void InvokeAbility(GameObject Owner, Transform AttackTransform = null) { }  // 既定は何もしない

        /// <summary>
        /// アビリティの「対象」を取得するユーティリティ。
        /// AbilityData.TargetTypes に応じて、現在ターゲット/ランダム敵などを返す。
        /// </summary>
        /// <param name="Owner">このアビリティの所有者。</param>
        /// <param name="TargetType">対象の取り方（CurrentTarget/SingleRandomEnemy/MultipleRandomEnemies）。</param>
        /// <returns>選定された Transform（見つからなければ null）。</returns>
        public virtual Transform GetTarget(GameObject Owner, AbilityData.TargetTypes TargetType)
        {
            Transform Target = null;                                       // 返却用

            EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>(); // 所有者の EmeraldSystem を取得

            if (TargetType == AbilityData.TargetTypes.MultipleRandomEnemies || TargetType == AbilityData.TargetTypes.SingleRandomEnemy)
            {
                // 視線上のターゲット群からランダムに選ぶ（1体）。複数対象の実射は派生側でループ生成などを想定。
                if (EmeraldComponent.DetectionComponent.LineOfSightTargets.Count > 0)
                    Target = EmeraldComponent.DetectionComponent.LineOfSightTargets[Random.Range(0, EmeraldComponent.DetectionComponent.LineOfSightTargets.Count)].transform;
            }
            else if (TargetType == AbilityData.TargetTypes.CurrentTarget)
            {
                // 現在の戦闘ターゲットをそのまま返す
                Target = EmeraldComponent.CombatTarget;
            }

            return Target;                                                 // 最終的な対象（null の場合は呼び出し側でケア）
        }
    }
}
