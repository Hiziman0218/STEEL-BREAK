using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI[] menuItems;  // UI上のメニュー項目
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    private int currentIndex = 0;

    void Start()
    {
        UpdateMenu();
    }

    void Update()
    {
        // 上キー or W
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentIndex = (currentIndex - 1 + menuItems.Length) % menuItems.Length;
            UpdateMenu();
        }

        // 下キー or S
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentIndex = (currentIndex + 1) % menuItems.Length;
            UpdateMenu();
        }

        // 決定キー（Enter または Z）
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
        {
            SelectMenu();
        }
    }

    void UpdateMenu()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].color = (i == currentIndex) ? selectedColor : normalColor;
        }
    }

    void SelectMenu()
    {
        switch (currentIndex)
        {
            case 0:
                // 選択中ミッションの戦闘シーン名を取得
                string battleScene = GameData.currentSelected.battlesceneName;
                SceneHistoryManager.LoadScene(battleScene);
                break;
            case 1:
                SceneHistoryManager.LoadScene("Custom");
                break;
            case 2:
                SceneHistoryManager.LoadScene("Mission");
                break;
            default:
                Debug.Log("未定義のメニュー");
                break;
        }
    }
}
