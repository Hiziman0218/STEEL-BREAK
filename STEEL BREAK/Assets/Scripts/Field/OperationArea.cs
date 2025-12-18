using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    private readonly List<Enemy> enemies = new();
    public Bounds AreaBounds { get; private set; }

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        AreaBounds = col.bounds;
    }

    /// <summary>
    /// ¶¬‚µ‚½“G‚ğƒŠƒXƒg‚É’Ç‰Á
    /// </summary>
    /// <param name="enemy">¶¬‚µ‚½“G</param>
    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    /// <summary>
    /// “G‚ğƒŠƒXƒg‚©‚çíœ
    /// </summary>
    /// <param name="enemy"></param>
    public void UnregisterEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
    }

    // ===============================
    // “G‚ÌS‘©ˆ—
    // ===============================
    private void LateUpdate()
    {
        AreaBounds = GetComponent<Collider>().bounds;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];
            if (enemy == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            ClampEnemy(enemy);
        }
    }

    /// <summary>
    /// “G‚ÌS‘©
    /// </summary>
    /// <param name="enemy"></param>
    private void ClampEnemy(Enemy enemy)
    {
        Vector3 pos = enemy.transform.position;
        Bounds b = AreaBounds;

        Vector3 clamped = new Vector3(
            Mathf.Clamp(pos.x, b.min.x, b.max.x),
            Mathf.Clamp(pos.y, b.min.y, b.max.y),
            Mathf.Clamp(pos.z, b.min.z, b.max.z)
        );

        if (pos != clamped)
        {
            enemy.ForceSetPosition(clamped);
            enemy.OnBlockedByOperationArea();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOutside = true;
            timer = timeLimit;
            ui.ShowWarning(true);

            // SEƒRƒ‹[ƒ`ƒ“ŠJn
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

            // ƒRƒ‹[ƒ`ƒ“’â~
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
            player.Destruction();

            isOutside = false;
            ui.ShowWarning(false);

            // ƒRƒ‹[ƒ`ƒ“’â~
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }
        }
    }

    /// <summary>
    /// ”ÍˆÍŠO‚É‚¢‚éŠÔAŒxSE‚ğˆê’èŠÔŠu‚Å–Â‚ç‚µ‘±‚¯‚é
    /// </summary>
    private IEnumerator PlayWarningSELoop()
    {
        // ”ÍˆÍŠO‚É‚¢‚éŠÔƒ‹[ƒv
        while (true)
        {
            audioSource.PlayOneShot(warningSE);  // Œp‚¬–Ú‚È‚µ‚ÅÄ¶

            // ”CˆÓ‚ÌŠÔŠui1•bj‘Ò‚Â
            yield return new WaitForSeconds(interval);
        }
    }
}
