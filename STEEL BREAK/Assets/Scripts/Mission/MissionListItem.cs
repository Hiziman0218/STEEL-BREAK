using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ミッション一覧のリストアイテム。MissionData を受け取り、選択時に親の MissionSelectManager に通知する。
/// プレハブには Button コンポーネントが必要です（Inspector で設定）。
/// </summary>
[RequireComponent(typeof(Button))]
public class MissionListItem : MonoBehaviour
{
    [Header("表示UI")]
    public TextMeshProUGUI nameText;
    public Image thumbImage; // (任意) サムネイルを表示する Image（あれば設定）

    private MissionData missionData;
    private MissionSelectManager selectManager;

    /// <summary>
    /// リストアイテムをセットアップする
    /// </summary>
    public void Setup(MissionData data, MissionSelectManager manager)
    {
        missionData = data;
        selectManager = manager;

        if (nameText != null)
            nameText.text = data.missionName ?? data.missionID ?? "(No name)";

        if (thumbImage != null)
        {
            if (data.missionImage != null)
            {
                thumbImage.sprite = data.missionImage;
                thumbImage.gameObject.SetActive(true);
            }
            else
            {
                thumbImage.gameObject.SetActive(false);
            }
        }

        // ボタンのクリックイベントを追加
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (selectManager != null && missionData != null)
        {
            selectManager.SelectMission(missionData);
        }
    }
}


//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//[RequireComponent(typeof(Button))]
//public class MissionListItem : MonoBehaviour
//{
//    public TextMeshProUGUI nameText;

//    private MissionData missionData;
//    private MissionSelectManager selectManager;

//    public void Setup(MissionData data, MissionSelectManager manager)
//    {
//        missionData = data;
//        selectManager = manager;

//        if (nameText != null)
//        {
//            nameText.text = data.missionName;
//        }

//        // 自動的にこのボタンにクリックイベントを追加
//        var button = GetComponent<Button>();
//        if (button != null)
//        {
//            button.onClick.RemoveAllListeners();
//            button.onClick.AddListener(OnClick);
//        }
//    }

//    public void OnClick()
//    {
//        if (selectManager != null && missionData != null)
//        {
//            selectManager.SelectMission(missionData);
//        }
//    }
//}
