using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Result : MonoBehaviour
{
    public enum EndType { GameClear, GameOver }

    [Header("�I���^�C�v")]
    public EndType endType = EndType.GameOver;

    [Header("�e�p�l��")]
    [SerializeField] private GameObject clearPanel;   // �N���A�p�p�l��
    [SerializeField] private GameObject overPanel;    // �Q�[���I�[�o�[�p�p�l��

    [Header("���ʃ��j���[���� (Retry, Title)")]
    [SerializeField] private TextMeshProUGUI[] menuItems;

    [Header("�p�l�����o���e�L�X�g")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("�\�������ݒ�")]
    [SerializeField] private string clearTitle = "Game Clear!";
    [SerializeField] private string overTitle = "Game Over";
    [SerializeField] private string retryText = "Retry";
    [SerializeField] private string returnText = "Return to Title";

    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    private int currentIndex = 0;

    void Start()
    {
        // �܂����p�l���Ƃ���\����
        clearPanel.SetActive(false);
        overPanel.SetActive(false);

        // ���[�h�ɍ��킹���p�l����\�����A�^�C�g����ݒ�
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

        // ���j���[���ڂ̕�����ݒ�
        // �i���Ԃ� Retry=0, Return=1 �̑O��j
        menuItems[0].text = retryText;
        menuItems[1].text = returnText;

        UpdateMenu();
    }

    void Update()
    {
        // ��/W
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentIndex = (currentIndex - 1 + menuItems.Length) % menuItems.Length;
            UpdateMenu();
        }
        // ��/S
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentIndex = (currentIndex + 1) % menuItems.Length;
            UpdateMenu();
        }
        // ���� (Enter/Z)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
        {
            SelectMenu();
        }
    }

    /// <summary>
    /// ���j���[���ڂ̐F���X�V
    /// </summary>
    void UpdateMenu()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].color = (i == currentIndex) ? selectedColor : normalColor;
        }
    }

    /// <summary>
    /// ���j���[���莞�̋���
    /// </summary>
    void SelectMenu()
    {
        switch (currentIndex)
        {
            case 0:
                // Retry �� ���݂̃V�[�����ēǍ�
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;
            case 1:
                // Return �� ���C�����j���[��
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu"); // �^�C�g���V�[�����ɍ��킹��
                break;
        }
    }
}