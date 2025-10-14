using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移履歴を管理するクラス。
/// 「前のシーン → 現在のシーン」という情報を記録し、
/// 戻る（GoBack）処理や履歴付きのLoadScene処理を提供する。
/// </summary>
public class SceneHistoryManager : MonoBehaviour
{
    // シングルトンインスタンス（唯一の存在）
    public static SceneHistoryManager Instance;

    // 一つ前にいたシーン名を保存
    private static string previousSceneName;

    // 現在のシーン名を保存
    private static string currentSceneName;

    /// <summary>
    /// シーンヒストリーマネージャーを動的に生成する。
    /// まだ存在しない場合のみGameObjectを作成する。
    /// </summary>
    public static void Create()
    {
        // Instanceがまだ存在しない場合のみ生成
        if (Instance == null)
        {
            // 新しい空のGameObjectを作成
            GameObject obj = new GameObject("SceneHistoryManager");
            // このクラスをアタッチして実行可能にする
            obj.AddComponent<SceneHistoryManager>();
        }
    }

    private void Awake()
    {
        // Awakeはオブジェクト生成時に最初に呼ばれる

        // まだInstanceが設定されていなければ、現在のオブジェクトを登録
        if (Instance == null)
        {
            Instance = this;
            // シーンが切り替わっても破棄されないように設定
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // すでに存在している場合は重複防止のため削除
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// シーンを読み込む（履歴を保存しつつ）。
    /// SceneManager.LoadScene() を直接呼ぶ代わりにこれを使うことで、
    /// 前のシーン名を自動で記録できる。
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        // マネージャーが存在しない場合は処理しない
        if (Instance == null) return;

        // すでにcurrentSceneNameに何か入っている場合（2回目以降の遷移）
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            // 現在のシーンを前のシーンとして保存
            previousSceneName = currentSceneName;
        }
        else
        {
            // 初回遷移の場合は、現在のアクティブシーンを前のシーンとして記録
            previousSceneName = SceneManager.GetActiveScene().name;
        }

        // 今から遷移するシーン名を「現在のシーン名」として記録
        currentSceneName = sceneName;

        // 実際にシーンをロード
        SceneManager.LoadScene(currentSceneName);
    }

    /// <summary>
    /// 現在のシーンを履歴に保存するだけのメソッド。
    /// すぐに遷移しないが、「戻る」先を指定しておきたい場合などに使う。
    /// </summary>
    public void SaveCurrentScene()
    {
        // 現在アクティブなシーン名を保存
        previousSceneName = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// 直前に保存したシーンに戻る。
    /// previousSceneName が記録されていれば、そのシーンを読み込む。
    /// </summary>
    public static void GoBack()
    {
        // 戻る先が存在するかチェック
        if (!string.IsNullOrEmpty(previousSceneName))
        {
            // 前のシーンに戻る
            LoadScene(previousSceneName);
        }
        else
        {
            // 記録がない場合は警告を表示
            Debug.LogWarning("戻る先のシーンが保存されていません");
        }
    }
}
