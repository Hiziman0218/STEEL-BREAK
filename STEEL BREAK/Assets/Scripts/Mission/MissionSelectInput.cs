using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class MissionSelectInput : MonoBehaviour
{
    [SerializeField] private Transform buttonParent; // ボタンを配置する親
    private List<Button> buttons = new List<Button>();

    void Start()
    {
        // ボタン生成処理を呼ぶ
        GenerateButtons();

        // ボタンがあるなら最初のボタンを選択状態にする
        if (buttons.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
    }

    void GenerateButtons()
    {
        // ここでボタンを生成する（例）
        // 実際はデータリストから動的に作る想定
        for (int i = 0; i < 3; i++)
        {
            GameObject btnObj = new GameObject("Button" + i, typeof(RectTransform), typeof(Button), typeof(Image));
            btnObj.transform.SetParent(buttonParent, false);

            Button btn = btnObj.GetComponent<Button>();
            buttons.Add(btn);

            // クリックイベント追加
            int index = i;
            btn.onClick.AddListener(() => OnMissionSelected(index));
        }

        // Navigation を設定（自動の場合は不要）
        for (int i = 0; i < buttons.Count; i++)
        {
            Navigation nav = buttons[i].navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = buttons[(i - 1 + buttons.Count) % buttons.Count];
            nav.selectOnDown = buttons[(i + 1) % buttons.Count];
            buttons[i].navigation = nav;
        }
    }

    void Update()
    {
        // Enterキーで選択中のボタンを実行
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                Button button = selected.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                }
            }
        }
    }

    void OnMissionSelected(int index)
    {
        Debug.Log("選択されたミッション: " + index);
    }
}
