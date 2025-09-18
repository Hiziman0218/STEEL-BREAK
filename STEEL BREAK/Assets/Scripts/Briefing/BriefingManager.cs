using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BriefingManager : MonoBehaviour
{
    [Header("基本情報UI")]
    public TextMeshProUGUI missionTitleText;  // ミッション名表示
    public TextMeshProUGUI clientText;        // 発注企業名表示
    public TextMeshProUGUI stageNameText;     // ステージ名表示
    public TextMeshProUGUI rewardAmountText;  // 報酬金額表示

    [Header("画像UI")]
    public Image companyImage;   // 企業ロゴ表示用
    public Image missionImage;   // ステージ画像表示用

    [Header("目標UI")]
    public TextMeshProUGUI[] objectiveTexts;  // ミッション目標（複数）

    [Header("メッセージ表示")]
    public TypeWriterEffect messageTyper;     // メッセージを一文字ずつ表示するスクリプト
    public AudioSource voiceSource;           // ボイス再生用のAudioSource（インスペクタで設定）

    [Header("UIオブジェクト")]
    public GameObject briefingUI;  // ブリーフィング画面の親UI
    public GameObject selectionUI; // ミッション選択画面の親UI

    private string[] voices;  // ボイスのパスを格納する配列（MissionDataから取得）

    void Start()
    {
        // 選択されたミッションデータを取得
        var mission = GameData.currentSelected;
        if (mission == null)
        {
            Debug.LogError("MissionDataが設定されていません");
            return;
        }

        // 基本テキストのセット
        missionTitleText.text = mission.missionName;        // ミッション名
        stageNameText.text = mission.stageName;             // ステージ名
        rewardAmountText.text = $"{mission.rewardAmount:N0}"; // 報酬を3桁区切りで表示

        // 画像のセット（nullチェックあり）
        if (companyImage != null) companyImage.sprite = mission.companyImage;
        if (missionImage != null) missionImage.sprite = mission.missionImage;

        // 目標テキスト初期化（必要なら後でセット）
        for (int i = 0; i < objectiveTexts.Length; i++)
        {
            objectiveTexts[i].text = "";
        }

        // MissionDataからvoices配列を取得
        voices = mission.voices;

        // メッセージの表示開始
        if (messageTyper != null)
        {
            // メッセージが全て終わったときに呼ばれる
            messageTyper.OnTypingFinished += OnMessageFinished;
            // メッセージが切り替わったタイミングでボイスを再生
            messageTyper.OnMessageChanged += PlayVoice;
            // タイピング開始
            messageTyper.StartTyping(mission.messages);
        }
    }

    /// <summary>
    /// メッセージ切り替え時に対応するボイスを再生
    /// </summary>
    private void PlayVoice(int index)
    {
        if (voices != null && index < voices.Length)
        {
            // ResourcesフォルダからAudioClipをロード
            var clip = Resources.Load<AudioClip>(voices[index]);
            if (clip != null && voiceSource != null)
            {
                voiceSource.clip = clip;
                voiceSource.Play();  // 再生開始
            }
            else
            {
                Debug.LogWarning($"ボイスクリップが見つからない: {voices[index]}");
            }
        }
    }

    /// <summary>
    /// メッセージが全て終わった時に呼ばれる
    /// </summary>
    private void OnMessageFinished()
    {
        // ブリーフィングUIを閉じて、ミッション選択画面を表示
        if (briefingUI != null) briefingUI.SetActive(false);
        if (selectionUI != null) selectionUI.SetActive(true);

        // イベント登録を解除（メモリリーク防止）
        if (messageTyper != null)
        {
            messageTyper.OnTypingFinished -= OnMessageFinished;
            messageTyper.OnMessageChanged -= PlayVoice;
        }
    }
}
