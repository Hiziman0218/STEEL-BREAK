using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class Briefing : MonoBehaviour
{
    public TextMeshProUGUI[] menuItems;  // UI上のメニュー項目
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    private int currentIndex = 0;

    void Start()
    {
        UpdateMenu();
        GameData.currentSelected = null;

        // 各メニューにマウスイベントを登録
        for (int i = 0; i < menuItems.Length; i++)
        {
            int index = i;
            // イベントトリガーを追加（存在しない場合）
            EventTrigger trigger = menuItems[i].gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = menuItems[i].gameObject.AddComponent<EventTrigger>();
            }

            // ホバー時の処理
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((eventData) =>
            {
                currentIndex = index;
                UpdateMenu();
            });
            trigger.triggers.Add(entryEnter);

            // クリック時の処理
            EventTrigger.Entry entryClick = new EventTrigger.Entry();
            entryClick.eventID = EventTriggerType.PointerClick;
            entryClick.callback.AddListener((eventData) =>
            {
                currentIndex = index;
                SelectMenu();
            });
            trigger.triggers.Add(entryClick);
        }
    }

    void Update()
    {
        // キーボード操作にも対応
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

    void SelectMenu()
    {
        switch (currentIndex)
        {
            case 0:
                SceneHistoryManager.LoadScene("Mission");
                break;
            case 1:
                SceneHistoryManager.LoadScene("Custom");
                break;
            case 2:
                SceneHistoryManager.LoadScene("Title");
                break;
            default:
                Debug.Log("未定義のメニュー");
                break;
        }
    }
}
