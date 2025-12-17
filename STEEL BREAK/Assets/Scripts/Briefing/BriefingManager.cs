using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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

    private int currentIndex = 0;
    private Coroutine waitVoiceCoroutine;

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
        // 📋 テキスト・画像初期化
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

        voices = mission.voices ?? new string[0];
        messages = mission.messages ?? new string[0];

        if (messages.Length == 0)
        {
            EndBriefing();
            return;
        }

        //========================
        // 💬 最初のメッセージ開始
        //========================
        PlayMessage(0);
    }

    /// <summary>
    /// 指定 index のメッセージ＋ボイスを再生
    /// </summary>
    private void PlayMessage(int index)
    {
        if (index >= messages.Length)
        {
            EndBriefing();
            return;
        }

        currentIndex = index;

        // テキスト開始（1文ずつ）
        messageTyper.StartTyping(new string[] { messages[index] });

        // ボイス再生
        PlayVoice(index);
    }

    /// <summary>
    /// ボイス再生
    /// </summary>
    private void PlayVoice(int index)
    {
        if (voiceSource == null) return;
        if (voices == null || index >= voices.Length) return;

        var clip = Resources.Load<AudioClip>(voices[index]);
        if (clip == null)
        {
            Debug.LogWarning($"[BriefingManager] ボイスが見つかりません: {voices[index]}");
            PlayNextMessage();
            return;
        }

        if (voiceSource.isPlaying)
            voiceSource.Stop();

        voiceSource.clip = clip;
        voiceSource.Play();

        // 既存の待機コルーチン停止
        if (waitVoiceCoroutine != null)
            StopCoroutine(waitVoiceCoroutine);

        waitVoiceCoroutine = StartCoroutine(WaitForVoiceEnd());
    }

    /// <summary>
    /// ボイス終了待ち
    /// </summary>
    private IEnumerator WaitForVoiceEnd()
    {
        yield return new WaitWhile(() => voiceSource.isPlaying);
        PlayNextMessage();
    }

    /// <summary>
    /// 次のメッセージへ
    /// </summary>
    private void PlayNextMessage()
    {
        currentIndex++;
        PlayMessage(currentIndex);
    }

    /// <summary>
    /// ブリーフィング終了処理
    /// </summary>
    private void EndBriefing()
    {
        Debug.Log("[BriefingManager] ブリーフィング終了");

        if (waitVoiceCoroutine != null)
        {
            StopCoroutine(waitVoiceCoroutine);
            waitVoiceCoroutine = null;
        }

        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Stop();
            voiceSource.clip = null;
        }

        if (briefingUI != null) briefingUI.SetActive(false);
        if (selectionUI != null) selectionUI.SetActive(true);
    }
}
