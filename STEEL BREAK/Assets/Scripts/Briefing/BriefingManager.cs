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

    private bool is_message_playing_ = false;
    private bool is_briefing_finished_ = false; // ★ 追加

    void Start()
    {
        var mission = GameData.currentSelected;
        if (mission == null)
        {
            EndBriefing();
            return;
        }

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

        PlayMessage(0);
    }

    void Update()
    {
        if (is_briefing_finished_) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            OnUserNext();
        }
    }

    private void OnUserNext()
    {
        if (!is_message_playing_) return;

        // ボイス停止
        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Stop();
        }

        messageTyper.ForceComplete();

        if (waitVoiceCoroutine != null)
        {
            StopCoroutine(waitVoiceCoroutine);
            waitVoiceCoroutine = null;
        }

        PlayNextMessage();
    }

    private void PlayMessage(int index)
    {
        if (is_briefing_finished_) return;

        if (index >= messages.Length)
        {
            EndBriefing();
            return;
        }

        is_message_playing_ = true;
        currentIndex = index;

        messageTyper.StartTyping(new string[] { messages[index] });
        PlayVoice(index);
    }

    private void PlayVoice(int index)
    {
        if (is_briefing_finished_) return;
        if (voiceSource == null) return;
        if (voices == null || index >= voices.Length)
        {
            PlayNextMessage();
            return;
        }

        var clip = Resources.Load<AudioClip>(voices[index]);
        if (clip == null)
        {
            PlayNextMessage();
            return;
        }

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.Play();

        if (waitVoiceCoroutine != null)
            StopCoroutine(waitVoiceCoroutine);

        waitVoiceCoroutine = StartCoroutine(WaitForVoiceEnd());
    }

    private IEnumerator WaitForVoiceEnd()
    {
        yield return new WaitWhile(() => voiceSource.isPlaying);

        if (is_briefing_finished_) yield break;

        PlayNextMessage();
    }

    private void PlayNextMessage()
    {
        currentIndex++;
        PlayMessage(currentIndex);
    }

    public void SkipBriefing()
    {
        EndBriefing();
    }

    private void EndBriefing()
    {
        if (is_briefing_finished_) return;

        is_briefing_finished_ = true;
        is_message_playing_ = false;

        if (waitVoiceCoroutine != null)
        {
            StopCoroutine(waitVoiceCoroutine);
            waitVoiceCoroutine = null;
        }

        if (voiceSource != null)
        {
            voiceSource.Stop();
            voiceSource.clip = null;
        }

        if (messageTyper != null)
        {
            messageTyper.ForceComplete();
        }

        if (briefingUI != null) briefingUI.SetActive(false);
        if (selectionUI != null) selectionUI.SetActive(true);
    }
}
