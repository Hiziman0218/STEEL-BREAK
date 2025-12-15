using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameData
{
    public static MissionData currentSelected;
    public static MechSaveData mechSaveData = new MechSaveData();

    // Resources/Prefabs 以下に置いた ResultMenu Prefab のパス
    private const string ResultMenuPath = "Result/Result";

    /// <summary>
    /// ゲームクリア画面を動的生成して表示
    /// </summary>
    public static void ShowGameClear()
    {
        ShowResult(EndType.GameClear);
    }

    /// <summary>
    /// ゲームオーバー画面を動的生成して表示
    /// </summary>
    public static void ShowGameOver()
    {
        ShowResult(EndType.GameOver);
    }

    // Result.cs 側の enum
    public enum EndType { GameClear, GameOver }

    /// <summary>
    /// 共通生成処理
    /// </summary>
    private static void ShowResult(EndType type)
    {
        // 既に表示済みなら生成しない
        if (Object.FindObjectOfType<Result>() != null)
            return;

        // Prefab をロード
        var prefab = Resources.Load<GameObject>(ResultMenuPath);
        if (prefab == null)
        {
            Debug.LogError($"ResultMenu Prefab が Resources/{ResultMenuPath}.prefab に見つかりません");
            return;
        }

        // Canvas の子として生成したい場合は第2引数に parent を渡す
        var instance = Object.Instantiate(prefab);
        var result = instance.GetComponent<Result>();
        if (result == null)
        {
            Debug.LogError("生成した Prefab に Result コンポーネントがアタッチされていません");
            return;
        }

        // クリア／オーバー モードをセット
        result.endType = (type == EndType.GameClear)
            ? Result.EndType.GameClear
            : Result.EndType.GameOver;

        // ゲーム停止
        Time.timeScale = 0f;
    }
}

/// <summary>
/// スロットごとのセーブデータ構造
/// </summary>
[System.Serializable]
public class SlotSaveData
{
    public string slotName;      // スロットの名前（PartType列挙型の文字列）
    public string partsDataName;    // パーツデータの名前（Resources/PartsData にある）

    public int ap;                 // 個別パーツのAP
    public int weight;             // 個別パーツの重量
}

/// <summary>
/// 機体全体のセーブデータ構造
/// </summary>
[System.Serializable]
public class MechSaveData
{
    public List<SlotSaveData> slots = new();  // 各スロットのパーツ情報リスト

    public int totalAP;            // 合計AP
    public int totalWeight;        // 合計重量

    [SerializeField] private string saveFileName = "mech_save.json";  // セーブファイル名

    private string GetSavePath()
    {
        string saveDirectory;

        #if UNITY_EDITOR
            saveDirectory = Path.Combine(Application.dataPath, "Savedata");
        #else
            saveDirectory = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Savedata");
        #endif

        return Path.Combine(saveDirectory, saveFileName);
    }

    /// <summary>
    /// 現在の装備情報を保存する
    /// </summary>
    public void Save()
    {
        MechAssemblyManager assemblyManager = MechAssemblyManager.instance;
        if (assemblyManager == null) return;

        MechSaveData data = new MechSaveData();

        int totalAp = 0;
        int totalWeight = 0;

        foreach (var kvp in assemblyManager.GetEquippedParts())
        {
            foreach (var part in kvp.Value)
            {
                if (part == null || part.partData == null) continue;

                int ap = part.partData.ap;
                int weight = part.partData.weight;

                Debug.Log($"SAVE PART: {part.partsDataName} | AP={ap} | Weight={weight}");

                totalAp += ap;
                totalWeight += weight;

                data.slots.Add(new SlotSaveData
                {
                    slotName = kvp.Key.ToString(),
                    partsDataName = part.partData.name,
                    ap = ap,
                    weight = weight
                });
            }
        }

        data.totalAP = totalAp;
        data.totalWeight = totalWeight;

        GameData.mechSaveData = data;

        string path = GetSavePath();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log("メカ構成を保存しました。");
    }


    /// <summary>
    /// 保存された装備情報を読み込む
    /// </summary>
    public void Load()
    {
        string path = GetSavePath();

        if (!File.Exists(path))
        {
            Debug.LogWarning("保存ファイルが見つかりません: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        MechSaveData data = JsonUtility.FromJson<MechSaveData>(json);

        slots = data.slots;
        totalAP = data.totalAP;
        totalWeight = data.totalWeight;

        Debug.Log("メカ構成を読み込みました: " + path);
    }

    public static MissionData currentSelected;
    public static MechSaveData mechSaveData = new MechSaveData();

    /// <summary>
    /// セーブデータに保存されている合計APを取得
    /// </summary>
    public int GetTotalAP()
    {
        return totalAP;
    }

    /// <summary>
    /// セーブデータに保存されている合計Weightを取得
    /// </summary>
    public int GetTotalWeight()
    {
        return totalWeight;
    }


}