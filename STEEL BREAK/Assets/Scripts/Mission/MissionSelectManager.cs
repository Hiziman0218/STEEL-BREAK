using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MissionSelectManager : MonoBehaviour
{
    [Header("UI�Q��")]
    public Transform listContent;           // ���X�g�A�C�e���𐶐�����e�iScrollContent�j
    public GameObject listItemPrefab;       // ���X�g�A�C�e���̃v���n�u�iMissionListItem ���A�^�b�`�j

    [Header("�ڍו\��")]
    public TextMeshProUGUI missionTitleText;
    public TextMeshProUGUI clientText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI descriptionText;
    public Image missionImageDisplay;

    [Header("Mission�e�L�X�g��u�� Resources �t�H���_���̃p�X")]
    [Tooltip("Resources/Missions �t�H���_�� *.txt ��u���Ă�������")]
    public string resourcesFolder = "Missions";

    // �ǂݍ��� MissionData �̃��X�g�i���I�� ScriptableObject ���쐬�j
    private List<MissionData> missions = new List<MissionData>();

    void Start()
    {
        if (listContent == null || listItemPrefab == null)
        {
            Debug.LogError("MissionSelectManager: listContent �܂��� listItemPrefab �����ݒ�ł��B");
            return;
        }

        LoadMissionsFromResources();

        if (missions.Count == 0)
        {
            Debug.LogWarning("MissionSelectManager: Missions ���ǂݍ��܂�܂���ł����BResources/Missions/*.txt ���m�F���Ă��������B");
            return;
        }

        // ���X�g����
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
                Debug.LogError("MissionSelectManager: listItemPrefab �� MissionListItem �X�N���v�g���A�^�b�`���Ă��������B");
            }
        }

        // �ŏ��̃~�b�V������I��\��
        SelectMission(missions[0]);
    }

    /// <summary>
    /// Resources/<resourcesFolder> �ɂ��邷�ׂĂ� TextAsset ��ǂݍ��݁AMissionData �𐶐�����
    /// </summary>
    void LoadMissionsFromResources()
    {
        missions.Clear();

        // Resources.LoadAll<TextAsset>("Missions")
        TextAsset[] txtFiles = Resources.LoadAll<TextAsset>(resourcesFolder);
        if (txtFiles == null || txtFiles.Length == 0)
        {
            Debug.LogWarning($"MissionSelectManager: Resources/{resourcesFolder} �� *.txt ��������܂���B");
            return;
        }

        foreach (var txt in txtFiles)
        {
            // �V���� MissionData ���C���X�^���X�����Ēl�𖄂߂�
            MissionData mission = ScriptableObject.CreateInstance<MissionData>();

            // �������i�z��̒��������킹�铙�j
            mission.objectives = new string[3];
            mission.objectiveAmounts = new int[3];
            mission.messages = new string[0];

            // �s���Ƃɏ���
            string[] lines = txt.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#")) continue; // �R�����g�s

                int colon = line.IndexOf(':');
                if (colon < 0) continue;
                string key = line.Substring(0, colon).Trim();
                string value = line.Substring(colon + 1).Trim();

                // key �ɂ���đ��
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
                        // �摜�� Resources ���̃p�X�i��: Images/mountain_thumb�j
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
                        // ��؂�� '|' 
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
                    // �ǉ��t�B�[���h����΂����ɏ���
                    default:
                        // ���� or �������O�o��
                        break;
                }
            }

            // �~�b�V���������Ȃ���΃t�@�C���������ɐݒ�
            if (string.IsNullOrEmpty(mission.missionName))
                mission.missionName = txt.name;

            missions.Add(mission);
        }
    }

    /// <summary>
    /// UI �Ƀ~�b�V�����ڍׂ�\�����āAGameData.currentSelected �ɑ������
    /// </summary>
    public void SelectMission(MissionData mission)
    {
        if (mission == null) return;

        // �݊��̂��� GameData.currentSelected �� MissionData ���Z�b�g�i���������Ƃ̌݊����ێ��j
        GameData.currentSelected = mission;

        missionTitleText.text = mission.missionName ?? "";
        clientText.text = string.IsNullOrEmpty(mission.client) ? "" : $"�˗���: {mission.client}";
        rewardText.text = string.IsNullOrEmpty(mission.reward) ? "" : $"��V: {mission.reward}";
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
    /// �~�b�V�����J�n�{�^���iBriefing �V�[���֑J�ځj
    /// </summary>
    public void OnStartMission()
    {
        if (GameData.currentSelected != null)
        {
            // Briefing �V�[���ɑJ�ڂ���ꍇ�A
            // Briefing �V�[������ GameData.currentSelected ���Q�Ƃ��ď������Ă�������
            UnityEngine.SceneManagement.SceneManager.LoadScene("Briefing");
        }
    }
}
