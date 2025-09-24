using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PixelPlay.Utils;

public class LockOn : MonoBehaviour
{
    [Header("索敵設定")]
    [Tooltip("ロックオン可能な最大距離")]
    public float detectionRange = 20f;

    [Tooltip("敵のレイヤー")]
    public LayerMask enemyLayer;

    private bool lockOnEnabled = true; //ロックオン機能の有効/無効フラグ

    private Transform currentTarget; //現在のロックオン対象
    public Transform CurrentTarget => currentTarget; //現在のロックオン対象(外部参照用)

    private Enemy currentEnemy; //イベント登録用に保持
    private InputManager input; //入力受け取りクラス
    private List<Transform> candidates = new List<Transform>(); //ターゲット候補リスト

    private void Start()
    {
        input = GetComponent<InputManager>();
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
            if (dist > detectionRange || !candidates.Contains(currentTarget))
            {
                Unlock();
                return;
            }

            //カメラ中心から最も近い敵に自動でロックし直す
            Transform nearestToCenter = FindClosestToScreenCenter(candidates);
            if (nearestToCenter != null && nearestToCenter != currentTarget)
            {
                Lock(nearestToCenter);
            }
        }

        //未ロック → 自動ロック
        if (candidates.Count > 0)
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
        //検出前のコライダー数をログ
        var cols = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);

        //Enemy & 生存フィルタ後の数をログ
        candidates = cols
            .Select(c => c.GetComponentInParent<Enemy>())
            .Where(e => e != null && e.IsAlive)
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

        OnLock(target);
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

            OnUnlock(currentTarget);

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
    /// 画面の中心から最も近い敵を探索
    /// </summary>
    /// <param name="candidates"></param>
    /// <returns></returns>
    private Transform FindClosestToScreenCenter(List<Transform> candidates)
    {
        if (candidates == null || candidates.Count == 0 || Camera.main == null)
            return null;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (Transform t in candidates)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(t.position);
            // カメラの前方にいる敵だけ対象にする
            if (screenPos.z < 0) continue;

            float dist = Vector2.Distance(screenCenter, screenPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = t;
            }
        }

        return closest;
    }

    /// <summary>
    /// 次のターゲットへ切り替え
    /// </summary>
    private void SwitchTarget()
    {
        if (candidates.Count < 2) return;
        int idx = candidates.IndexOf(currentTarget);
        if (idx < 0) idx = 0;
        idx = (idx + 1) % candidates.Count;
        Lock(candidates[idx]);
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
    /// ロック通知(デバッグ)
    /// </summary>
    /// <param name="target"></param>
    private void OnLock(Transform target)
    {
        Debug.Log($"ロックオン対象を発見 : {target.name}");
    }

    /// <summary>
    /// ロック解除通知(デバッグ)
    /// </summary>
    /// <param name="target"></param>
    private void OnUnlock(Transform target)
    {
        Debug.Log($"ロックオン対象を喪失 : {target.name}");
    }

    /// <summary>
    /// ギズモを表示
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
