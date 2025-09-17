using System.Collections;                         // （保持）コルーチン用（本クラスでは未使用）
using System.Collections.Generic;                 // （保持）汎用コレクション（本クラスでは未使用）
using UnityEngine;                                // UnityEngine の属性（Header）や AudioClip を使用

namespace EmeraldAI.SoundDetection                 // サウンド検知システム関連の名前空間
{
    [System.Serializable]                          // シリアライズ可能：インスペクタに表示されるデータクラス

    // 【クラス概要】Reaction：
    //  サウンド検知や誘引（Attract）システムが実行する「1つの反応項目」を表すデータコンテナ。
    //  ReactionType に応じて、下の各フィールド（Int/Float/String/Bool/SoundRef など）の意味が切り替わる。
    //  例）Delay なら FloatValue=待機秒、PlaySound なら SoundRef=再生クリップ + FloatValue=音量、等。
    public class Reaction
    {
        [Header("リアクションの種類（何を行うか）")] // 例：Delay / PlaySound / MoveToLoudestTarget など
        public ReactionTypes ReactionType = ReactionTypes.None;

        [Header("整数値1（用途は ReactionType に依存）")] // 例：秒数・回数・Waypoints 数・角度など
        public int IntValue1 = 5;

        [Header("整数値2（用途は ReactionType に依存）")] // 例：半径・追加パラメータなど
        public int IntValue2 = 2;

        [Header("文字列値（用途は ReactionType に依存）")] // 例：DebugLogMessage で出力するメッセージ
        public string StringValue = "New Message";

        [Header("小数値（用途は ReactionType に依存）")]   // 例：Delay の待機秒・音量・待機時間など
        public float FloatValue = 1f;

        [Header("真偽値（用途は ReactionType に依存）")]   // 例：到達まで待機するか、フラグのON/OFF など
        public bool BoolValue = true;

        [Header("サウンド参照（PlaySound 用の AudioClip）")]
        public AudioClip SoundRef;

        [Header("Attract Modifier の挙動種類")]             // 例：音源へ移動 / 周囲を回る / 見つめる
        public AttractModifierReactionTypes AttractModifierReaction = AttractModifierReactionTypes.MoveToAttractSource;

        [Header("移動状態（SetMovementState で使用）")]     // Walk / Run / Sprint など
        public EmeraldMovement.MovementStates MovementState = EmeraldMovement.MovementStates.Walk;

        [Header("エディタ表示用：この要素の行の高さ")]      // インスペクタ描画上の行高を指定
        public ElementLineHeights ElementLineHeight = ElementLineHeights.One;

        // （補足）ElementLineHeights は UI 上のレイアウト調整用で、ゲーム挙動には影響しない
        public enum ElementLineHeights
        {
            One,    // 1行分
            Two,    // 2行分
            Three,  // 3行分
            Four,   // 4行分
            Five,   // 5行分
            Six     // 6行分
        }
    }
}
