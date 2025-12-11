using UnityEngine;

public class PlayerTransparentController : MonoBehaviour
{
    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Target renderers (透明化する対象)")]
    public Renderer[] targetRenderers;

    [Header("Distance Settings")]
    public float transparentDistance = 1.2f; // 透明化開始
    public float opaqueDistance = 1.5f;      // 不透明化開始（ヒステリシス）

    [Header("Fade Settings")]
    public float fadeSpeed = 10f; // フェードの速さ
    public float targetAlpha = 0.3f; // 透明化後のα値

    private bool isTransparent = false;
    private Material[] mats;

    void Start()
    {
        // Materialインスタンス化（共有マテリアル破壊防止）
        mats = new Material[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            mats[i] = targetRenderers[i].material;
        }
    }

    void Update()
    {
        float dist = Vector3.Distance(cameraTransform.position, transform.position);

        // 透明化開始
        if (!isTransparent && dist < transparentDistance)
        {
            isTransparent = true;
        }
        // 元に戻す
        else if (isTransparent && dist > opaqueDistance)
        {
            isTransparent = false;
        }

        UpdateAlpha();
    }

    void UpdateAlpha()
    {
        float target = isTransparent ? targetAlpha : 1f;

        foreach (Material m in mats)
        {
            Color c = m.color;
            c.a = Mathf.Lerp(c.a, target, Time.deltaTime * fadeSpeed);
            m.color = c;

            // 透明化に必要な設定
            if (isTransparent)
            {
                m.SetFloat("_Surface", 1); // Transparent
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                m.SetFloat("_Surface", 0); // Opaque
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            }
        }
    }
}
