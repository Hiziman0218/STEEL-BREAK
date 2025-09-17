using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RuntimeInitialize
{
    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        GameData.mechSaveData.Load();

        SceneHistoryManager.Create();
    }
}
