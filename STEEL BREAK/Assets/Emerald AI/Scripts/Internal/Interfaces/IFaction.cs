using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///
/// 【IFaction】
/// オブジェクト（AIやキャラクターなど）の「派閥インデックス」を取得するためのインターフェイス。
/// 派閥インデックスは、Faction Data の「Faction Name List（派閥名リスト）」の並び順と対応します。
///
public interface IFaction
{
    /// <summary>
    /// オブジェクトの派閥インデックスを返します。
    /// 値は Faction Data の「Faction Name List（派閥名リスト）」の要素インデックスに対応します。
    /// </summary>
    int GetFaction();
}
