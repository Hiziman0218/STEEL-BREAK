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
        UpdateMenu();
    }

    void Update()
    {
        // ��L�[ or W
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentIndex = (currentIndex - 1 + menuItems.Length) % menuItems.Length;
            UpdateMenu();
        }

        // ���L�[ or S
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentIndex = (currentIndex + 1) % menuItems.Length;
            UpdateMenu();
        }

        // ����L�[�iEnter �܂��� Z�j
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
                // �I�𒆃~�b�V�����̐퓬�V�[�������擾
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
