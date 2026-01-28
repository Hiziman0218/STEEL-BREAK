using UnityEditor;
using UnityEngine;

public class GameExit : MonoBehaviour
{
    /*
    //起動時に自動で呼ばれる
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        //既に存在していなければ自動生成
        if (FindObjectOfType<GameExit>() == null)
        {
            GameObject obj = new GameObject("GameExit");
            obj.AddComponent<GameExit>();
            Object.DontDestroyOnLoad(obj);
        }
    }

    void Update()
    {
        //ESCキーで終了
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

#if UNITY_EDITOR
        //Pauseショートカット(Editor限定)
        if (Input.GetKeyDown(KeyCode.P))
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }
#endif
    }
    */
}
