using UnityEngine;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] CanvasGroup fadeCanvas;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator FadeOut(float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            fadeCanvas.alpha = t / duration;
            yield return null;
        }
        fadeCanvas.alpha = 1;
    }

    public IEnumerator FadeIn(float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            fadeCanvas.alpha = 1 - (t / duration);
            yield return null;
        }
        fadeCanvas.alpha = 0;
    }
}
