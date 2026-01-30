using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PixelPlay.Utils;

public class LockOn : MonoBehaviour
{
    [Header("索敵設定")]
    [Tooltip("ロックオン可能な最大距離")]
    public float detectionRange = 30f;
    [Tooltip("ロックを解除する距離のオフセット")]
    [SerializeField] private float unlockRangeOffset = 10f;
    [Tooltip("ロックオン可能な最大角度")]
    public float maxAngle = 60f;
    [Tooltip("敵のレイヤー")]
    public LayerMask enemyLayer;

    [Header("ボス索敵設定")]
    [Tooltip("ボス用の索敵距離")]
    public float bossDetectionRange = 50f;

    private bool lockOnEnabled = true; //ロックオン機能の有効/無効フラグ

    private float unlockRange = 22f; //ロックを解除する距離

    private Transform currentTarget; //現在のロックオン対象
    public Transform CurrentTarget => currentTarget; //現在のロックオン対象(外部参照用)

    private Enemy currentEnemy; //イベント登録用に保持
    private InputManager input; //入力受け取りクラス
    private List<Transform> candidates = new List<Transform>(); //ターゲット候補リスト

    private void Start()
    {
        input = GetComponent<InputManager>();

        //ロックを解除する距離を計算
        unlockRange = detectionRange + unlockRangeOffset;
    }

    void Update()
    {
        // トグル切り替え
        if (input.IsLockOnCancel)
        {
            lockOnEnabled = !lockOnEnabled;
            if (!lockOnEnabled) Unlock();
            return;
        }
        if (!lockOnEnabled) return;

        //ボスを優先的にロック
        if (input.IsTargetBoss)
        {
            TryBossLock();
        }

        // ターゲット候補更新
        RefreshCandidates();

        // 生存・範囲チェック → アンロック
        if (currentTarget != null)
        {
            //死亡チェック
            var enemy = currentTarget.GetComponentInParent<Enemy>();
            if (enemy == null || !enemy.IsAlive)
            {
                Unlock();
                return;
            }

            //範囲外チェック
            float dist = Vector3.Distance(transform.position, currentTarget.position);
            if (dist > unlockRange)
            {
                Unlock();
                return;
            }
        }

        // 未ロック時のみ自動ロックを行う
        if (currentTarget == null && candidates.Count > 0)
        {
            //候補リストの先頭(最も近い)を取り出して画面内判定
            Transform candidate = candidates[0];
            Vector3 screenPos = ScreenUtility.WorldToScreen(Camera.main, candidate.position);
            if (ScreenUtility.IsInScreen(screenPos))
            {
                //画面内にいるときだけロック処理を呼ぶ
                Lock(candidate);
            }
        }
    }

    //範囲内の敵を検出し、距離順にソート
    private void RefreshCandidates()
    {
        var cols = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);

        candidates = cols
            .Select(c => c.GetComponentInParent<Enemy>())
            .Where(e => e != null && e.IsAlive)
            .Where(e =>
            {
                // 前方との角度チェック
                Vector3 dirToEnemy = (e.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToEnemy);
                return angle < maxAngle; // プレイヤーの前方範囲のみ通す
            })
            .Select(e => e.transform)
            .Distinct()
            .OrderBy(t => Vector3.Distance(transform.position, t.position))
            .ToList();
    }

    /// <summary>
    /// ロックオン開始
    /// </summary>
    /// <param name="target">ロックオンするターゲット</param>
    private void Lock(Transform target)
    {
        if (currentTarget == target) return;

        // 古いターゲットのフラグ解除
        if (currentTarget != null)
        {
            var oldTargetScript = currentTarget.GetComponent<Target>();
            if (oldTargetScript != null)
                oldTargetScript.IsLockedOn = false;
        }

        // 古いイベント解除
        if (currentEnemy != null)
        {
            currentEnemy.OnDeath -= OnTargetDeath;
            currentEnemy = null;
        }

        // 新しいターゲットに更新
        currentTarget = target;

        // 新しいイベント登録
        currentEnemy = target.GetComponentInParent<Enemy>();
        if (currentEnemy != null)
        {
            currentEnemy.OnDeath += OnTargetDeath;
        }

        // 新ターゲットの Target スクリプトをオンに
        var targetScript = target.GetComponent<Target>();
        if (targetScript != null)
        {
            targetScript.IsLockedOn = true;
        }
    }

    /// <summary>
    /// ロック解除
    /// </summary>
    private void Unlock()
    {
        if (currentTarget != null)
        {
            //現ターゲットのTargetスクリプトをオフに
            var targetScript = currentTarget.GetComponent<Target>();
            if (targetScript != null)
            {
                targetScript.IsLockedOn = false;
            }

            //イベント解除
            if (currentEnemy != null)
            {
                currentEnemy.OnDeath -= OnTargetDeath;
                currentEnemy = null;
            }

            currentTarget = null;
        }
    }

    /// <summary>
    /// 次のターゲットへ切り替え
    /// </summary>
    public void SwitchTarget()
    {
        if (candidates.Count < 2)
        {
            Debug.Log("変更できる対象がいません。");
            return;
        }
        int idx = candidates.IndexOf(currentTarget);
        if (idx < 0) idx = 0;
        idx = (idx + 1) % candidates.Count;
        Lock(candidates[idx]);
        Debug.Log("ターゲットを変更しました。");
    }

    /// <summary>
    /// ボスを優先的にロック
    /// </summary>
    private void TryBossLock()
    {
        var cols = Physics.OverlapSphere(
            transform.position,
            bossDetectionRange,
            enemyLayer
        );

        var boss = cols
            .Select(c => c.GetComponentInParent<Boss>())
            .FirstOrDefault(b =>
            {
                if (b == null) return false;

                var enemy = b.GetComponent<Enemy>();
                if (enemy == null || !enemy.IsAlive) return false;

                Vector3 dir = (b.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dir);

                return angle < maxAngle;
            });

        if (boss != null)
        {
            Lock(boss.transform);
            Debug.Log("ボスを優先ロックしました");
        }
    }

    /// <summary>
    /// 敵の死亡通知を受け取る
    /// </summary>
    /// <param name="enemy"></param>
    private void OnTargetDeath(Enemy enemy)
    {
        if (enemy == currentEnemy)
        {
            Debug.Log($"ロックオン対象 : {enemy.name} が死亡、ロックオンを解除");
            Unlock(); // 死亡と同時にロック解除
        }
    }

    /// <summary>
    /// ギズモを表示(デバッグ)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, bossDetectionRange);

        // 前方視野を描画
        Vector3 forward = transform.forward * bossDetectionRange;
        Quaternion leftRot = Quaternion.AngleAxis(-maxAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(maxAngle, Vector3.up);
        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftDir);
        Gizmos.DrawLine(transform.position, transform.position + rightDir);
    }
}