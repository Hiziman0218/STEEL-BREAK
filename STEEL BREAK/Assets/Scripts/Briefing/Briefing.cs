using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI[] menuItems;  // UI��̃��j���[����
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    private int currentIndex = 0;

    void Start()
    {
        // �}�E�X����p�n���h�����e���j���[�ɒǉ�
        for (int i = 0; i < menuItems.Length; i++)
        {
            var handler = menuItems[i].gameObject.AddComponent<MenuItemMouseHandler>();
            handler.Setup(i, this);
        }

        UpdateMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentIndex = (currentIndex - 1 + menuItems.Length) % menuItems.Length;
            UpdateMenu();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentIndex = (currentIndex + 1) % menuItems.Length;
            UpdateMenu();
        }

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

    public void SetCurrentIndex(int idx)
    {
        currentIndex = idx;
        UpdateMenu();
    }

    public void SelectMenuFromIndex(int idx)
    {
        currentIndex = idx;
        UpdateMenu();
        SelectMenu();
    }

    void SelectMenu()
    {
        switch (currentIndex)
        {
            case 0:
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
                Debug.Log("����`�̃��j���[");
                break;
        }
    }
}
