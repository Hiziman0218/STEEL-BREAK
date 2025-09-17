using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// �~�b�V�����ꗗ�̃��X�g�A�C�e���BMissionData ���󂯎��A�I�����ɐe�� MissionSelectManager �ɒʒm����B
/// �v���n�u�ɂ� Button �R���|�[�l���g���K�v�ł��iInspector �Őݒ�j�B
/// </summary>
[RequireComponent(typeof(Button))]
public class MissionListItem : MonoBehaviour
{
    [Header("�\��UI")]
    public TextMeshProUGUI nameText;
    public Image thumbImage; // (�C��) �T���l�C����\������ Image�i����ΐݒ�j

    private MissionData missionData;
    private MissionSelectManager selectManager;

    /// <summary>
    /// ���X�g�A�C�e�����Z�b�g�A�b�v����
    /// </summary>
    public void Setup(MissionData data, MissionSelectManager manager)
    {
        missionData = data;
        selectManager = manager;

        if (nameText != null)
            nameText.text = data.missionName ?? data.missionID ?? "(No name)";

        if (thumbImage != null)
        {
            if (data.missionImage != null)
            {
                thumbImage.sprite = data.missionImage;
                thumbImage.gameObject.SetActive(true);
            }
            else
            {
                thumbImage.gameObject.SetActive(false);
            }
        }

        // �{�^���̃N���b�N�C�x���g��ǉ�
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (selectManager != null && missionData != null)
        {
            selectManager.SelectMission(missionData);
        }
    }
}


//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//[RequireComponent(typeof(Button))]
//public class MissionListItem : MonoBehaviour
//{
//    public TextMeshProUGUI nameText;

//    private MissionData missionData;
//    private MissionSelectManager selectManager;

//    public void Setup(MissionData data, MissionSelectManager manager)
//    {
//        missionData = data;
//        selectManager = manager;

//        if (nameText != null)
//        {
//            nameText.text = data.missionName;
//        }

//        // �����I�ɂ��̃{�^���ɃN���b�N�C�x���g��ǉ�
//        var button = GetComponent<Button>();
//        if (button != null)
//        {
//            button.onClick.RemoveAllListeners();
//            button.onClick.AddListener(OnClick);
//        }
//    }

//    public void OnClick()
//    {
//        if (selectManager != null && missionData != null)
//        {
//            selectManager.SelectMission(missionData);
//        }
//    }
//}
