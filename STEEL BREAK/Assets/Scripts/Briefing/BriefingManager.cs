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

    [Header("目標UI")]
    public TextMeshProUGUI[] objectiveTexts;

    [Header("メッセージ表示")]
    public TypeWriterEffect messageTyper;
    public AudioSource voiceSource;

    [Header("UIオブジェクト")]
    public GameObject briefingUI;
    public GameObject selectionUI;

    private string[] voices;
    private string[] messages;

    void Start()
    {
        var mission = GameData.currentSelected;
        if (mission == null)
        {
            Debug.LogError("[BriefingManager] MissionDataが設定されていません。");
            if (briefingUI != null) briefingUI.SetActive(false);
            if (selectionUI != null) selectionUI.SetActive(true);
            return;
        }

        //========================
        // 📋 テキスト・画像の初期化
        //========================
        missionTitleText.text = mission.missionName;
        stageNameText.text = mission.stageName;
        rewardAmountText.text = $"{mission.rewardAmount:N0}";
        if (companyImage != null) companyImage.sprite = mission.companyImage;
        if (missionImage != null) missionImage.sprite = mission.missionImage;

        foreach (var text in objectiveTexts)
        {
            if (text != null) text.text = "";
        }

        //========================
        // 🎙️ メッセージとボイス設定
        //========================
        voices = mission.voices ?? new string[0];
        messages = mission.messages ?? new string[0];

        if (messages.Length == 0)
        {
            Debug.LogWarning("[BriefingManager] mission.messages が空です。");
            EndBriefing();
            return;
        }

        //========================
        // 💬 TypeWriter開始
        //========================
        if (messageTyper != null)
        {
            messageTyper.OnTypingFinished += OnMessageFinished;
            messageTyper.OnMessageChanged += PlayVoice;
            messageTyper.StartTyping(messages);
        }
        else
        {
            Debug.LogWarning("[BriefingManager] messageTyper が設定されていません。");
        }
    }

    /// <summary>
    /// ボイスを再生
    /// </summary>
    private void PlayVoice(int index)
    {
        if (voiceSource == null) return;
        if (voices == null || voices.Length == 0) return;

        // 範囲外対策
        if (index >= voices.Length)
        {
            Debug.LogWarning($"[BriefingManager] ボイス配列が不足しています: index={index}");
            return;
        }

        // 再生
        var clip = Resources.Load<AudioClip>(voices[index]);
        if (clip != null)
        {
            if (voiceSource.isPlaying)
                voiceSource.Stop();

            voiceSource.clip = clip;
            voiceSource.Play();
        }
        else
        {
            Debug.LogWarning($"[BriefingManager] ボイスが見つかりません: {voices[index]}");
        }
    }

    /// <summary>
    /// メッセージが全て終わったとき
    /// </summary>
    private void OnMessageFinished()
    {
        EndBriefing();
    }

    /// <summary>
    /// ブリーフィング終了処理
    /// </summary>
    private void EndBriefing()
    {
        Debug.Log("[BriefingManager] ブリーフィング終了。ミッション選択画面へ戻ります。");

        // 🎧 再生中のボイスを停止（スキップ時も含む）
        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Stop();
            voiceSource.clip = null; // 安全のためクリア
        }

        if (briefingUI != null) briefingUI.SetActive(false);
        if (selectionUI != null) selectionUI.SetActive(true);

        if (messageTyper != null)
        {
            messageTyper.OnTypingFinished -= OnMessageFinished;
            messageTyper.OnMessageChanged -= PlayVoice;
        }

        // 🔁 SceneHistoryManager対応（任意）
        // SceneHistoryManager.GoBack();
    }


}
