using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン履歴を管理するクラス。前のシーン名を保存し、戻る処理を行う。
/// </summary>
public class SceneHistoryManager : MonoBehaviour
{
    public static SceneHistoryManager Instance;

    private static string previousSceneName;

    private static string currentSceneName;

    public static void Create()
    {
        if(Instance == null)
        {
            GameObject obj = new GameObject("SceneHistoryManager");
            obj.AddComponent<SceneHistoryManager>();
        }
    }

    private void Awake()
    {
        // シングルトンとして保持
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン切り替えで消えないように
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance == null) return;

        if (!string.IsNullOrEmpty(currentSceneName))
        {
            previousSceneName = currentSceneName;
        }
        else
        {
            previousSceneName = SceneManager.GetActiveScene().name;
        }

        currentSceneName = sceneName;
        SceneManager.LoadScene(currentSceneName);
    }

    /// <summary>
    /// 現在のシーン名を履歴として保存する
    /// </summary>
    public void SaveCurrentScene()
    {
        previousSceneName = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// 保存したシーンに戻る
    /// </summary>
    public static void GoBack()
    {
        if (!string.IsNullOrEmpty(previousSceneName))
        {
            LoadScene(previousSceneName);
        }
        else
        {
            Debug.LogWarning("戻る先のシーンが保存されていません");
        }
    }
}
