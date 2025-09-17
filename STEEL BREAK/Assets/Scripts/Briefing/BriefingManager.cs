using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BriefingManager : MonoBehaviour
{
    [Header("基本情報UI")]
    public TextMeshProUGUI missionTitleText;
    public TextMeshProUGUI clientText;
    public TextMeshProUGUI stageNameText;
    public TextMeshProUGUI rewardAmountText;

    [Header("画像UI")]
    public Image companyImage;
    public Image missionImage;

    [Header("目標UI（3つ）")]
    public TextMeshProUGUI[] objectiveTexts; // 配列で3つ分のTextを設定

    [Header("メッセージ表示")]
    public TypeWriterEffect messageTyper;

    [Header("UIオブジェクト")]
    public GameObject briefingUI;  // ブリーフィング全体UI親
    public GameObject selectionUI; // 次に表示したいUI親

    void Start()
    {
        var mission = GameData.currentSelected;
        if (mission == null)
        {
            Debug.LogError("MissionDataが設定されていません");
            return;
        }

        // 基本テキスト
        missionTitleText.text = mission.missionName;
        //clientText.text = $"依頼者: {mission.client}";
        stageNameText.text = mission.stageName;
        rewardAmountText.text = $"{mission.rewardAmount:N0}";

        // 画像
        if (companyImage != null) companyImage.sprite = mission.companyImage;
        if (missionImage != null) missionImage.sprite = mission.missionImage;

        // 目標（3つ）
        for (int i = 0; i < objectiveTexts.Length; i++)
        {
            if (i < mission.objectives.Length && i < mission.objectiveAmounts.Length)
            {
                objectiveTexts[i].text = $"{mission.objectives[i]}：¥{mission.objectiveAmounts[i]:N0}";
            }
            else
            {
                objectiveTexts[i].text = "";
            }
        }

        // メッセージ（タイプライター）開始＆終了コールバック登録
        if (messageTyper != null)
        {
            messageTyper.OnTypingFinished += OnMessageFinished;
            messageTyper.StartTyping(mission.messages);
        }
    }

    private void OnMessageFinished()
    {
        // ブリーフィングUIを非表示に
        if (briefingUI != null) briefingUI.SetActive(false);

        // 選択UIを表示
        if (selectionUI != null) selectionUI.SetActive(true);

        // イベント解除
        if (messageTyper != null)
        {
            messageTyper.OnTypingFinished -= OnMessageFinished;
        }
    }
}
