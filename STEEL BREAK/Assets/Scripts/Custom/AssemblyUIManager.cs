using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// パーツ選択 UI 全体を制御するマネージャー。
/// タブに応じたパーツリストを表示し、選択されたパーツを Mech に装着する。
/// </summary>
public class AssemblyUIManager : MonoBehaviour
{
    [Header("参照")]

    // パーツリストを表示する親コンテナ（スクロールビューのContentなど）
    public Transform partListParent;

    // パーツリストのアイテムプレハブ（PartListItemスクリプトがアタッチされている）
    public GameObject listItemPrefab;

    // Mechにパーツを装着するマネージャー（実体側のモデル制御）
    public MechAssemblyManager mechAssemblyManager;

    [Header("パーツデータ")]

    // 全てのパーツのデータ（ScriptableObjectなどで登録されている）
    public List<PartData> allParts;

    // 現在選択中のタブ（部位種別）
    private PartType currentTabType;

    [SerializeField] private MechSaveLoader saveLoader;


    /// <summary>
    /// 現在のタブに対応するパーツリストを表示し直す。
    /// </summary>
    public void RefreshPartList()
    {
        // ① 現在表示されているリストアイテムをすべて削除
        foreach (Transform child in partListParent)
        {
            Destroy(child.gameObject);
        }

        PartData[] filteredParts;

        // ② 特例処理：WeaponL の場合、Weapon タイプのパーツも一緒に表示する
        if (currentTabType == PartType.WeaponL)
        {
            filteredParts = allParts
                .Where(p => p.partType == PartType.WeaponL || p.partType == PartType.Weapon)
                .ToArray();
        }
        else
        {
            // ③ 通常：選択されたタブと一致するタイプのパーツだけを抽出
            filteredParts = allParts
                .Where(p => p.partType == currentTabType)
                .ToArray();
        }

        // ④ 抽出したパーツを元に、UIリストアイテムを生成して並べる
        foreach (var part in filteredParts)
        {
            GameObject item = Instantiate(listItemPrefab, partListParent); // プレハブを生成して親にセット
            item.GetComponent<PartListItem>().Setup(part, this);           // 各アイテムにパーツ情報を設定
        }
    }

    /// <summary>
    /// タブが選択されたときに呼ばれる。PartTypeに応じてリストを更新。
    /// </summary>
    /// <param name="no">PartTypeのint値（Enumの番号）</param>
    public void OnTabSelected(int no)
    {
        // ① 渡された番号が PartType に変換可能かをチェック
        if (System.Enum.IsDefined(typeof(PartType), no))
        {
            // ② 正常な番号なら PartType に変換して保存
            currentTabType = (PartType)no;

            // ③ リストを再構築
            RefreshPartList();
        }
        else
        {
            // 異常値のときは警告を出力
            Debug.LogWarning($"無効な PartType の番号: {no}");
        }
    }

    /// <summary>
    /// パーツがリストから選択されたときに呼ばれる。Mech に装着処理を依頼。
    /// </summary>
    /// <param name="part">選択された PartData</param>
    public void OnPartSelected(PartData part)
    {
        // Mechにパーツを装着
        mechAssemblyManager.AttachPart(part, currentTabType);

        // 必要であればステータスUI更新などの追加処理をここで実行可能
    }

    // WeaponL ボタンが押されたとき
    public void OnWeaponLButtonClicked(PartData selectedWeapon)
    {
        mechAssemblyManager.AttachPart(selectedWeapon, currentTabType);
    }

    public void OnClickSave()
    {
        GameData.mechSaveData.Save();
    }

}