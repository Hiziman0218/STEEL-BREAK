using UnityEditor;
using UnityEngine;

public class GameExit : MonoBehaviour
{
    private GameObject exitConfirmInstance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (FindObjectOfType<GameExit>() == null)
        {
            GameObject obj = new GameObject("GameExit");
            obj.AddComponent<GameExit>();
            DontDestroyOnLoad(obj);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // GameManager がある = ミッション中
            if (FindObjectOfType<GameManager>() != null)
            {
                // 何もしない（ポーズ画面側に任せる）
                return;
            }

            // 非プレイ中 → 終了確認UIを出す
            ShowExitConfirm();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }
#endif
    }

    void ShowExitConfirm()
    {
        if (exitConfirmInstance != null && exitConfirmInstance)
            return;

        GameObject prefab = Resources.Load<GameObject>("UI/GameEnd");

        if (prefab == null)
        {
            Debug.LogError("ExitConfirm prefab not found");
            return;
        }

        exitConfirmInstance = Instantiate(prefab);
    }
}
