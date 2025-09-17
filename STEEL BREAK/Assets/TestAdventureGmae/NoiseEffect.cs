using UnityEngine;

[ExecuteInEditMode] // エディタ上でも動作させる
public class NoiseEffect : MonoBehaviour
{
    [Header("ノイズ描画に使用するマテリアル（Custom/AnimatedNoiseEffectShader）を割当ててください")]
    public Material m_NoiseMaterial; // ノイズ描画用マテリアル

    [Header("ノイズの強さ（0.0〜1.0）")]
    [Range(0f, 1f)]
    public float m_NoiseStrength = 0.5f; // ノイズの濃さ

    [Header("ノイズのアニメーション速度（0.1〜10）")]
    [Range(0.1f, 10f)]
    public float m_NoiseSpeed = 1.0f; // ノイズの動くスピード

    [Header("ノイズのアルファ値（透明度）0=透明、1=不透明")]
    [Range(0f, 1f)]
    public float m_Alpha = 1.0f; // ノイズの透明度

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // マテリアル未設定時は元画像をそのまま出力
        if (m_NoiseMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // ノイズ強度をマテリアルに反映
        m_NoiseMaterial.SetFloat("_Strength", m_NoiseStrength);

        // ノイズスピード（アニメーション速度）を反映
        m_NoiseMaterial.SetFloat("_Speed", m_NoiseSpeed);

        // アルファ値（透明度）を反映
        m_NoiseMaterial.SetFloat("_Alpha", m_Alpha);

        // 描画処理（ノイズエフェクト適用）
        Graphics.Blit(source, destination, m_NoiseMaterial);
    }
}
