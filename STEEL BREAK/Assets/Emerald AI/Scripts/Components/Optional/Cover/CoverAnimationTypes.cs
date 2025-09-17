using UnityEngine;  // Unity の基本API（このファイルでは直接の参照はないが、プロジェクト整合のため保持）

namespace EmeraldAI // EmeraldAI に属する名前空間
{
    // 【列挙型の概要】CoverTypes：AI のカバー（遮蔽）動作に対応したアニメーション種別を表す列挙
    public enum CoverTypes
    {
        CrouchAndPeak, // しゃがみ＋ピーク（遮蔽物から身を乗り出して覗き込む）動作
        CrouchOnce,    // 一度だけしゃがむ（短時間のカバー／最小しゃがみ）動作
        Stand          // 立ったままのカバー（しゃがまない）動作
    }
}
