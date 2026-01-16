using UnityEngine;
using TMPro;
using System.Collections;

public class TypeWriterEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentMessage;

    public void StartTyping(string[] messages)
    {
        if (messages == null || messages.Length == 0) return;

        currentMessage = messages[0];

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

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

    /// <summary>
    /// 即全文表示（次メッセージには進まない）w
    /// </summary>
    public void ForceComplete()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (textComponent != null)
        {
            textComponent.text = currentMessage;
        }

        isTyping = false;
    }
}
