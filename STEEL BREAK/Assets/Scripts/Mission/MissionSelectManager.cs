using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ミッション選択画面全体を制御するクラス。
/// Resources/Missions フォルダにある .txt ファイルを読み込み、
/// UIリストに動的にミッションを並べて、詳細表示と選択を行う。
/// </summary>
public class MissionSelectManager : MonoBehaviour
{
    //=====================================
    // ▼ インスペクター設定用フィールド
    //=====================================

    [Header("UI参照")]
    public Transform listContent;           // ミッションリストを配置する親（ScrollView の Content）
    public GameObject listItemPrefab;       // 各リスト項目のプレハブ（MissionListItem スクリプト付き）

    [Header("詳細表示")]
    public TextMeshProUGUI missionTitleText; // ミッション名表示テキスト
    public TextMeshProUGUI clientText;       // 依頼者名表示テキスト
    public TextMeshProUGUI rewardText;       // 報酬表示テキスト
    public TextMeshProUGUI descriptionText;  // ミッション説明文表示テキスト
    public Image missionImageDisplay;        // ミッション画像（サムネイルなど）

    [Header("Missionテキストを置く Resources フォルダ内のパス")]
    [Tooltip("Resources/Missions フォルダに *.txt を置いてください")]
    public string resourcesFolder = "Missions";

    //=====================================
    // ▼ 内部変数
    //=====================================

    // 読み込んだミッションデータのリスト
    private List<MissionData> missions = new List<MissionData>();

    // 生成されたボタンを記録（上下キー移動ナビゲーションに使う）
    private List<Button> generatedButtons = new List<Button>();


    //=====================================
    // ▼ 初期化処理
    //=====================================
    void Start()
    {
        // UI設定の抜け確認
        if (listContent == null || listItemPrefab == null)
        {
            Debug.LogError("MissionSelectManager: listContent または listItemPrefab が未設定です。");
            return;
        }

        // ミッションデータを Resources から読み込む
        LoadMissionsFromResources();

        // 読み込み失敗時のチェック
        if (missions.Count == 0)
        {
            Debug.LogWarning("MissionSelectManager: Missions が読み込まれませんでした。Resources/Missions/*.txt を確認してください。");
            return;
        }

        // 古いボタンリストをクリア
        generatedButtons.Clear();

        //==========================
        // ミッションリスト生成処理
        //==========================
        foreach (var mission in missions)
        {
            // プレハブを生成してリストに追加
            var go = Instantiate(listItemPrefab, listContent);

            // MissionListItem スクリプトを取得してデータを設定
            var item = go.GetComponent<MissionListItem>();
            if (item != null)
            {
                // このManagerの参照も渡す（クリック時にSelectMissionを呼ぶため）
                item.Setup(mission, this);
            }
            else
            {
                Debug.LogError("MissionSelectManager: listItemPrefab に MissionListItem スクリプトをアタッチしてください。");
            }

            // Button コンポーネントを取得し、リストに保存
            var button = go.GetComponent<Button>();
            if (button != null)
                generatedButtons.Add(button);
        }

        //==========================
        // 最初のミッションを詳細表示
        //==========================
        SelectMission(missions[0]);

        //==========================
        // ボタンの上下ナビゲーション設定
        //==========================
        SetupButtonNavigation();

        //==========================
        // 最初のボタンをフォーカス状態に設定
        //==========================
        if (generatedButtons.Count > 0)
        {
            // コルーチンで1フレーム待って設定する（EventSystemの初期化待ち）
            StartCoroutine(SetInitialSelection(generatedButtons[0].gameObject));
        }
    }

    /// <summary>
    /// 最初に選択するボタンを EventSystem に登録する
    /// </summary>
    System.Collections.IEnumerator SetInitialSelection(GameObject firstButton)
    {
        yield return null; // 1フレーム待機してから実行
        EventSystem.current.SetSelectedGameObject(firstButton);
    }

    /// <summary>
    /// 上下キーで移動できるよう Navigation を設定
    /// </summary>
    void SetupButtonNavigation()
    {
        for (int i = 0; i < generatedButtons.Count; i++)
        {
            Navigation nav = generatedButtons[i].navigation;
            nav.mode = Navigation.Mode.Explicit; // 明示的に上下を設定

            // 上キー押したときの遷移先（循環式）
            nav.selectOnUp = generatedButtons[(i - 1 + generatedButtons.Count) % generatedButtons.Count];

            // 下キー押したときの遷移先（循環式）
            nav.selectOnDown = generatedButtons[(i + 1) % generatedButtons.Count];

            generatedButtons[i].navigation = nav;
        }
    }


