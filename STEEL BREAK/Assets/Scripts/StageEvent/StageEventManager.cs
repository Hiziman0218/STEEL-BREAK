using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class StageEventManager : MonoBehaviour
{
    [Header("UI参照")]
    public GameObject eventUIPanel;
    public Image eventImage;
    public TextMeshProUGUI eventText;
    public AudioSource voiceSource;

    private string[] messages;
    private string[] images;
    private string[] voices;
    private int currentIndex = 0;

    public void StartEvent(string eventDataPath)
    {
        // Resourcesからtxt読み込み
        TextAsset txt = Resources.Load<TextAsset>("StageEvents/" + eventDataPath);
        if (txt == null)
        {
            Debug.LogError($"StageEventManager: {eventDataPath} が見つかりません");
            return;
        }

        // 行解析
        string descriptionLine = "";
        string imagesLine = "";
        string voicesLine = "";

        string[] lines = txt.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (line.StartsWith("#")) continue;

            int colon = line.IndexOf(':');
            if (colon < 0) continue;

            string key = line.Substring(0, colon).Trim();
            string value = line.Substring(colon + 1).Trim();

            switch (key)
            {
                case "description": descriptionLine = value; break;
                case "images": imagesLine = value; break;
                case "voices": voicesLine = value; break;
            }
        }

        // descriptionは複数行に分割
        messages = descriptionLine.Split('|');

        // imagesは1個ならすべて同じにする、複数なら対応する数だけ
        string[] parsedImages = string.IsNullOrEmpty(imagesLine) ? new string[0] : imagesLine.Split('|');
        if (parsedImages.Length == 1 && messages.Length > 1)
        {
            images = new string[messages.Length];
            for (int i = 0; i < messages.Length; i++)
                images[i] = parsedImages[0];
        }
        else
        {
            images = parsedImages;
        }

        // voicesはメッセージ数と同じかチェック
        voices = string.IsNullOrEmpty(voicesLine) ? new string[0] : voicesLine.Split('|');
        if (voices.Length != messages.Length)
        {
            Debug.LogWarning($"StageEventManager: voices数({voices.Length})とmessages数({messages.Length})が一致しません");
        }

        currentIndex = 0;
        eventUIPanel.SetActive(true);
        ShowNext();
    }

    private void ShowNext()
    {
        if (currentIndex >= messages.Length)
        {
            eventUIPanel.SetActive(false);
            return;
        }

        // メッセージ表示
        eventText.text = messages[currentIndex];

        // 画像表示（あれば）
        if (images.Length > 0 && currentIndex < images.Length)
        {
            var sprite = Resources.Load<Sprite>(images[currentIndex]);
            if (sprite != null)
            {
                eventImage.sprite = sprite;
                eventImage.gameObject.SetActive(true);
            }
            else
            {
                eventImage.gameObject.SetActive(false);
            }
        }
        else
        {
            eventImage.gameObject.SetActive(false);
        }

        // ボイス再生
        if (voices.Length > currentIndex)
        {
            var clip = Resources.Load<AudioClip>(voices[currentIndex]);
            if (clip != null)
            {
                voiceSource.clip = clip;
                voiceSource.Play();
                StartCoroutine(WaitForVoiceThenNext(clip.length));
            }
            else
            {
                StartCoroutine(WaitForVoiceThenNext(0));
            }
        }
        else
        {
            StartCoroutine(WaitForVoiceThenNext(0));
        }
    }

    private IEnumerator WaitForVoiceThenNext(float voiceLength)
    {
        yield return new WaitForSeconds(voiceLength + 0.5f);
        currentIndex++;
        ShowNext();
    }
}
