using UnityEngine;

public class SmoothRotator : MonoBehaviour
{
    Vector3 currentRotationDir;
    Vector3 targetRotationDir;

    [Header("回転速度")]
    public float rotationSpeed = 100f;
    [Header("方向の変化速度")]
    public float directionSmoothness = 0.1f;
    [Header("方向切り替え間隔")]
    public float directionChangeInterval = 2f;

    float timer;

    void Start()
    {
        targetRotationDir = GetRandomDir();
        currentRotationDir = targetRotationDir;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 一定間隔で方向を更新
        if (timer >= directionChangeInterval)
        {
            targetRotationDir = GetRandomDir();
            timer = 0f;
        }

        // 徐々に方向を変えていく
        currentRotationDir = Vector3.Slerp(currentRotationDir, targetRotationDir, directionSmoothness * Time.deltaTime);

        // 回転処理
        transform.Rotate(currentRotationDir * rotationSpeed * Time.deltaTime);
    }

    Vector3 GetRandomDir()
    {
        // より自然なランダム方向を生成
        return Random.onUnitSphere;
    }
}
