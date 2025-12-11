using UnityEngine;
using System.Collections;

public class OperationArea : MonoBehaviour
{
    [SerializeField] private OperationUI ui;
    [SerializeField] private float timeLimit = 5f;

    [Header("Warning SE")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip warningSE;
    [SerializeField] private float interval = 1f;

    private float timer;
    private bool isOutside = false;
    private Coroutine warningCoroutine;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOutside = true;
            timer = timeLimit;
            ui.ShowWarning(true);

            // SEコルーチン開始
            if (warningCoroutine == null)
                warningCoroutine = StartCoroutine(PlayWarningSELoop());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOutside = false;
            ui.ShowWarning(false);

            // コルーチン停止
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }
        }
    }

    private void Update()
    {
        if (!isOutside) return;

        timer -= Time.deltaTime;
        ui.UpdateTimer(timer);

        if (timer <= 0f)
        {
            Player player = FindAnyObjectByType<Player>();
            player.GetDamage(player.GetStatus().GetMaxHP());

            isOutside = false;
            ui.ShowWarning(false);

            // コルーチン停止
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }
        }
    }

    /// <summary>
    /// 範囲外にいる間、警告SEを一定間隔で鳴らし続ける
    /// </summary>
    private IEnumerator PlayWarningSELoop()
    {
        // 範囲外にいる間ループ
        while (true)
        {
            audioSource.PlayOneShot(warningSE);  // 継ぎ目なしで再生

            // 任意の間隔（1秒）待つ
            yield return new WaitForSeconds(interval);
        }
    }
}
