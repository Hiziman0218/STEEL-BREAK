using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("ゲーム終了確認UI")]
    public GameObject GameEndUIPrefab;

    [Header("メニュー項目(上から順に)")]
    [SerializeField] private TextMeshProUGUI[] menuItems;

    [Header("表示文言")]
    [SerializeField]
    private string[] menuTexts =
    {
        "ゲームを続ける",
        "リトライ",
        "メインメニューに戻る",
        "ゲームを終了する"
    };

    [Header("色設定")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    private int currentIndex = 0;
    private bool isPaused = false;
    private GameObject GameEndUI = null;

    void Start()
    {
        // 初期状態では非表示
        gameObject.SetActive(false);

        // 文言反映
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].text = menuTexts[i];
        }
    }

    void Update()
    {
        if (!isPaused) return;
        if (GameEndUI != null) return;

        // 上 / W
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentIndex = (currentIndex - 1 + menuItems.Length) % menuItems.Length;
            UpdateMenu();
        }

        // 下 / S
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentIndex = (currentIndex + 1) % menuItems.Length;
            UpdateMenu();
        }

        // 決定
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
        {
            SelectMenu();
        }

        // ESCでも解除可能に
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Resume();
        }
    }

    /// <summary>
    /// ポーズ開始
    /// </summary>
    public void Pause()
    {
        //リザルトがあるなら、ポーズ不可能
        if (FindObjectOfType<Result>() != null) return;

        isPaused = true;
        currentIndex = 0;
        UpdateMenu();

        Time.timeScale = 0f;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// ポーズ解除
    /// </summary>
    void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 選択色更新
    /// </summary>
    void UpdateMenu()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].color = (i == currentIndex) ? selectedColor : normalColor;
        }
    }

    /// <summary>
    /// メニュー決定処理
    /// </summary>
    void SelectMenu()
    {
        Time.timeScale = 1f;

        switch (currentIndex)
        {
            case 0:
                // ゲームを続ける
                Resume();
                break;

            case 1:
                // リトライ
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;

            case 2:
                // メインメニューへ
                SceneManager.LoadScene("MainMenu");
                break;

            case 3:
                // ゲーム終了
                GameEndUI = Instantiate(GameEndUIPrefab);
                break;
        }
    }
}