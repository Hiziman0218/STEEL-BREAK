using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class MissionSelectInput : MonoBehaviour
{
    [SerializeField] private Transform buttonParent; // �{�^����z�u����e
    private List<Button> buttons = new List<Button>();

    void Start()
    {
        // �{�^�������������Ă�
        GenerateButtons();

        // �{�^��������Ȃ�ŏ��̃{�^����I����Ԃɂ���
        if (buttons.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
    }

    void GenerateButtons()
    {
        // �����Ń{�^���𐶐�����i��j
        // ���ۂ̓f�[�^���X�g���瓮�I�ɍ��z��
        for (int i = 0; i < 3; i++)
        {
            GameObject btnObj = new GameObject("Button" + i, typeof(RectTransform), typeof(Button), typeof(Image));
            btnObj.transform.SetParent(buttonParent, false);

            Button btn = btnObj.GetComponent<Button>();
            buttons.Add(btn);

            // �N���b�N�C�x���g�ǉ�
            int index = i;
            btn.onClick.AddListener(() => OnMissionSelected(index));
        }

        // Navigation ��ݒ�i�����̏ꍇ�͕s�v�j
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
        // Enter�L�[�őI�𒆂̃{�^�������s
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
        Debug.Log("�I�����ꂽ�~�b�V����: " + index);
    }
}
