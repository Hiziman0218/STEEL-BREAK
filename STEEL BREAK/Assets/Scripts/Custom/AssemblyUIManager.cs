using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// パーツ選択 UI 全体を制御するマネージャー。
/// タブに応じたパーツリストを表示し、選択されたパーツを Mech に装着する。
/// </summary>
public class AssemblyUIManager : MonoBehaviour
{
    public static AssemblyUIManager instance;

    private void Awake()
    {
        instance = this;
    }

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

    public TextMeshProUGUI apValueText;  // 右下の AP 表示 Text
    public TextMeshProUGUI weightValueText;  // 右下の Weight 表示 Text

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
        else if (currentTabType == PartType.BWeaponL)
        {
            filteredParts = allParts
                .Where(p => p.partType == PartType.BWeaponL || p.partType == PartType.BWeapon)
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

        // AP 合計を UI に反映
        UpdateAPDisplay();
        UpdateWeightDisplay();
    }

    // WeaponL ボタンが押されたとき
    public void OnWeaponLButtonClicked(PartData selectedWeapon)
    {
        mechAssemblyManager.AttachPart(selectedWeapon, currentTabType);
    }

    private void UpdateAPDisplay()
    {
        int totalAP = mechAssemblyManager.GetTotalAP();
        apValueText.text = totalAP.ToString();
    }

    private void UpdateWeightDisplay()
    {
        int totalweight = mechAssemblyManager.GetTotalweight();
        weightValueText.text = totalweight.ToString();
    }

    /// <summary>
    /// ステータスUI（AP・重量）を更新する
    /// </summary>
    public void UpdateStatusUI(int totalAP ,int totalWeight)
    {
        apValueText.text = totalAP.ToString();
        weightValueText.text = totalWeight.ToString();
    }


    public void OnClickSave()
    {
        GameData.mechSaveData.Save();
    }

}