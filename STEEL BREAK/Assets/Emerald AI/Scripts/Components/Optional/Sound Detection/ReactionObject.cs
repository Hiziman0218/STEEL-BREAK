using System.Collections;                         // （保持）コルーチン用（本ファイルでは未使用）
using System.Collections.Generic;                 // List<T> を使用するため
using UnityEngine;                                // ScriptableObject / 属性 を使用

namespace EmeraldAI.SoundDetection                 // サウンド検知関連の名前空間
{
    /// <summary>
    /// （日本語）サウンド検知/誘引システムで使用する「反応（Reaction）」のセットを格納する ScriptableObject。
    /// エディタからアセットとして作成し、EmeraldSoundDetector や AttractModifier などに割り当てて使用します。
    /// </summary>
    [CreateAssetMenu(fileName = "Reaction Object", menuName = "Emerald AI/Reaction Object")] // アセット作成メニュー（挙動維持のため英語のまま）
    [System.Serializable]                          // シリアライズ可能（インスペクタに表示）
    // 【クラス概要】ReactionObject：
    //  ・複数の Reaction（Delay/PlaySound/移動/注視 など）を順番に保持する入れ物。
    //  ・実行側（EmeraldSoundDetector 等）が ReactionList を順次処理して挙動を実現します。
    public class ReactionObject : ScriptableObject
    {
        [SerializeField]                           // インスペクタ表示用（外部からの直接代入は非推奨）
        [Header("このリアクションオブジェクトが保持する反応要素の一覧（実行順）")]
        public List<Reaction> ReactionList = new List<Reaction>(); // 反応のリスト。上から順に実行されます。
    }
}
