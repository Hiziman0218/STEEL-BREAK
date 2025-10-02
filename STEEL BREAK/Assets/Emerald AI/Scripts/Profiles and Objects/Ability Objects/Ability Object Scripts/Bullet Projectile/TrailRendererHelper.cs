using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【TrailRendererHelper】
    /// TrailRenderer を使用するプロジェクタイルを再利用（リスポーン）する際、
    /// 前回の軌跡が残って見えないように、TrailRenderer の全ポイントをクリアします。
    /// </summary>
    public class TrailRendererHelper : MonoBehaviour
    {
        [Header("このコンポーネントに付与された TrailRenderer の参照（内部用）")]
        TrailRenderer m_TrailRenderer;

        void Awake()
        {
            m_TrailRenderer = GetComponent<TrailRenderer>();
        }

        void OnDisable()
        {
            // 無効化時に軌跡を消去し、再出現時の残像を防ぐ
            m_TrailRenderer.Clear();
        }
    }
}
