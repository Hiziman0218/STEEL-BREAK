using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MissionSelectManager : MonoBehaviour
{
    [Header("UI参照")]
    public Transform listContent;           // リストアイテムを生成する親（ScrollContent）
    public GameObject listItemPrefab;       // リストアイテムのプレハブ（MissionListItem をアタッチ）

    [Header("詳細表示")]
    public TextMeshProUGUI missionTitleText;
    public TextMeshProUGUI clientText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI descriptionText;
    public Image missionImageDisplay;

    [Header("Missionテキストを置く Resources フォルダ内のパス")]
    [Tooltip("Resources/Missions フォルダに *.txt を置いてください")]
    public string resourcesFolder = "Missions";

    // 読み込んだ MissionData のリスト（動的に ScriptableObject を作成）
    private List<MissionData> missions = new List<MissionData>();

    void Start()
    {
        if (listContent == null || listItemPrefab == null)
        {
            Debug.LogError("MissionSelectManager: listContent または listItemPrefab が未設定です。");
            return;
        }

        LoadMissionsFromResources();

        if (missions.Count == 0)
        {
            Debug.LogWarning("MissionSelectManager: Missions が読み込まれませんでした。Resources/Missions/*.txt を確認してください。");
            return;
        }

        // リスト生成
        foreach (var mission in missions)
        {
            var go = Instantiate(listItemPrefab, listContent);
            var item = go.GetComponent<MissionListItem>();
            if (item != null)
            {
                item.Setup(mission, this);
            }
            else
            {
                Debug.LogError("MissionSelectManager: listItemPrefab に MissionListItem スクリプトをアタッチしてください。");
            }
        }

        // 最初のミッションを選択表示
        SelectMission(missions[0]);
    }

    /// <summary>
    /// Resources/<resourcesFolder> にあるすべての TextAsset を読み込み、MissionData を生成する
    /// </summary>
    void LoadMissionsFromResources()
    {
        missions.Clear();

        // Resources.LoadAll<TextAsset>("Missions")
        TextAsset[] txtFiles = Resources.LoadAll<TextAsset>(resourcesFolder);
        if (txtFiles == null || txtFiles.Length == 0)
        {
            Debug.LogWarning($"MissionSelectManager: Resources/{resourcesFolder} に *.txt が見つかりません。");
            return;
        }

        foreach (var txt in txtFiles)
        {
            // 新しい MissionData をインスタンス化して値を埋める
            MissionData mission = ScriptableObject.CreateInstance<MissionData>();

            // 初期化（配列の長さを合わせる等）
            mission.objectives = new string[3];
            mission.objectiveAmounts = new int[3];
            mission.messages = new string[0];

            // 行ごとに処理
            string[] lines = txt.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#")) continue; // コメント行

                int colon = line.IndexOf(':');
                if (colon < 0) continue;
                string key = line.Substring(0, colon).Trim();
                string value = line.Substring(colon + 1).Trim();

                // key によって代入
                switch (key)
                {
                    case "missionID":
                        mission.missionID = value;
                        break;
                    case "missionName":
                        mission.missionName = value;
                        break;
                    case "client":
                        mission.client = value;
                        break;
                    case "reward":
                        mission.reward = value;
                        break;
                    case "description":
                        mission.description = value;
                        break;
                    case "sceneName":
                        mission.sceneName = value;
                        break;
                    case "missionImage":
                        // 画像は Resources 内のパス（例: Images/mountain_thumb）
                        var sp = Resources.Load<Sprite>(value);
                        mission.missionImage = sp;
                        break;
                    case "companyImage":
                        mission.companyImage = Resources.Load<Sprite>(value);
                        break;
                    case "stageName":
                        mission.stageName = value;
                        break;
                    case "objectives":
                        // 区切りは '|' 
                        mission.objectives = value.Split('|');
                        break;
                    case "objectiveAmounts":
                        {
                            string[] nums = value.Split('|');
                            mission.objectiveAmounts = new int[nums.Length];
                            for (int i = 0; i < nums.Length; i++)
                            {
                                int.TryParse(nums[i], out mission.objectiveAmounts[i]);
                            }
                        }
                        break;
                    case "rewardAmount":
                        int.TryParse(value, out mission.rewardAmount);
                        break;
                    case "messages":
                        mission.messages = value.Split('|');
                        break;
                    case "battlesceneName":
                        mission.battlesceneName = value;
                        break;
                    // 追加フィールドあればここに書く
                    default:
                        // 無視 or 将来ログ出力
                        break;
                }
            }

            // ミッション名がなければファイル名を代わりに設定
            if (string.IsNullOrEmpty(mission.missionName))
                mission.missionName = txt.name;

            missions.Add(mission);
        }
    }

    /// <summary>
    /// UI にミッション詳細を表示して、GameData.currentSelected に代入する
    /// </summary>
    public void SelectMission(MissionData mission)
    {
        if (mission == null) return;

        // 互換のため GameData.currentSelected に MissionData をセット（既存処理との互換を維持）
        GameData.currentSelected = mission;

        missionTitleText.text = mission.missionName ?? "";
        clientText.text = string.IsNullOrEmpty(mission.client) ? "" : $"依頼者: {mission.client}";
        rewardText.text = string.IsNullOrEmpty(mission.reward) ? "" : $"報酬: {mission.reward}";
        descriptionText.text = mission.description ?? "";

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

    /// <summary>
    /// ミッション開始ボタン（Briefing シーンへ遷移）
    /// </summary>
    public void OnStartMission()
    {
        if (GameData.currentSelected != null)
        {
            // Briefing シーンに遷移する場合、
            // Briefing シーン側で GameData.currentSelected を参照して処理してください
            UnityEngine.SceneManagement.SceneManager.LoadScene("Briefing");
        }
    }
}
