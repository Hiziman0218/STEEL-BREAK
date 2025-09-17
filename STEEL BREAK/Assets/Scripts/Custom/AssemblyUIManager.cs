using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// �p�[�c�I�� UI �S�̂𐧌䂷��}�l�[�W���[�B
/// �^�u�ɉ������p�[�c���X�g��\�����A�I�����ꂽ�p�[�c�� Mech �ɑ�������B
/// </summary>
public class AssemblyUIManager : MonoBehaviour
{
    [Header("�Q��")]

    // �p�[�c���X�g��\������e�R���e�i�i�X�N���[���r���[��Content�Ȃǁj
    public Transform partListParent;

    // �p�[�c���X�g�̃A�C�e���v���n�u�iPartListItem�X�N���v�g���A�^�b�`����Ă���j
    public GameObject listItemPrefab;

    // Mech�Ƀp�[�c�𑕒�����}�l�[�W���[�i���̑��̃��f������j
    public MechAssemblyManager mechAssemblyManager;

    [Header("�p�[�c�f�[�^")]

    // �S�Ẵp�[�c�̃f�[�^�iScriptableObject�Ȃǂœo�^����Ă���j
    public List<PartData> allParts;

    // ���ݑI�𒆂̃^�u�i���ʎ�ʁj
    private PartType currentTabType;

    [SerializeField] private MechSaveLoader saveLoader;


    /// <summary>
    /// ���݂̃^�u�ɑΉ�����p�[�c���X�g��\���������B
    /// </summary>
    public void RefreshPartList()
    {
        // �@ ���ݕ\������Ă��郊�X�g�A�C�e�������ׂč폜
        foreach (Transform child in partListParent)
        {
            Destroy(child.gameObject);
        }

        PartData[] filteredParts;

        // �A ���Ꮘ���FWeaponL �̏ꍇ�AWeapon �^�C�v�̃p�[�c���ꏏ�ɕ\������
        if (currentTabType == PartType.WeaponL)
        {
            filteredParts = allParts
                .Where(p => p.partType == PartType.WeaponL || p.partType == PartType.Weapon)
                .ToArray();
        }
        else
        {
            // �B �ʏ�F�I�����ꂽ�^�u�ƈ�v����^�C�v�̃p�[�c�����𒊏o
            filteredParts = allParts
                .Where(p => p.partType == currentTabType)
                .ToArray();
        }

        // �C ���o�����p�[�c�����ɁAUI���X�g�A�C�e���𐶐����ĕ��ׂ�
        foreach (var part in filteredParts)
        {
            GameObject item = Instantiate(listItemPrefab, partListParent); // �v���n�u�𐶐����Đe�ɃZ�b�g
            item.GetComponent<PartListItem>().Setup(part, this);           // �e�A�C�e���Ƀp�[�c����ݒ�
        }
    }

    /// <summary>
    /// �^�u���I�����ꂽ�Ƃ��ɌĂ΂��BPartType�ɉ����ă��X�g���X�V�B
    /// </summary>
    /// <param name="no">PartType��int�l�iEnum�̔ԍ��j</param>
    public void OnTabSelected(int no)
    {
        // �@ �n���ꂽ�ԍ��� PartType �ɕϊ��\�����`�F�b�N
        if (System.Enum.IsDefined(typeof(PartType), no))
        {
            // �A ����Ȕԍ��Ȃ� PartType �ɕϊ����ĕۑ�
            currentTabType = (PartType)no;

            // �B ���X�g���č\�z
            RefreshPartList();
        }
        else
        {
            // �ُ�l�̂Ƃ��͌x�����o��
            Debug.LogWarning($"������ PartType �̔ԍ�: {no}");
        }
    }

    /// <summary>
    /// �p�[�c�����X�g����I�����ꂽ�Ƃ��ɌĂ΂��BMech �ɑ����������˗��B
    /// </summary>
    /// <param name="part">�I�����ꂽ PartData</param>
    public void OnPartSelected(PartData part)
    {
        // Mech�Ƀp�[�c�𑕒�
        mechAssemblyManager.AttachPart(part, currentTabType);

        // �K�v�ł���΃X�e�[�^�XUI�X�V�Ȃǂ̒ǉ������������Ŏ��s�\
    }

    // WeaponL �{�^���������ꂽ�Ƃ�
    public void OnWeaponLButtonClicked(PartData selectedWeapon)
    {
        mechAssemblyManager.AttachPart(selectedWeapon, currentTabType);
    }

    public void OnClickSave()
    {
        GameData.mechSaveData.Save();
    }

}