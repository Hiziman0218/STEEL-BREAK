using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Result : MonoBehaviour
{
    public enum EndType { GameClear, GameOver }

    [Header("終了タイプ")]
    public EndType endType = EndType.GameOver;

    [Header("各パネル")]
    [SerializeField] private GameObject clearPanel;   // クリア用パネル
    [SerializeField] private GameObject overPanel;    // ゲームオーバー用パネル

    [Header("共通メニュー項目 (Retry, Title)")]
    [SerializeField] private TextMeshProUGUI[] menuItems;

    [Header("パネル見出しテキスト")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("表示文言設定")]
    [SerializeField] private string clearTitle = "Game Clear!";
    [SerializeField] private string overTitle = "Game Over";
    [SerializeField] private string retryText = "Retry";
    [SerializeField] private string returnText = "Return to Title";

    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    private int currentIndex = 0;

    void Start()
    {
        // まず両パネルとも非表示に
        clearPanel.SetActive(false);
        overPanel.SetActive(false);

        // モードに合わせたパネルを表示し、タイトルを設定
        if (endType == EndType.GameClear)
        {
            clearPanel.SetActive(true);
            titleText.text = clearTitle;
        }
        else
        {
            overPanel.SetActive(true);
            titleText.text = overTitle;
        }

        // メニュー項目の文言を設定
        // （順番は Retry=0, Return=1 の前提）
        menuItems[0].text = retryText;
        menuItems[1].text = returnText;

        UpdateMenu();
    }

    void Update()
    {
        // 上/W
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentIndex = (currentIndex - 1 + menuItems.Length) % menuItems.Length;
            UpdateMenu();
        }
        // 下/S
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentIndex = (currentIndex + 1) % menuItems.Length;
            UpdateMenu();
        }
        // 決定 (Enter/Z)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
        {
            SelectMenu();
        }
    }

    /// <summary>
    /// メニュー項目の色を更新
    /// </summary>
    void UpdateMenu()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].color = (i == currentIndex) ? selectedColor : normalColor;
        }
    }

    /// <summary>
    /// メニュー決定時の挙動
    /// </summary>
    void SelectMenu()
    {
        switch (currentIndex)
        {
            case 0:
                // Retry → 現在のシーンを再読込
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;
            case 1:
                // Return → メインメニューへ
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu"); // タイトルシーン名に合わせて
                break;
        }
    }
}