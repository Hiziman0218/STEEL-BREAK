using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// パーツリストの各項目を管理するクラス（UIアイテム）
/// </summary>
public class PartListItem : MonoBehaviour
{
    // パーツのアイコン画像を表示するUI
    [SerializeField] private Image icon;

    // パーツ名を表示するTextMeshPro UI
    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private Button selectButton;

    // このリストアイテムが参照するパーツデータ
    private PartData partData;

    // パーツ選択UIを制御するマネージャーへの参照
    private AssemblyUIManager uiManager;

    /// <summary>
    /// パーツデータとUIマネージャーを設定し、表示を初期化
    /// </summary>
    /// <param name="data">表示対象のパーツデータ</param>
    /// <param name="manager">UIマネージャー</param>
    public void Setup(PartData data, AssemblyUIManager manager)
    {
        partData = data;
        uiManager = manager;

        // UIにアイコンと名前を設定
        icon.sprite = partData.partIcon;
        nameText.text = partData.partName;

        // ButtonコンポーネントにOnClickを登録
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelect);
    }

    /// <summary>
    /// プレイヤーがこのパーツを選択したときに呼ばれる
    /// </summary>
    public void OnSelect()
    {
        // UIマネージャーに選択されたパーツを通知
        uiManager.OnPartSelected(partData);
    }
}
