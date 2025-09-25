using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

    // 読み込んだ MissionData のリスト
    private List<MissionData> missions = new List<MissionData>();

    // 生成したボタンを保持してNavigation設定に使う
    private List<Button> generatedButtons = new List<Button>();

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

        generatedButtons.Clear();

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

            // Button コンポーネントを保持
            var button = go.GetComponent<Button>();
            if (button != null) generatedButtons.Add(button);
        }

        // 最初のミッションを選択表示
        SelectMission(missions[0]);

        // Navigation 設定（上下キー移動可能にする）
        SetupButtonNavigation();

        // 最初のボタンをフォーカス
        if (generatedButtons.Count > 0)
        {
            StartCoroutine(SetInitialSelection(generatedButtons[0].gameObject));
        }
    }

    System.Collections.IEnumerator SetInitialSelection(GameObject firstButton)
    {
        yield return null; // 1フレーム待ってから設定
        EventSystem.current.SetSelectedGameObject(firstButton);
    }

    void SetupButtonNavigation()
    {
        for (int i = 0; i < generatedButtons.Count; i++)
        {
            Navigation nav = generatedButtons[i].navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = generatedButtons[(i - 1 + generatedButtons.Count) % generatedButtons.Count];
            nav.selectOnDown = generatedButtons[(i + 1) % generatedButtons.Count];
            generatedButtons[i].navigation = nav;
        }
    }

    void Update()
    {
        // Enter または Space で現在選択中のボタンを押す
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                var button = selected.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                }
            }
        }
    }

    void LoadMissionsFromResources()
    {
        missions.Clear();

        TextAsset[] txtFiles = Resources.LoadAll<TextAsset>(resourcesFolder);
        if (txtFiles == null || txtFiles.Length == 0)
        {
            Debug.LogWarning($"MissionSelectManager: Resources/{resourcesFolder} に *.txt が見つかりません。");
            return;
        }

        foreach (var txt in txtFiles)
        {
            MissionData mission = ScriptableObject.CreateInstance<MissionData>();

            mission.objectives = new string[3];
            mission.objectiveAmounts = new int[3];
            mission.messages = new string[0];

            string[] lines = txt.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#")) continue;

                int colon = line.IndexOf(':');
                if (colon < 0) continue;
                string key = line.Substring(0, colon).Trim();
                string value = line.Substring(colon + 1).Trim();

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
                    case "objectiveAmounts":
                        string[] nums = value.Split('|');
                        mission.objectiveAmounts = new int[nums.Length];
                        for (int i = 0; i < nums.Length; i++) int.TryParse(nums[i], out mission.objectiveAmounts[i]);
                        break;
                    case "rewardAmount": int.TryParse(value, out mission.rewardAmount); break;
                    case "messages": mission.messages = value.Split('|'); break;
                    case "voices": mission.voices = value.Split('|'); break;
                    case "battlesceneName": mission.battlesceneName = value; break;
                }
            }

            if (string.IsNullOrEmpty(mission.missionName))
                mission.missionName = txt.name;

            missions.Add(mission);
        }
    }

    public void SelectMission(MissionData mission)
    {
        if (mission == null) return;

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

    public void OnStartMission()
    {
        if (GameData.currentSelected != null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Briefing");
        }
    }
}
