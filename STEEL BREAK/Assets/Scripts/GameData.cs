using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameData
{
    public static MissionData currentSelected;
    public static MechSaveData mechSaveData = new MechSaveData();

    // Resources/Prefabs �ȉ��ɒu���� ResultMenu Prefab �̃p�X
    private const string ResultMenuPath = "Result/Result";

    /// <summary>
    /// �Q�[���N���A��ʂ𓮓I�������ĕ\��
    /// </summary>
    public static void ShowGameClear()
    {
        ShowResult(EndType.GameClear);
    }

    /// <summary>
    /// �Q�[���I�[�o�[��ʂ𓮓I�������ĕ\��
    /// </summary>
    public static void ShowGameOver()
    {
        ShowResult(EndType.GameOver);
    }

    // Result.cs ���� enum
    public enum EndType { GameClear, GameOver }

    /// <summary>
    /// ���ʐ�������
    /// </summary>
    private static void ShowResult(EndType type)
    {
        // ���ɕ\���ς݂Ȃ琶�����Ȃ�
        if (Object.FindObjectOfType<Result>() != null)
            return;

        // Prefab �����[�h
        var prefab = Resources.Load<GameObject>(ResultMenuPath);
        if (prefab == null)
        {
            Debug.LogError($"ResultMenu Prefab �� Resources/{ResultMenuPath}.prefab �Ɍ�����܂���");
            return;
        }

        // Canvas �̎q�Ƃ��Đ����������ꍇ�͑�2������ parent ��n��
        var instance = Object.Instantiate(prefab);
        var result = instance.GetComponent<Result>();
        if (result == null)
        {
            Debug.LogError("�������� Prefab �� Result �R���|�[�l���g���A�^�b�`����Ă��܂���");
            return;
        }

        // �N���A�^�I�[�o�[ ���[�h���Z�b�g
        result.endType = (type == EndType.GameClear)
            ? Result.EndType.GameClear
            : Result.EndType.GameOver;

        // �Q�[����~
        Time.timeScale = 0f;
    }
}

/// <summary>
/// �X���b�g���Ƃ̃Z�[�u�f�[�^�\��
/// </summary>
[System.Serializable]
public class SlotSaveData
{
    public string slotName;      // �X���b�g�̖��O�iPartType�񋓌^�̕�����j
    public string partsDataName;    // �p�[�c�f�[�^�̖��O�iResources/PartsData �ɂ���j
}

/// <summary>
/// �@�̑S�̂̃Z�[�u�f�[�^�\��
/// </summary>
[System.Serializable]
public class MechSaveData
{
    public List<SlotSaveData> slots = new();  // �e�X���b�g�̃p�[�c��񃊃X�g
    [SerializeField] private string saveFileName = "mech_save.json";  // �Z�[�u�t�@�C����

    /// <summary>
    /// ���݂̑�������ۑ�����
    /// </summary>
    public void Save()
    {
        MechAssemblyManager assemblyManager = MechAssemblyManager.instance;
        if (assemblyManager == null) return;

        MechSaveData data = new MechSaveData();  // �ۑ��p�f�[�^���쐬

        // �������̑S�p�[�c���擾
        foreach (var kvp in assemblyManager.GetEquippedParts())
        {
            foreach (var part in kvp.Value)
            {
                if (part == null) continue;

                // �X���b�g���� PartType �Ƃ��ĕۑ�
                string slotName = kvp.Key.ToString();

                // �X���b�g����ǉ�
                data.slots.Add(new SlotSaveData
                {
                    slotName = slotName,
                    partsDataName = part.partsDataName,
                });
            }
        }

        // JSON�`���ɕϊ����ĕۑ�
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, saveFileName), json);

        Debug.Log("���J�\����ۑ����܂����B");
    }

    /// <summary>
    /// �ۑ����ꂽ��������ǂݍ���
    /// </summary>
    public void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);

        // �ۑ��t�@�C�������݂��Ȃ��ꍇ
        if (!File.Exists(path))
        {
            Debug.LogWarning("�ۑ��t�@�C����������܂���");
            return;
        }

        // �t�@�C����ǂݍ���Ńf�V���A���C�Y
        string json = File.ReadAllText(path);
        MechSaveData data = JsonUtility.FromJson<MechSaveData>(json);
        slots = data.slots;

        //foreach (var slot in data.slots)
        //{
        //    PartType type;

        //    // �X���b�g���� PartType �ɕϊ�
        //    if (!System.Enum.TryParse(slot.slotName, out type)) continue;

        //    // Resources/Parts ����v���n�u��ǂݍ���
        //    GameObject prefab = Resources.Load<GameObject>($"Parts/{slot.prefabName}");
        //    if (prefab == null)
        //    {
        //        Debug.LogWarning($"�v���n�u {slot.prefabName} ���ǂݍ��߂܂���ł���");
        //        continue;
        //    }

        //    // �_�~�[�� PartData ���쐬���đ���
        //    PartData dummyPart = new PartData
        //    {
        //        partType = type,
        //        partPrefab = prefab
        //    };

        //    assemblyManager.AttachPart(dummyPart, type);
        //}

        Debug.Log("���J�\����ǂݍ��݂܂����B");
    }

    public static MissionData currentSelected;
    public static MechSaveData mechSaveData = new MechSaveData();
}