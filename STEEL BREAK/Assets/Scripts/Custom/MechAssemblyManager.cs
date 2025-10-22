using UnityEngine;
using System.Collections.Generic;
using Game.Enum;

//========================================
// 📦 EquippedData : 現在装着中のパーツ情報
//========================================
public class EquippedData
{
    public string partsDataName;                         // パーツデータ名（保存や復元用）
    public List<GameObject> partObjs = new();            // 実際に装着されているプレハブオブジェクト群
    public List<ModifierBoneData> modifiedData = new();  // このパーツで変更したボーン情報（リセット用）
}

//========================================
// ⚙️ ModifierBoneData : ボーンの変更データ管理
//========================================
public class ModifierBoneData
{
    public ModifierData boneModifierData;                  // 変更されたボーンの情報
    public List<ModifierData> childrenModifierData = new();// 子ボーンや子オブジェクトの変更情報

    // 🔄 ボーンを元の状態に戻す処理
    public void ResetModifier()
    {
        // 子データ（子Transform）をリセット
        foreach (var childData in childrenModifierData)
            childData.ResetModifier();

        // 現在の子を一時退避して親を変更（スケールリセットのため）
        List<Transform> children = new();
        Transform boneTF = boneModifierData.modifiedTF;
        for (int i = 0; i < boneTF.childCount; i++)
            children.Add(boneTF.GetChild(i));

        // 一時的に親から外す
        foreach (Transform child in children)
            child.SetParent(boneTF.parent);

        // ボーン自体の位置・回転・スケールを初期値に戻す
        boneModifierData.ResetModifier();

        // 子を再度戻す
        foreach (Transform child in children)
            child.SetParent(boneTF);
    }
}

//========================================
// 🧩 ModifierData : 1つのTransformの変更前情報
//========================================
public class ModifierData
{
    public Transform modifiedTF;      // 対象のTransform
    public Vector3 localPos;          // 元のローカル位置
    public Quaternion localRot;       // 元のローカル回転
    public Vector3 localScale;        // 元のローカルスケール

    public ModifierData(Transform tf)
    {
        modifiedTF = tf;
        localPos = tf.localPosition;
        localRot = tf.localRotation;
        localScale = tf.localScale;
    }

    // 🧭 状態を元に戻す
    public void ResetModifier()
    {
        modifiedTF.localPosition = localPos;
        modifiedTF.localRotation = localRot;
        modifiedTF.localScale = localScale;
    }
}

// バック武装の左右を指定するための列挙
public enum BackSide
{
    Right,
    Left
}

//========================================
// 🦾 MechAssemblyManager : メカ組み立ての中心管理クラス
//========================================
public class MechAssemblyManager : MonoBehaviour
{
    public static MechAssemblyManager instance; // シングルトン（1つだけ存在）

    [Header("スロットの親（例：MechRoot）")]
    [SerializeField] private Transform mechRoot;  // メカ全体のルート（Rigなど）

    [Header("各スロットのTransform（部位ごとに設定）")]
    [SerializeField] private Transform headSlot;     // 頭パーツの取付位置
    [SerializeField] private Transform bodySlot;     // 胴パーツの取付位置
    [SerializeField] private Transform weaponSlot;   // 右武器スロット
    [SerializeField] private Transform weaponLSlot;  // 左武器スロット
    [SerializeField] private Transform boosterSlot;  // ブースター位置

    // 複数装着できる部位（例：両腕、脚）
    [SerializeField] private Transform[] lArmSlots;  // 左腕の装着位置
    [SerializeField] private Transform[] rArmSlots;  // 右腕の装着位置
    [SerializeField] private Transform[] legSlots;   // 脚の装着位置

    [SerializeField] private PlayerBase customPlayer; // 対象プレイヤー（メカ制御クラス）

    // 🔗 現在装着中のパーツを PartType ごとに管理
    private Dictionary<PartType, List<EquippedData>> equippedParts = new();

    private void Awake()
    {
        instance = this;

        // 各パーツ種別を初期化（空リスト作成）
        foreach (PartType type in System.Enum.GetValues(typeof(PartType)))
            equippedParts[type] = new List<EquippedData>();
    }

