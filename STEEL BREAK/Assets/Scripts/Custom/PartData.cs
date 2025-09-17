using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �@�̃p�[�c�̏���ێ�����ScriptableObject�N���X�B
/// �e�p�[�c�̃X�e�[�^�X���ށA�����ڂ̏��Ȃǂ��܂Ƃ߂Ĉ�����悤�ɂ��܂��B
/// </summary>
[CreateAssetMenu(fileName = "PartData", menuName = "Mech/PartData")]
public class PartData : ScriptableObject
{
    [Header("��{���")]
    public string partName;              // �p�[�c��
    public PartType partType;           // �p�[�c���

    [Header("������")]
    public Sprite partIcon;             // UI�p�A�C�R��

    [Tooltip("�P�ꑕ�����ʂ̃v���n�u")]
    public GameObject partPrefab;       // �P��v���n�u�iHead�ȂǂɎg�p�j

    [Tooltip("�����������\�ȕ��ʗp�̃v���n�u�Q")]
    public List<GameObject> multiPrefabs; // �����v���n�u�iL_Arm�Ȃǁj

    [Header("�g������{�[�����")]
    public List<BoneScaleInfo> boneScales = new();

    [Tooltip("�v���n�u�̃X�P�[���E�ʒu�␳���𕡐��i�[")]
    public List<PrefabScaleInfo> scaleInfos = new List<PrefabScaleInfo>();

    [Header("�p�[�c�̃X�e�[�^�X")]
    public int AP;
    public int stability;
    public int weight;
    public int enLoad;

    [Header("����")]
    [TextArea(2, 5)]
    public string description;
}

/// <summary>
/// �p�[�c�̃v���n�u�Ǝ��t����{�[���̏����܂Ƃ߂��\����
/// </summary>
[System.Serializable]
public class PartPrefabInfo
{
    [Tooltip("���̃��b�V�������t������Ώۂ̃{�[����")]
    public string targetBoneName;

    [Tooltip("���t����v���n�u�i���̃{�[���ɃA�^�b�`�����j")]
    public GameObject partPrefab;
}

[System.Serializable]
public class BoneScaleInfo
{
    [Tooltip("�Ώۂ̃{�[����")]
    public string boneName;

    [Tooltip("�X�P�[�����O����{�� (x, y, z)")]
    public Vector3 scale = Vector3.one;

    [Tooltip("���[�J���ʒu�I�t�Z�b�g (x, y, z)")]
    public List<ChildBoneOffsetInfo> offsetInfos = new List<ChildBoneOffsetInfo>();
}

[System.Serializable]
public class PrefabScaleInfo
{
    [Tooltip("��������v���n�u")]
    // �Ώۃv���n�u
    public GameObject prefab;

    // �X�P�[�����O�{��
    public Vector3 scale = Vector3.one;

    // ���[�J���ʒu�I�t�Z�b�g
    public Vector3 positionOffset = Vector3.zero;

    // ���[�J����]�I�t�Z�b�g
    public Vector3 rotationOffset = Vector3.zero;
}

[System.Serializable]
public class ChildBoneOffsetInfo
{
    public string childName;
    public Vector3 offset = Vector3.zero;
}