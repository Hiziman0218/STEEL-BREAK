using UnityEngine;
using System.Collections.Generic;

// 装備データ
public class EquippedData
{
    public string partsDataName;
    public List<GameObject> partObjs = new List<GameObject>();
    public List<ModifierBoneData> modifiedData = new List<ModifierBoneData>();
}

// ボーンごとの変更データ
public class ModifierBoneData
{
    public ModifierData boneModifierData;
    public List<ModifierData> childrenModifierData = new List<ModifierData>();

    public void ResetModifier()
    {
        foreach (var childData in childrenModifierData)
        {
            childData.ResetModifier();
        }

        List<Transform> children = new List<Transform>();
        Transform boneTF = boneModifierData.modifiedTF;
        for (int i = 0; i < boneTF.childCount; i++)
        {
            children.Add(boneTF.GetChild(i));
        }
        foreach (Transform child in children)
        {
            child.SetParent(boneTF.parent);
        }

        boneModifierData.ResetModifier();

        foreach (Transform child in children)
        {
            child.SetParent(boneTF);
        }
    }
}

// オブジェクトの変更前のデータ
public class ModifierData
{
    public Transform modifiedTF;
    public Vector3 localPos;
    public Quaternion localRot;
    public Vector3 localScale;

    public ModifierData(Transform tf)
    {
        modifiedTF = tf;
        localPos = tf.localPosition;
        localRot = tf.localRotation;
        localScale = tf.localScale;
    }

    public void ResetModifier()
    {
        modifiedTF.localPosition = localPos;
        modifiedTF.localRotation = localRot;
        modifiedTF.localScale = localScale;
    }
}

/// <summary>
/// メカ組み立てを管理するマネージャー
/// パーツの装着、取り外し、スケール補正、ボーンのスケーリングなどを処理
/// </summary>
public class MechAssemblyManager : MonoBehaviour
{
    public static MechAssemblyManager instance;

    [Header("スロットの親（例：MechRoot）")]
    [SerializeField] private Transform mechRoot;  // メカ全体のルートノード

    [Header("各スロットのTransform（部位ごとに設定）")]
    [SerializeField] private Transform headSlot;
    [SerializeField] private Transform bodySlot;
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private Transform weaponLSlot;
    [SerializeField] private Transform boosterSlot;

    // 複数装着が可能な部位（例：腕や脚）
    [SerializeField] private Transform[] lArmSlots;
    [SerializeField] private Transform[] rArmSlots;
    [SerializeField] private Transform[] legSlots;

    [SerializeField] private PlayerBase customPlayer;

    // 装着中のパーツ一覧（PartTypeごとにリスト）
    private Dictionary<PartType, List<EquippedData>> equippedParts = new();

    private void Awake()
    {
        instance = this;
        // 各 PartType に対して空のリストを初期化
        foreach (PartType type in System.Enum.GetValues(typeof(PartType)))
        {
            equippedParts[type] = new List<EquippedData>();
        }
    }

    private void Start()
    {
        SetPlayer(customPlayer);
    }

