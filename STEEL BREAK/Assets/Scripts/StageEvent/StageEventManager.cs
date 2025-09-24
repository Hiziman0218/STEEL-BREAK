using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class StageEventManager : MonoBehaviour
{
    [Header("UI�Q��")]
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
        // Resources����txt�ǂݍ���
        TextAsset txt = Resources.Load<TextAsset>("StageEvents/" + eventDataPath);
        if (txt == null)
        {
            Debug.LogError($"StageEventManager: {eventDataPath} ��������܂���");
            return;
        }

        // �s���
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

        // description�͕����s�ɕ���
        messages = descriptionLine.Split('|');

        // images��1�Ȃ炷�ׂē����ɂ���A�����Ȃ�Ή����鐔����
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

        // voices�̓��b�Z�[�W���Ɠ������`�F�b�N
        voices = string.IsNullOrEmpty(voicesLine) ? new string[0] : voicesLine.Split('|');
        if (voices.Length != messages.Length)
        {
            Debug.LogWarning($"StageEventManager: voices��({voices.Length})��messages��({messages.Length})����v���܂���");
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

        // ���b�Z�[�W�\��
        eventText.text = messages[currentIndex];

        // �摜�\���i����΁j
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

        // �{�C�X�Đ�
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
