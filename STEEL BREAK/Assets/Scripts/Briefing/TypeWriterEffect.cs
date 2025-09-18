using System;
using UnityEngine;
using TMPro;
using System.Collections;

public class TypeWriterEffect : MonoBehaviour
{
    // --- �C�x���g ---
    public event Action OnTypingFinished;     // �S�Ẵ��b�Z�[�W��\�����I������Ƃ��ɒʒm
    public event Action<int> OnMessageChanged; // �V�������b�Z�[�W��\������^�C�~���O�Œʒm�i�C���f�b�N�X�t���j

    [SerializeField] private TextMeshProUGUI textComponent; // ���b�Z�[�W�\���p��TextMeshProUGUI
    [SerializeField] private float typingSpeed = 0.05f;     // 1�������\������Ԋu�i�b�j

    private string[] messages;           // �\�����郁�b�Z�[�W�̔z��
    private int currentMessageIndex = 0; // ���ݕ\�����Ă��郁�b�Z�[�W�̃C���f�b�N�X

    private Coroutine typingCoroutine;   // ���s���̃R���[�`����ێ����邽�߂̕ϐ�
    private bool isTyping = false;       // ���݃^�C�s���O�����ǂ���
    private string currentMessage;       // ���ݕ\�����̃��b�Z�[�W�i�X�L�b�v�p�j

    /// <summary>
    /// ���b�Z�[�W�z����󂯎��A�ŏ��̃��b�Z�[�W�\�����J�n����
    /// </summary>
    public void StartTyping(string[] messages)
    {
        this.messages = messages;
        currentMessageIndex = 0;

        // ���b�Z�[�W�������ꍇ�͑��I��
        if (messages == null || messages.Length == 0)
        {
            OnTypingFinished?.Invoke();  // �C�x���g�𔭉�
            return;
        }

        ShowNextMessage();
    }

    /// <summary>
    /// ���̃��b�Z�[�W��\������
    /// </summary>
    private void ShowNextMessage()
    {
        // �S���b�Z�[�W��\�����I�������I���C�x���g
        if (currentMessageIndex >= messages.Length)
        {
            OnTypingFinished?.Invoke();
            return;
        }

        currentMessage = messages[currentMessageIndex];

        // ���b�Z�[�W���؂�ւ�������Ƃ�ʒm�i�{�C�X�Đ��Ȃǂɗ��p�j
        OnMessageChanged?.Invoke(currentMessageIndex);

        currentMessageIndex++;

        // ���ɃR���[�`���������Ă�����~�߂�i���̃��b�Z�[�W�ɐ؂�ւ��j
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 1�������\������R���[�`�����J�n
        typingCoroutine = StartCoroutine(TypeMessageCoroutine(currentMessage));
    }

    /// <summary>
    /// ���b�Z�[�W��1�������\������R���[�`��
    /// </summary>
    private IEnumerator TypeMessageCoroutine(string message)
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char c in message)
        {
            textComponent.text += c;                // 1�����ǉ�
            yield return new WaitForSeconds(typingSpeed); // �w�莞�ԑҋ@
        }

        isTyping = false; // �ł��I����������
    }

    /// <summary>
    /// �v���C���[���N���b�N/�L�[���͂����Ƃ��̋���
    /// </summary>
    public void OnUserClicked()
    {
        if (isTyping)
        {
            // �^�C�s���O���Ȃ瑦�\���i�X�L�b�v�j
            StopCoroutine(typingCoroutine);
            textComponent.text = currentMessage;
            isTyping = false;
        }
        else
        {
            // �^�C�s���O���I����Ă����玟�̃��b�Z�[�W��
            ShowNextMessage();
        }
    }

    private void Update()
    {
        // �X�y�[�X�L�[�������ꂽ��N���b�N����
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnUserClicked();
        }
    }
}