    private void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }

    public void SetPlayer(PlayerBase player)
    {
        if (player == null) return;

        customPlayer = player;

        //各部位を設定
        mechRoot = customPlayer.mechRoot;
        headSlot = customPlayer.headSlot;
        bodySlot = customPlayer.bodySlot;
        weaponSlot = customPlayer.weaponSlot;
        weaponLSlot = customPlayer.weaponLSlot;
        boosterSlot = customPlayer.boosterSlot;
        lArmSlots = customPlayer.lArmSlots;
        rArmSlots = customPlayer.rArmSlots;
        legSlots = customPlayer.legSlots;

        ApplySaveData();
    }

    private void ApplySaveData()
    {
        GameData.mechSaveData.Load();
        MechSaveData saveData = GameData.mechSaveData;

        foreach (var slot in saveData.slots)
        {
            PartType type;

            // スロット名を PartType に変換
            if (!System.Enum.TryParse(slot.slotName, out type)) continue;

            // Resources/Parts からプレハブを読み込む
            PartData partData = Resources.Load<PartData>($"PartsData/{slot.slotName}/{slot.partsDataName}");
            if (partData == null)
            {
                if (type == PartType.Weapon)
                {
                    string slotName = PartType.WeaponL.ToString();
                    partData = Resources.Load<PartData>($"PartsData/{slotName}/{slot.partsDataName}");
                }
                else if (type == PartType.WeaponL)
                {
                    string slotName = PartType.Weapon.ToString();
                    partData = Resources.Load<PartData>($"PartsData/{slotName}/{slot.partsDataName}");
                }

                if (partData == null)
                {
                    Debug.LogWarning($"プレハブ PartsData/{slot.slotName}/{slot.partsDataName} が読み込めませんでした");
                    continue;
                }
            }

            // ダミーの PartData を作成して装着
            AttachPart(partData, type);
        }
    }

    /// <summary>
    /// 指定されたパーツを対応するスロットに装着する
    /// </summary>
    public void AttachPart(PartData partData, PartType tabType)
    {
        if (partData.partPrefab == null && partData.multiPrefabs.Count == 0)
        {
            Debug.LogWarning("パーツプレハブが設定されていません");
            return;
        }


        // 複数装着可能な部位（例：腕や脚）
        if (partData.partType == PartType.L_Arm)
        {
            AttachToMultipleSlots(partData, lArmSlots, PartType.L_Arm);
        }
        else if (partData.partType == PartType.R_Arm)
        {
            AttachToMultipleSlots(partData, rArmSlots, PartType.R_Arm);
        }
        else if (partData.partType == PartType.Leg)
        {
            AttachToMultipleSlots(partData, legSlots, PartType.Leg);
        }
        else
        {
            PartType partType = partData.partType;
            if (partType == PartType.Weapon)
            {
                if(tabType == PartType.WeaponL)
                {
                    partType = PartType.WeaponL;
                }
            }

            // 単一スロットの場合
            Transform slot = GetSlotTransform(partType);
            if (slot == null) return;

            // 古いパーツを削除
            foreach (var part in equippedParts[partType])
            {
                ResetBoneScalesToPart(part.modifiedData);
                foreach (var obj in part.partObjs)
                {
                    Destroy(obj);
                }
            }
            equippedParts[partType].Clear();

            // プレハブをスロットに装着
            GameObject newPart = Instantiate(partData.partPrefab, slot);
            newPart.transform.localPosition = Vector3.zero;
            newPart.transform.localRotation = Quaternion.identity;
            newPart.transform.localScale = Vector3.one;

            // ▶️ 武器パーツなら CustomPlayer に装着
            if (partType == PartType.Weapon || partType == PartType.WeaponL)
            {
                IWeapon weapon = newPart.GetComponent<IWeapon>();
                if (weapon != null)
                {
                    if (partType == PartType.Weapon)
                    {
                        customPlayer.EquipWeapon(weapon, PlayerBase.WeaponSlot.RightHand);
                    }
                    else if (partType == PartType.WeaponL)
                    {
                        customPlayer.EquipWeapon(weapon, PlayerBase.WeaponSlot.LeftHand);
                    }
                }
                else
                {
                    Debug.LogWarning($"{newPart.name} は IWeapon を実装していません。");
                }
            }

            EquippedData data = new EquippedData();
            data.partObjs.Add(newPart);
            data.partsDataName = partData.name;

            // 生成したパーツに限定してスケーリング
            ApplyBoneScalesToPart(partData, newPart, ref data.modifiedData);

            // 🔧 スケール・位置・回転補正を適用
            ApplyPrefabScaleInfo(partData, newPart);


            // 装着リストに登録
            equippedParts[partType].Add(data);
        }

        // 🔁 その他のボーンスケーリング（必要であれば）
        //ApplyBoneScales(partData);
    }

    /// <summary>
    /// 複数スロットに対応したパーツを装着（例：両腕、脚）
    /// </summary>
    private void AttachToMultipleSlots(PartData partData, Transform[] slots, PartType partType)
    {
        // 既存のパーツを削除
        foreach (var part in equippedParts[partType])
        {
            ResetBoneScalesToPart(part.modifiedData);
            foreach (var obj in part.partObjs)
            {
                Destroy(obj);
            }
        }
        equippedParts[partType].Clear();

        EquippedData data = new EquippedData();
        data.partsDataName = partData.name;
        // 各スロットにプレハブを装着
        for (int i = 0; i < slots.Length && i < partData.multiPrefabs.Count; i++)
        {
            GameObject prefab = partData.multiPrefabs[i];
            if (prefab == null) continue;

            Transform slot = slots[i];
            GameObject newPart = Instantiate(prefab, slot);
            newPart.transform.localPosition = Vector3.zero;
            newPart.transform.localRotation = Quaternion.identity;

            data.partObjs.Add(newPart);

            // 🔧 スケール・位置・回転補正を適用
            ApplyMultiplePrefabScaleInfo(partData, prefab, newPart);
        }
        // 装着済みリストに追加
        equippedParts[partType].Add(data);
    }

    public Dictionary<PartType, List<EquippedData>> GetEquippedParts()
    {
        return equippedParts;
    }

    /// <summary>
    /// 指定したパーツのスケール・位置・回転補正を適用する
    /// </summary>
    private void ApplyMultiplePrefabScaleInfo(PartData partData, GameObject originPrefab, GameObject instantiatedPart)
    {
        if (partData.scaleInfos == null || partData.scaleInfos.Count == 0) return;

        foreach (var scaleInfo in partData.scaleInfos)
        {
            if (scaleInfo.prefab == null) continue;

            // 対象プレハブと一致するスケール情報を適用
            if (scaleInfo.prefab.name == originPrefab.name)
            {
                instantiatedPart.transform.localScale = scaleInfo.scale;
                instantiatedPart.transform.localPosition = scaleInfo.positionOffset;
                instantiatedPart.transform.localRotation = Quaternion.Euler(scaleInfo.rotationOffset);
                break;
            }
            
        }
    }

    /// <summary>
    /// 指定したパーツのスケール・位置・回転補正を適用する
    /// </summary>
    private void ApplyPrefabScaleInfo(PartData partData, GameObject instantiatedPart)
    {
        if (partData.scaleInfos == null || partData.scaleInfos.Count == 0) return;

        foreach (var scaleInfo in partData.scaleInfos)
        {
            if (scaleInfo.prefab == null) continue;

            // 対象プレハブと一致するスケール情報を適用
            if (scaleInfo.prefab.name == partData.partPrefab.name)
            {
                instantiatedPart.transform.localScale = scaleInfo.scale;
                instantiatedPart.transform.localPosition = scaleInfo.positionOffset;
                instantiatedPart.transform.localRotation = Quaternion.Euler(scaleInfo.rotationOffset);
                break;
            }
        }
    }

    /// <summary>
    /// 指定した PartType に対応するスロット Transform を取得
    /// </summary>
    private Transform GetSlotTransform(PartType partType)
    {
        return partType switch
        {
            PartType.Head => headSlot,
            PartType.Body => bodySlot,
            PartType.Weapon => weaponSlot,
            PartType.WeaponL => weaponLSlot,
            PartType.Booster => boosterSlot,
            _ => null,
        };
    }

    private void ResetBoneScalesToPart(List<ModifierBoneData> modifiers)
    {
        foreach (var modifierBoneData in modifiers)
        {
            modifierBoneData.ResetModifier();
        }
    }

    /// <summary>
    /// このパーツを装着したGameObject内部のボーンのみをスケーリングする
    /// </summary>
    private void ApplyBoneScalesToPart(PartData partData, GameObject instantiatedPart, ref List<ModifierBoneData> modifiers)
    {
        if (partData.boneScales == null) return;

        foreach (var boneScale in partData.boneScales)
        {
            // パーツ GameObject 内部から探す
            Transform targetBone = FindChildTransformRecursive(mechRoot, boneScale.boneName);
            if (targetBone != null)
            {
                List<Transform> children = new List<Transform>();
                for(int i = 0; i < targetBone.childCount; i++)
                {
                    children.Add(targetBone.GetChild(i));
                }
                foreach(Transform child in children)
                {
                    child.SetParent(targetBone.parent);
                }

                // 変更前の状態を記憶しておく
                ModifierBoneData data = new ModifierBoneData();
                data.boneModifierData = new ModifierData(targetBone);

                targetBone.localScale = boneScale.scale;

                foreach (Transform child in children)
                {
                    child.SetParent(targetBone);

                    var offsetInfo = boneScale.offsetInfos.Find(_ => _.childName == child.name);
                    if (offsetInfo != null)
                    {
                        data.childrenModifierData.Add(new ModifierData(child));
                        child.localPosition += offsetInfo.offset;
                    }
                }

                modifiers.Add(data);
            }
            else
            {
                Debug.LogWarning($"[{instantiatedPart.name}] 内にボーン '{boneScale.boneName}' が見つかりません");
            }
        }
    }

    //private void ApplyBoneScales(PartData partData)
    //{
    //    foreach (var boneScale in partData.boneScales)
    //    {
    //        // メカルート以下から該当ボーンを探す
    //        Transform targetBone = FindChildTransformRecursive(mechRoot, boneScale.boneName);
    //        if (targetBone != null)
    //        {
    //            targetBone.localScale = boneScale.scale;
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"スケーリング対象のボーン {boneScale.boneName} が見つかりませんでした。");
    //        }
    //    }
    //}

    /// <summary>
    /// ボーン名で子オブジェクトを再帰的に探す
    /// </summary>
    private Transform FindChildTransformRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;

        foreach (Transform child in parent)
        {
            var result = FindChildTransformRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
