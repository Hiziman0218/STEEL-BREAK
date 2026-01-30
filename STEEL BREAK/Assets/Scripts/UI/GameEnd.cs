using UnityEngine;
using TMPro;

public class GameEnd : MonoBehaviour
{
    [Header("最上位オブジェクト")]
    public GameObject root;

    [Header("メニュー項目(上から順に)")]
    [SerializeField] private TextMeshProUGUI[] menuItems;

    [Header("表示文言")]
    [SerializeField]
    private string[] menuTexts =
    {
        "ゲームを終了する",
        "ゲームを終了しない",
    };

    void Start()
    {
        Time.timeScale = 0f;

        // 文言反映
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].text = menuTexts[i];
        }

        //マウスカーソルを表示する
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// ゲーム続行
    /// </summary>
    public void GameContinue()
    {
        if (FindObjectOfType<GameManager>() == null)
        {
            Time.timeScale = 1f;
        }
        else
        {
            //マウスカーソルを非表示
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        //自身を削除
        Destroy(root);
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    public void GameQuit()
    {
        Time.timeScale = 1f;
        //ゲーム終了
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
