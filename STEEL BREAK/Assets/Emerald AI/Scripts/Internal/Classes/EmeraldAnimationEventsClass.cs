using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 【EmeraldAnimationEventsClass】
    /// アニメーション設定UIで表示するイベント名・説明文・AnimationEvent本体を1セットとして保持するデータクラス。
    /// </summary>
    public class EmeraldAnimationEventsClass
    {
        [Header("UIやリスト上で表示するイベント名（ユーザー向けの見出し）")]
        public string eventDisplayName;         // イベントの表示名

        [Header("イベントの説明文（使い方・注意点など）")]
        public string eventDescription;         // イベントの解説テキスト

        [Header("UnityのAnimationEvent実体（functionName や各種パラメータを保持）")]
        public AnimationEvent animationEvent;   // 実際に呼び出される AnimationEvent

        /// <summary>
        /// コンストラクタ：表示名、AnimationEvent、説明文を受け取り初期化します。
        /// </summary>
        public EmeraldAnimationEventsClass(string m_eventDisplayName, AnimationEvent m_animationEvent, string m_eventDescription)
        {
            eventDisplayName = m_eventDisplayName;   // 表示名を設定
            animationEvent = m_animationEvent;       // AnimationEvent を設定
            eventDescription = m_eventDescription;   // 説明文を設定
        }
    }
}
