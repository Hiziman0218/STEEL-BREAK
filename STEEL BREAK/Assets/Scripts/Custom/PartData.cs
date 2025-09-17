using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 機体パーツの情報を保持するScriptableObjectクラス。
/// 各パーツのステータスや種類、見た目の情報などをまとめて扱えるようにします。
/// </summary>
[CreateAssetMenu(fileName = "PartData", menuName = "Mech/PartData")]
public class PartData : ScriptableObject
{
    [Header("基本情報")]
    public string partName;              // パーツ名
    public PartType partType;           // パーツ種別

    [Header("見た目")]
    public Sprite partIcon;             // UI用アイコン

    [Tooltip("単一装着部位のプレハブ")]
    public GameObject partPrefab;       // 単一プレハブ（Headなどに使用）

    [Tooltip("複数装着が可能な部位用のプレハブ群")]
    public List<GameObject> multiPrefabs; // 複数プレハブ（L_Armなど）

    [Header("拡張するボーン情報")]
    public List<BoneScaleInfo> boneScales = new();

    [Tooltip("プレハブのスケール・位置補正情報を複数格納")]
    public List<PrefabScaleInfo> scaleInfos = new List<PrefabScaleInfo>();

    [Header("パーツのステータス")]
    public int AP;
    public int stability;
    public int weight;
    public int enLoad;

    [Header("説明")]
    [TextArea(2, 5)]
    public string description;
}

/// <summary>
/// パーツのプレハブと取り付け先ボーンの情報をまとめた構造体
/// </summary>
[System.Serializable]
public class PartPrefabInfo
{
    [Tooltip("このメッシュが取り付けられる対象のボーン名")]
    public string targetBoneName;

    [Tooltip("取り付けるプレハブ（このボーンにアタッチされる）")]
    public GameObject partPrefab;
}

[System.Serializable]
public class BoneScaleInfo
{
    [Tooltip("対象のボーン名")]
    public string boneName;

    [Tooltip("スケーリングする倍率 (x, y, z)")]
    public Vector3 scale = Vector3.one;

    [Tooltip("ローカル位置オフセット (x, y, z)")]
    public List<ChildBoneOffsetInfo> offsetInfos = new List<ChildBoneOffsetInfo>();
}

[System.Serializable]
public class PrefabScaleInfo
{
    [Tooltip("調整するプレハブ")]
    // 対象プレハブ
    public GameObject prefab;

    // スケーリング倍率
    public Vector3 scale = Vector3.one;

    // ローカル位置オフセット
    public Vector3 positionOffset = Vector3.zero;

    // ローカル回転オフセット
    public Vector3 rotationOffset = Vector3.zero;
}

[System.Serializable]
public class ChildBoneOffsetInfo
{
    public string childName;
    public Vector3 offset = Vector3.zero;
}