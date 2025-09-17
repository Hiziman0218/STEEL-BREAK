using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// �p�[�c���X�g�̊e���ڂ��Ǘ�����N���X�iUI�A�C�e���j
/// </summary>
public class PartListItem : MonoBehaviour
{
    // �p�[�c�̃A�C�R���摜��\������UI
    [SerializeField] private Image icon;

    // �p�[�c����\������TextMeshPro UI
    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private Button selectButton;

    // ���̃��X�g�A�C�e�����Q�Ƃ���p�[�c�f�[�^
    private PartData partData;

    // �p�[�c�I��UI�𐧌䂷��}�l�[�W���[�ւ̎Q��
    private AssemblyUIManager uiManager;

    /// <summary>
    /// �p�[�c�f�[�^��UI�}�l�[�W���[��ݒ肵�A�\����������
    /// </summary>
    /// <param name="data">�\���Ώۂ̃p�[�c�f�[�^</param>
    /// <param name="manager">UI�}�l�[�W���[</param>
    public void Setup(PartData data, AssemblyUIManager manager)
    {
        partData = data;
        uiManager = manager;

        // UI�ɃA�C�R���Ɩ��O��ݒ�
        icon.sprite = partData.partIcon;
        nameText.text = partData.partName;

        // Button�R���|�[�l���g��OnClick��o�^
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelect);
    }

    /// <summary>
    /// �v���C���[�����̃p�[�c��I�������Ƃ��ɌĂ΂��
    /// </summary>
    public void OnSelect()
    {
        // UI�}�l�[�W���[�ɑI�����ꂽ�p�[�c��ʒm
        uiManager.OnPartSelected(partData);
    }
}