    //=====================================
    // ▼ 毎フレーム処理
    //=====================================
    void Update()
    {
        // Enter または Space キーで現在選択中のボタンを押す処理
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                var button = selected.GetComponent<Button>();
                if (button != null)
                {
                    // onClick イベントを呼び出す（MissionListItem → SelectMission）
                    button.onClick.Invoke();
                }
            }
        }
    }


    //=====================================
    // ▼ ミッション読み込み処理
    //=====================================
    void LoadMissionsFromResources()
    {
        missions.Clear();

        // Resources/Missions フォルダ内の全テキストファイルを読み込む
        TextAsset[] txtFiles = Resources.LoadAll<TextAsset>(resourcesFolder);

        if (txtFiles == null || txtFiles.Length == 0)
        {
            Debug.LogWarning($"MissionSelectManager: Resources/{resourcesFolder} に *.txt が見つかりません。");
            return;
        }

        // 各テキストを MissionData に変換
        foreach (var txt in txtFiles)
        {
            MissionData mission = ScriptableObject.CreateInstance<MissionData>();

            // 初期化
            mission.objectives = new string[3];
            mission.objectiveAmounts = new int[3];
            mission.messages = new string[0];

            // テキストを1行ずつ処理
            string[] lines = txt.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                // 空行やコメント行(#で始まる)をスキップ
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#")) continue;

                // 「キー:値」形式で分割
                int colon = line.IndexOf(':');
                if (colon < 0) continue;

                string key = line.Substring(0, colon).Trim();
                string value = line.Substring(colon + 1).Trim();

                // 各キーに応じた処理
                switch (key)
                {
                    case "missionID": mission.missionID = value; break;
                    case "missionName": mission.missionName = value; break;
                    case "client": mission.client = value; break;
                    case "reward": mission.reward = value; break;
                    case "description": mission.description = value; break;
                    case "sceneName": mission.sceneName = value; break;
                    case "missionImage": mission.missionImage = Resources.Load<Sprite>(value); break;
                    case "companyImage": mission.companyImage = Resources.Load<Sprite>(value); break;
                    case "stageName": mission.stageName = value; break;
                    case "objectives": mission.objectives = value.Split('|'); break;

                    // 目的達成数を | 区切りで読み込む
                    case "objectiveAmounts":
                        string[] nums = value.Split('|');
                        mission.objectiveAmounts = new int[nums.Length];
                        for (int i = 0; i < nums.Length; i++)
                            int.TryParse(nums[i], out mission.objectiveAmounts[i]);
                        break;

                    // 報酬額
                    case "rewardAmount":
                        int.TryParse(value, out mission.rewardAmount);
                        break;

                    // メッセージ・ボイス（ブリーフィング用）
                    case "messages": mission.messages = value.Split('|'); break;
                    case "voices": mission.voices = value.Split('|'); break;

                    // 戦闘シーン名
                    case "battlesceneName": mission.battlesceneName = value; break;
                }
            }

            // missionName が空の場合はファイル名を代用
            if (string.IsNullOrEmpty(mission.missionName))
                mission.missionName = txt.name;

            // リストに追加
            missions.Add(mission);
        }
    }


    //=====================================
    // ▼ ミッションを選択したときの処理
    //=====================================
    public void SelectMission(MissionData mission)
    {
        if (mission == null) return;

        // 現在選択中のミッションを保存
        GameData.currentSelected = mission;

        // UI更新
        missionTitleText.text = mission.missionName ?? "";
        clientText.text = string.IsNullOrEmpty(mission.client) ? "" : $"依頼者: {mission.client}";
        rewardText.text = string.IsNullOrEmpty(mission.reward) ? "" : $"報酬: {mission.reward}";
        descriptionText.text = mission.description ?? "";

        // ミッション画像の有無で表示を切り替え
        if (mission.missionImage != null)
        {
            missionImageDisplay.sprite = mission.missionImage;
            missionImageDisplay.gameObject.SetActive(true);
        }
        else
        {
            missionImageDisplay.gameObject.SetActive(false);
        }
    }


    //=====================================
    // ▼ 「開始」ボタンを押したときの処理
    //=====================================
    public void OnStartMission()
    {
        // 選択中のミッションがある場合のみ実行
        if (GameData.currentSelected != null)
        {
            // Briefing シーンに遷移（ブリーフィングへ）
            UnityEngine.SceneManagement.SceneManager.LoadScene("Briefing");
        }
    }
}
