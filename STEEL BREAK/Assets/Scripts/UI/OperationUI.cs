using UnityEngine;
using System.Collections;
using TMPro;

public class OperationUI : MonoBehaviour
{
    [Header("タイマー(TMP)")]
    [SerializeField] private TMP_Text timerText;

    [Header("警告表示 (フェード用 CanvasGroup)")]
    [SerializeField] private CanvasGroup warningGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    /// <summary>
    /// 警告文をフェードインさせる
    /// </summary>
    public void ShowWarning()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeWarning(0f, 1f, fadeDuration));
    }

    /// <summary>
    /// 警告文をフェードアウトさせる
    /// </summary>
    public void HideWarning()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeWarning(1f, 0f, fadeDuration));
    }

    /// <summary>
    /// タイマー更新（小数第2位）
    /// </summary>
    public void UpdateTimer(float time)
    {
        timerText.text = time.ToString("F2");
    }

    /// <summary>
    /// 警告文フェードのコルーチン
    /// </summary>
    private IEnumerator FadeWarning(float start, float end, float duration)
    {
        float t = 0f;

        warningGroup.alpha = start;
        warningGroup.gameObject.SetActive(true);

        while (t < duration)
        {
            t += Time.deltaTime;
            warningGroup.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        warningGroup.alpha = end;

        if (end == 0f)
            warningGroup.gameObject.SetActive(false);
    }
}
