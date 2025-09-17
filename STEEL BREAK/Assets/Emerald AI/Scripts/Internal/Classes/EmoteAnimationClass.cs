using UnityEngine;  // Unity の基本API

namespace EmeraldAI  // EmeraldAI 用の名前空間
{
    /// <summary>
    /// 【EmoteAnimationClass】
    /// 単一の「エモート（感情表現）アニメーション」を定義するシリアライズ可能なデータクラスです。  // クラスの用途説明
    /// ・AnimationID（識別ID）と、実際に再生する AnimationClip を1セットで保持します。             // 保持要素の概要
    /// ・エディタや外部データから参照しやすいよう、シンプルな構造になっています。                     // 設計意図
    /// </summary>
    [System.Serializable]  // インスペクタ表示/保存を可能にする属性
    public class EmoteAnimationClass  // クラス定義の開始
    {
        /// <summary>
        /// コンストラクタ：エモートIDとアニメーションクリップを受け取って初期化します。                // コンストラクタの説明
        /// </summary>
        public EmoteAnimationClass(int NewAnimationID, AnimationClip NewEmoteAnimationClip)  // コンストラクタ宣言
        {
            AnimationID = NewAnimationID;               // 受け取ったIDを反映
            EmoteAnimationClip = NewEmoteAnimationClip; // 受け取ったクリップ参照を反映
        }

        [Header("このエモートを識別するID（外部データやUIからの参照用）。既定値=1")]  // 変数の説明（インスペクタ見出し）
        public int AnimationID = 1;  // エモートの識別ID

        [Header("再生するエモート用 AnimationClip（モーション本体の参照）")]            // 変数の説明（インスペクタ見出し）
        public AnimationClip EmoteAnimationClip;  // エモートのアニメーションクリップ参照
    }  // クラス定義の終わり
}  // 名前空間の終わり
