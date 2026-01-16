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

    [Header("ボイス")]
    public AudioSource voiceSource;
    [SerializeField] private GameObject voiceObject_; // ★AudioSource付きGO

    [Header("UIオブジェクト")]
    public GameObject briefingUI;
    public GameObject selectionUI;

    private string[] voices;
    private string[] messages;

    private int currentIndex = 0;
    private Coroutine waitVoiceCoroutine;

    private bool is_message_playing_ = false;
    private bool is_briefing_finished_ = false;
    private bool is_voice_disabled_ = false;

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

        EnableVoiceObject(true);
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
        if (!is_message_playing_ || is_briefing_finished_) return;

        StopVoiceAll();
        messageTyper.ForceComplete();
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
        if (is_briefing_finished_ || is_voice_disabled_) return;
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

        EnableVoiceObject(true);
        StopVoiceAll();

        voiceSource.clip = clip;
        voiceSource.Play();

        waitVoiceCoroutine = StartCoroutine(WaitForVoiceEnd());
    }

    private IEnumerator WaitForVoiceEnd()
    {
        while (voiceSource != null && voiceSource.isPlaying)
        {
            if (is_briefing_finished_ || is_voice_disabled_)
                yield break;

            yield return null;
        }

        if (is_briefing_finished_ || is_voice_disabled_)
            yield break;

        PlayNextMessage();
    }

    private void PlayNextMessage()
    {
        if (is_briefing_finished_) return;
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
        is_voice_disabled_ = true;

        StopVoiceAll();
        EnableVoiceObject(false); //ここで完全停止w

        if (messageTyper != null)
        {
            messageTyper.ForceComplete();
        }

        if (briefingUI != null) briefingUI.SetActive(false);
        if (selectionUI != null) selectionUI.SetActive(true);
    }

    //================================
    // 音声制御
    //================================

    private void StopVoiceAll()
    {
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
    }

    private void EnableVoiceObject(bool enable)
    {
        if (voiceObject_ == null)
        {
            Debug.LogError("❌ voiceObject_ が未設定です");
            return;
        }

        Debug.Log($"🔊 VoiceObject SetActive({enable}) : {voiceObject_.name}");
        voiceObject_.SetActive(enable);
    }

}
