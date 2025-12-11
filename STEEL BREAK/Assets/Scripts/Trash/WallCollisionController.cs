using UnityEngine;

public class WallCollisionController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Movement movement;

    [Header("Position Push Back")]
    [SerializeField] private float pushStrength = 0.03f;

    [Header("Effect")]
    [SerializeField] private GameObject wallHitEffectPrefab;
    private GameObject currentEffect;

    private bool isTouchingWall = false;
    private Vector3 wallNormal = Vector3.zero;


    // Movement から呼ばれる。入力方向を制限する
    public Vector3 ModifyDirection(Vector3 dir)
    {
        if (!isTouchingWall) return dir;

        // 入力方向のうち壁内側への成分を削除
        float dot = Vector3.Dot(dir, wallNormal);
        if (dot > 0f)   // 壁方向（外側）に向かっている場合のみ削除
            dir -= wallNormal * dot;

        return dir.normalized;
    }


    private void Update()
    {
        if (!isTouchingWall) return;

        // ---- ① 位置補正（最後の保険） ----
        transform.position += wallNormal * pushStrength;

        // ---- ② 速度制御（最重要）----
        CancelVelocityAgainstWall();
    }


    /// <summary>
    /// 壁方向の速度（Movement 内部速度）を破壊する
    /// </summary>
    private void CancelVelocityAgainstWall()
    {
        if (movement == null) return;

        //Vector3 vel = movement.Velocity;
        //float dot = Vector3.Dot(vel, wallNormal);

        // 壁方向へ進む速度成分を打ち消す
        //if (dot > 0f)
        {
            //vel -= wallNormal * dot;
            //movement.SetVelocity(vel);
        }
    }


    // ---- Trigger ----

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out RangeWall wall))
        {
            isTouchingWall = true;
            wallNormal = wall.normal.normalized;

            PlayEffect(other.ClosestPoint(transform.position));
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out RangeWall wall))
        {
            isTouchingWall = true;
            wallNormal = wall.normal.normalized;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out RangeWall _))
        {
            isTouchingWall = false;
            StopEffect();
        }
    }



    // ---- Effect ----

    private void PlayEffect(Vector3 hitPoint)
    {
        if (wallHitEffectPrefab == null || currentEffect != null) return;

        currentEffect = Instantiate(wallHitEffectPrefab, hitPoint, Quaternion.identity);
        currentEffect.transform.forward = wallNormal;
    }

    private void StopEffect()
    {
        if (currentEffect != null)
            Destroy(currentEffect);

        currentEffect = null;
    }
}