    private void Start()
    {
        SetPlayer(customPlayer);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    //========================================
    // 🎮 プレイヤー設定（スロット情報をリンク）
    //========================================
    public void SetPlayer(PlayerBase player)
    {
        if (player == null) return;

        customPlayer = player;

        // プレイヤー内のスロットを同期
        mechRoot = customPlayer.mechRoot;
        headSlot = customPlayer.headSlot;
        bodySlot = customPlayer.bodySlot;
        weaponSlot = customPlayer.weaponSlot;
        weaponLSlot = customPlayer.weaponLSlot;
        boosterSlot = customPlayer.boosterSlot;
        lArmSlots = customPlayer.lArmSlots;
        rArmSlots = customPlayer.rArmSlots;
        legSlots = customPlayer.legSlots;

        // 保存データからパーツを復元
        ApplySaveData();
    }

    //========================================
    // 💾 セーブデータを読み込み・装着反映
    //========================================
    private void ApplySaveData()
    {
        GameData.mechSaveData.Load();
        MechSaveData saveData = GameData.mechSaveData;

        foreach (var slot in saveData.slots)
        {
            PartType type;

            // スロット名文字列 → Enum に変換
            if (!System.Enum.TryParse(slot.slotName, out type)) continue;

            // 該当パーツデータを Resources からロード
            PartData partData = Resources.Load<PartData>($"PartsData/{slot.slotName}/{slot.partsDataName}");

            // 武器スロットの左右違いを考慮して再検索
            if (partData == null)
            {
                if (type == PartType.Weapon)
                    partData = Resources.Load<PartData>($"PartsData/WeaponL/{slot.partsDataName}");
                else if (type == PartType.WeaponL)
                    partData = Resources.Load<PartData>($"PartsData/Weapon/{slot.partsDataName}");

                if (partData == null)
                {
                    Debug.LogWarning($"PartsData/{slot.slotName}/{slot.partsDataName} が見つかりません");
                    continue;
                }
            }

            // 実際に装着処理
            AttachPart(partData, type);
        }
    }

    //========================================
    // 🧩 パーツ装着メイン処理
    //========================================
    public void AttachPart(PartData partData, PartType tabType)
    {
        if (partData.partPrefab == null && partData.multiPrefabs.Count == 0)
        {
            Debug.LogWarning("⚠️ パーツプレハブが設定されていません");
            return;
        }

        // 複数装着パーツ（例：両腕・脚）
        if (partData.partType == PartType.L_Arm)
            AttachToMultipleSlots(partData, lArmSlots, PartType.L_Arm);
        else if (partData.partType == PartType.R_Arm)
            AttachToMultipleSlots(partData, rArmSlots, PartType.R_Arm);
        else if (partData.partType == PartType.Leg)
            AttachToMultipleSlots(partData, legSlots, PartType.Leg);

        else
        {
            PartType partType = partData.partType;

            // 左右の武器スロットを調整
            if (partType == PartType.Weapon && tabType == PartType.WeaponL)
                partType = PartType.WeaponL;

            // 左右の肩武器スロットを調整
            if (partType == PartType.BWeapon && tabType == PartType.BWeaponL)
                partType = PartType.BWeaponL;

            // 対応スロット取得
            Transform slot = GetSlotTransform(partType);
            if (slot == null) return;

            // 旧パーツ削除
            foreach (var part in equippedParts[partType])
            {
                ResetBoneScalesToPart(part.modifiedData);
                foreach (var obj in part.partObjs)
                    Destroy(obj);
            }
            equippedParts[partType].Clear();

            // 新パーツ生成
            GameObject newPart = Instantiate(partData.partPrefab, slot);
            newPart.transform.localPosition = Vector3.zero;
            newPart.transform.localRotation = Quaternion.identity;
            newPart.transform.localScale = Vector3.one;

            // 🔫 武器なら CustomPlayer に登録
            if (partType == PartType.Weapon || partType == PartType.WeaponL || partType == PartType.BWeapon || partType == PartType.BWeaponL)
            {
                IWeapon weapon = newPart.GetComponent<IWeapon>();
                if (weapon != null)
                {
                    switch (partType)
                    {
                        case PartType.Weapon:
                            customPlayer.EquipWeapon(weapon, WeaponSlot.RightHand);
                            break;

                        case PartType.WeaponL:
                            customPlayer.EquipWeapon(weapon, WeaponSlot.LeftHand);
                            break;

                        case PartType.BWeapon:
                            customPlayer.EquipWeapon(weapon, WeaponSlot.RightBack);
                            break;

                        case PartType.BWeaponL:
                            customPlayer.EquipWeapon(weapon, WeaponSlot.LeftBack);
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning($"{newPart.name} は IWeapon を実装していません。");
                }
            }

            // パーツ情報を登録
            EquippedData data = new()
            {
                partsDataName = partData.name
            };
            data.partObjs.Add(newPart);

            // 🔧 スケール・位置・回転補正
            ApplyPrefabScaleInfo(partData, newPart);

            // 🔧 ボーンスケーリング適用
            ApplyBoneScalesToPart(partData, newPart, ref data.modifiedData);



            // 登録
            equippedParts[partType].Add(data);
        }
    }

    //========================================
    // 🦿 複数スロット装着処理（両腕・脚）
    //========================================
    private void AttachToMultipleSlots(PartData partData, Transform[] slots, PartType partType)
    {
        // 旧パーツ削除
        foreach (var part in equippedParts[partType])
        {
            ResetBoneScalesToPart(part.modifiedData);
            foreach (var obj in part.partObjs)
                Destroy(obj);
        }
        equippedParts[partType].Clear();

        EquippedData data = new() { partsDataName = partData.name };

        // 各スロットに順番にプレハブ生成
        for (int i = 0; i < slots.Length && i < partData.multiPrefabs.Count; i++)
        {
            GameObject prefab = partData.multiPrefabs[i];
            if (prefab == null) continue;

            Transform slot = slots[i];
            GameObject newPart = Instantiate(prefab, slot);
            newPart.transform.localPosition = Vector3.zero;
            newPart.transform.localRotation = Quaternion.identity;

            data.partObjs.Add(newPart);

            // スケール補正
            ApplyBoneScalesToPart(partData, newPart, ref data.modifiedData);
            ApplyMultiplePrefabScaleInfo(partData, prefab, newPart);
        }

        equippedParts[partType].Add(data);
    }

    //========================================
    // 🧾 現在装着中パーツを取得
    //========================================
    public Dictionary<PartType, List<EquippedData>> GetEquippedParts()
    {
        return equippedParts;
    }

    //========================================
    // 📏 個別プレハブのスケール補正適用
    //========================================
    private void ApplyMultiplePrefabScaleInfo(PartData partData, GameObject originPrefab, GameObject instantiatedPart)
    {
        if (partData.scaleInfos == null || partData.scaleInfos.Count == 0) return;

        foreach (var scaleInfo in partData.scaleInfos)
        {
            if (scaleInfo.prefab == null) continue;

            if (scaleInfo.prefab.name == originPrefab.name)
            {
                instantiatedPart.transform.localScale = scaleInfo.scale;
                instantiatedPart.transform.localPosition = scaleInfo.positionOffset;
                instantiatedPart.transform.localRotation = Quaternion.Euler(scaleInfo.rotationOffset);
                break;
            }
        }
    }

    //========================================
    // 📏 単一パーツのスケール補正適用
    //========================================
    private void ApplyPrefabScaleInfo(PartData partData, GameObject instantiatedPart)
    {
        if (partData.scaleInfos == null || partData.scaleInfos.Count == 0) return;

        foreach (var scaleInfo in partData.scaleInfos)
        {
            if (scaleInfo.prefab == null) continue;

            if (scaleInfo.prefab.name == partData.partPrefab.name)
            {
                instantiatedPart.transform.localScale = scaleInfo.scale;
                instantiatedPart.transform.localPosition = scaleInfo.positionOffset;
                instantiatedPart.transform.localRotation = Quaternion.Euler(scaleInfo.rotationOffset);
                break;
            }
        }
    }

    //========================================
    // 🧩 パーツ種別 → スロット参照取得
    //========================================
    private Transform GetSlotTransform(PartType partType)
    {
        return partType switch
        {
            PartType.Head => headSlot,
            PartType.Body => bodySlot,
            PartType.Weapon => weaponSlot,
            PartType.WeaponL => weaponLSlot,
            PartType.Booster => boosterSlot,
            PartType.BWeapon => customPlayer.rightBackTransform,
            PartType.BWeaponL => customPlayer.leftBackTransform,
            _ => null,
        };
    }

    //========================================
    // 🔄 ボーンスケールを元に戻す
    //========================================
    private void ResetBoneScalesToPart(List<ModifierBoneData> modifiers)
    {
        foreach (var modifierBoneData in modifiers)
            modifierBoneData.ResetModifier();
    }

    //========================================
    // 🦴 ボーンスケール適用（装着パーツ内のみ）
    //========================================
    private void ApplyBoneScalesToPart(PartData partData, GameObject instantiatedPart, ref List<ModifierBoneData> modifiers)
    {
        if (partData.boneScales == null) return;

        foreach (var boneScale in partData.boneScales)
        {
            // メカルート以下から対象ボーンを探索
            Transform targetBone = FindChildTransformRecursive(mechRoot, boneScale.boneName);
            if (targetBone != null)
            {
                // 子を一時的に外す（スケール反映のため）
                List<Transform> children = new();
                for (int i = 0; i < targetBone.childCount; i++)
                    children.Add(targetBone.GetChild(i));

                foreach (Transform child in children)
                    child.SetParent(targetBone.parent);

                // 元データ記録
                ModifierBoneData data = new();
                data.boneModifierData = new ModifierData(targetBone);

                // スケール適用
                targetBone.localScale = boneScale.scale;

                // 子を戻して位置補正
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

    //========================================
    // 🔍 子階層からボーン名で検索
    //========================================
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
