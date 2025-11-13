using UnityEngine;
using System.IO;

public class TitleSceneManager : MonoBehaviour
{
    void Start()
    {
        InitializeDefaultSaveData();
    }

    private void InitializeDefaultSaveData()
    {
        string saveDirectory;
        string saveFileName = "mech_save.json";

#if UNITY_EDITOR
        saveDirectory = Path.Combine(Application.dataPath, "Savedata");
#else
        saveDirectory = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Savedata");
#endif

        string savePath = Path.Combine(saveDirectory, saveFileName);

        // 🔍 すでにセーブデータがあるなら何もしない
        if (File.Exists(savePath))
        {
            Debug.Log("既存のセーブデータがあるため、初期装備はスキップしました。");
            return;
        }

        Debug.Log("セーブデータが存在しないため、初期装備を作成して保存します。");

        // MechSaveData インスタンスを新規作成
        MechSaveData defaultData = new MechSaveData();

        // ======== 初期装備を設定 ========
        // ここでスロットとパーツ名を設定（例）
        defaultData.slots.Add(new SlotSaveData
        {
            slotName = "Head",
            partsDataName = "Atlas(Head)"
        });
        defaultData.slots.Add(new SlotSaveData
        {
            slotName = "Body",
            partsDataName = "Atlas(Body)"
        });
        defaultData.slots.Add(new SlotSaveData
        {
            slotName = "L_Arm",
            partsDataName = "Atlas(L_Arm)"
        });
        defaultData.slots.Add(new SlotSaveData
        {
            slotName = "R_Arm",
            partsDataName = "Atlas(R_Arm)"
        });
        defaultData.slots.Add(new SlotSaveData
        {
            slotName = "Leg",
            partsDataName = "Atlas(Lge)"
        });
        defaultData.slots.Add(new SlotSaveData
        {
            slotName = "Weapon",
            partsDataName = "ショットガン"
        });
        defaultData.slots.Add(new SlotSaveData
        {
            slotName = "WeaponL",
            partsDataName = "アサルトライフル"
        });
        defaultData.slots.Add(new SlotSaveData
        {
            slotName = "Booster",
            partsDataName = "Booster2"
        });
        defaultData.slots.Add(new SlotSaveData
        {
            slotName = "BWeapon",
            partsDataName = "ガトリング"
        });
        defaultData.slots.Add(new SlotSaveData
        {
            slotName = "BWeaponL",
            partsDataName = "ミサイル"
        });

        // JSON化して保存
        Directory.CreateDirectory(saveDirectory);
        string json = JsonUtility.ToJson(defaultData, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"初期装備を保存しました: {savePath}");
    }
}
