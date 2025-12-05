using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OperationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject warningPanel;     //警告パネル
    [SerializeField] private Text warningText;            //警告文
    [SerializeField] private GameObject timerParent;      //タイマーパネル全体
    [SerializeField] private TextMeshProUGUI timerText;   //タイマー文字

    [Header("Warning Text Pulse")]
    [SerializeField] private float warningMinAlpha = 0.5f;
    [SerializeField] private float warningMaxAlpha = 1.0f;
    [SerializeField] private float pulseSpeed = 2f;

    private bool isWarningActive = false;
    private float pulseTime = 0f;

    private void Start()
    {
        //各UIを非表示に
        warningPanel.SetActive(false);
        timerParent.SetActive(false);
    }

    public void ShowWarning(bool show)
    {
        isWarningActive = show;

        warningPanel.SetActive(show);
        timerParent.SetActive(show);
    }

    public void UpdateTimer(float time)
    {
        //タイマーの数値を小数点第2位まで表示
        timerText.text = time.ToString("F2");
    }

    private void Update()
    {
        if (!isWarningActive) return;

        //パルス処理
        pulseTime += Time.deltaTime * pulseSpeed;

        float alpha = Mathf.Lerp(
            warningMinAlpha,
            warningMaxAlpha,
            (Mathf.Sin(pulseTime) + 1f) / 2f
        );

        Color c = warningText.color;
        c.a = alpha;
        warningText.color = c;
    }
}
