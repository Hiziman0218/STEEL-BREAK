using System;
using UnityEngine;
using TMPro;
using System.Collections;

public class TypeWriterEffect : MonoBehaviour
{
    public event Action OnTypingFinished;

    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private float typingSpeed = 0.05f;

    private string[] messages;
    private int currentMessageIndex = 0;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentMessage;

    public void StartTyping(string[] messages)
    {
        this.messages = messages;
        currentMessageIndex = 0;

        if (messages == null || messages.Length == 0)
        {
            OnTypingFinished?.Invoke();
            return;
        }

        ShowNextMessage();
    }

    private void ShowNextMessage()
    {
        if (currentMessageIndex >= messages.Length)
        {
            OnTypingFinished?.Invoke();
            return;
        }

        currentMessage = messages[currentMessageIndex];
        currentMessageIndex++;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeMessageCoroutine(currentMessage));
    }

    private IEnumerator TypeMessageCoroutine(string message)
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char c in message)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void OnUserClicked()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            textComponent.text = currentMessage;
            isTyping = false;
        }
        else
        {
            ShowNextMessage();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnUserClicked();
        }
    }
}
