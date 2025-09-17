using System.Collections.Generic;               // List（リスト）を使用するための名前空間
using UnityEngine;                               // Unity の基盤API

namespace EmeraldAI                                 // EmeraldAI 用の名前空間
{
    /// <summary>
    /// 【派閥データ（ScriptableObject）】
    /// ゲーム内で使用する「派閥名」の一覧を保持するデータコンテナ。
    /// インスペクタから要素を追加・編集して、AIやシステムの派閥判定に利用する。
    /// </summary>
    [System.Serializable]                           // シリアライズ可能であることを示す属性（保存・表示向け）
    public class EmeraldFactionData : ScriptableObject   // ScriptableObject として扱うデータ資産クラス
    {
        [SerializeField]                             // 非publicでもシリアライズ化する属性（ここでは public だが元ソースを尊重し保持）
        [Header("派閥名の一覧（文字列）。AI/システムが参照する派閥名をここで管理します。重複や空文字は避けることを推奨")]
        /// <summary>
        /// 派閥名を保持するリスト。インデックス順や命名規則はプロジェクトルールに従う。
        /// 例）「Player」「Neutral」「Enemy」「Merchant」など
        /// </summary>
        public List<string> FactionNameList = new List<string>(); // 派閥名リストの実体（デフォルトは空リスト）
    }
}
