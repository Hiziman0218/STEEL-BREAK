using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// シーン遷移履歴を管理するマネージャー。
/// 自動生成され、履歴付きの LoadScene / GoBack が利用可能。
/// </summary>
public class SceneHistoryManager : MonoBehaviour
{
    //=== シングルトン ===//
    public static SceneHistoryManager Instance;

    //=== シーン履歴（スタック形式で管理）===//
    private static Stack<string> sceneHistory = new Stack<string>();

    //=== 現在シーン名 ===//
    private static string currentSceneName;

    //========================================
    // 🚀 初期化（ゲーム起動時に自動実行）
    //========================================
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Create();
    }

    /// <summary>
    /// インスタンスを生成（存在しない場合のみ）
    /// </summary>
    public static void Create()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("SceneHistoryManager");
            Instance = obj.AddComponent<SceneHistoryManager>();
            DontDestroyOnLoad(obj);
            Debug.Log("[SceneHistoryManager] Created automatically.");
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    //========================================
    // 🧭 シーン遷移（履歴を記録しつつ）
    //========================================
    public static void LoadScene(string sceneName)
    {
        if (Instance == null)
            Create();

        // 現在のシーンを履歴に追加
        string activeScene = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(activeScene))
        {
            sceneHistory.Push(activeScene);
        }

        currentSceneName = sceneName;
        Debug.Log($"[SceneHistoryManager] LoadScene → {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    //========================================
    // 🔙 戻る処理
    //========================================
    public static void GoBack()
    {
        if (Instance == null)
            Create();

        if (sceneHistory.Count > 0)
        {
            string previous = sceneHistory.Pop();
            currentSceneName = previous;
            Debug.Log($"[SceneHistoryManager] GoBack → {previous}");
            SceneManager.LoadScene(previous);
        }
        else
        {
            Debug.LogWarning("[SceneHistoryManager] 戻る履歴がありません。");
        }
    }

    //========================================
    // 🧩 現在・履歴の確認
    //========================================
    public static void PrintHistory()
    {
        string current = SceneManager.GetActiveScene().name;
        Debug.Log($"[SceneHistoryManager] 現在: {current}, 履歴数: {sceneHistory.Count}");

        foreach (var scene in sceneHistory)
        {
            Debug.Log($" ┗ {scene}");
        }
    }

    //========================================
    // 🧱 現在のシーンを履歴に手動で保存（必要時のみ）
    //========================================
    public void SaveCurrentScene()
    {
        string current = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(current))
        {
            sceneHistory.Push(current);
            Debug.Log($"[SceneHistoryManager] SaveCurrentScene → {current}");
        }
    }
}
