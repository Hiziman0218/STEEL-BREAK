using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 【FactionExtension】
    /// 任意の GameObject に「派閥（Faction）」情報を付与する拡張。
    /// Emerald AI の検知・関係性判定で使用される派閥IDを保有します。
    /// </summary>
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/getting-started/setting-up-a-player-with-emerald-ai")] // 公式Wiki（ヘルプURL）
    public class FactionExtension : MonoBehaviour, IFaction
    {
        [Header("設定折りたたみを隠す（エディタ用）")]
        public bool HideSettingsFoldout;

        [Header("派閥設定の折りたたみ（エディタ用）")]
        public bool FactionFoldout = true;

        [Header("現在の派閥ID（整数インデックス）")]
        [SerializeField] public int CurrentFaction = 0;

        [Header("派閥名の文字列リスト（全AIで共有される静的リスト）")]
        [SerializeField] public static List<string> StringFactionList = new List<string>();

        /// <summary>
        /// このオブジェクトに設定された派閥IDを取得します。
        /// </summary>
        public int GetFaction()
        {
            return CurrentFaction;
        }
    }
}
