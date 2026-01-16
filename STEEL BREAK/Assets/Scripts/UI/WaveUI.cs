using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentWaveText;
    [SerializeField] private TextMeshProUGUI totalWaveText;

    private void OnEnable()
    {
        Field.OnWaveChanged += UpdateWaveText;
    }

    private void OnDisable()
    {
        Field.OnWaveChanged -= UpdateWaveText;
    }

    private void UpdateWaveText(int current, int total)
    {
        currentWaveText.text = $"{current}";
        totalWaveText.text = $"{total}";
    }
}