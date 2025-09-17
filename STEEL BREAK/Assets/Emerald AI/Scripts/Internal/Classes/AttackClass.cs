using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 【AttackClass】
    /// AIの攻撃に関するすべての情報を保持するシリアライズ可能なデータクラス。
    /// ・攻撃データの配列（アビリティ、アニメ番号、距離、クールダウン等）
    /// ・攻撃選択方式（順番/ランダム など AttackPickTypes）
    /// などをまとめて管理します。
    /// </summary>
    [System.Serializable]
    public class AttackClass
    {
        [Header("攻撃データリストの参照開始インデックス（通常は0=先頭）")]
        public int AttackListIndex = 0;

        [Header("攻撃の選択方式（AttackPickTypes を使用。例：Order=順番、Random=ランダム など）")]
        public AttackPickTypes AttackPickType = AttackPickTypes.Order;

        [SerializeField]
        [Header("攻撃データ（アビリティ、アニメ番号、距離、クールダウン設定など）の一覧")]
        public List<AttackData> AttackDataList = new List<AttackData>();

        /// <summary>
        /// 【AttackData】
        /// 単一の攻撃エントリを表すデータ構造。
        /// ・実行するアビリティ
        /// ・再生するアニメーション番号
        /// ・使用距離/至近距離の閾値
        /// ・クールダウン設定
        /// ・抽選の重み（確率）
        /// などを保持します。
        /// </summary>
        [System.Serializable]
        public class AttackData
        {
            [Header("実行するアビリティ（弾・斬撃・魔法等の振る舞いを定義するオブジェクト）")]
            public EmeraldAbilityObject AbilityObject;

            [Header("使用する攻撃アニメーションのインデックス（Animator 側の設定に対応）")]
            public int AttackAnimation;

            [Header("この攻撃が選ばれる確率（%）。合計に対する重みとして扱う")]
            public int AttackOdds = 25;

            [Header("この攻撃が有効となる最大距離。ここまで接近していれば使用可能")]
            public float AttackDistance = 3f;

            [Header("近すぎると使用しない最小距離。これ未満では別の手段へ切り替え")]
            public float TooCloseDistance = 1f;

            [Header("クールダウンを無視するか（true=無視、false=通常のクールダウン適用）")]
            public bool CooldownIgnored;

            [Header("クールダウン終了のタイムスタンプ（Time.time で比較）")]
            public float CooldownTimeStamp;

            /// <summary>
            /// 指定の AttackData がリスト内に含まれているかを判定します。
            /// 注意：この実装は foreach の最初の要素のみを評価して即時 return するため、
            /// リスト全体の完全な検索にはなっていません（元ソース尊重のため挙動は変更しません）。
            /// </summary>
            public bool Contains(List<AttackData> m_AttackDataList, AttackData m_AttackDataClass)
            {
                foreach (AttackData AttackInfo in m_AttackDataList)
                {
                    return (AttackInfo == m_AttackDataClass);
                }

                return false;
            }
        }
    }
}
