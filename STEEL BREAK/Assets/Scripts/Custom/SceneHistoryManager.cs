using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �V�[���������Ǘ�����N���X�B�O�̃V�[������ۑ����A�߂鏈�����s���B
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
        // �V���O���g���Ƃ��ĕێ�
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �V�[���؂�ւ��ŏ����Ȃ��悤��
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
    /// ���݂̃V�[�����𗚗��Ƃ��ĕۑ�����
    /// </summary>
    public void SaveCurrentScene()
    {
        previousSceneName = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// �ۑ������V�[���ɖ߂�
    /// </summary>
    public static void GoBack()
    {
        if (!string.IsNullOrEmpty(previousSceneName))
        {
            LoadScene(previousSceneName);
        }
        else
        {
            Debug.LogWarning("�߂��̃V�[�����ۑ�����Ă��܂���");
        }
    }
}
