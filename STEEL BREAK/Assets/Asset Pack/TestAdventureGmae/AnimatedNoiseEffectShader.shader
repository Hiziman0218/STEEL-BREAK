Shader "Custom/AnimatedNoiseEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // メインテクスチャ（未使用）
        _Strength ("Noise Strength", Range(0,1)) = 0.5 // ノイズの強さ
        _Speed ("Noise Animation Speed", Range(0.1, 10)) = 1.0 // アニメーション速度
        _Alpha ("Alpha (透明度)", Range(0,1)) = 1.0 // ノイズの透明度
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" } // 重ね表示用
        Blend SrcAlpha OneMinusSrcAlpha // アルファブレンド設定
        ZWrite Off // 深度バッファ書き込みオフ
        Cull Off // 両面描画

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float _Strength;
            float _Speed;
            float _Alpha;

            struct appdata
            {
                float4 vertex : POSITION; // 頂点座標
                float2 uv : TEXCOORD0;    // UV座標
            };

            struct v2f
            {
                float2 uv : TEXCOORD0; // UVを渡す
                float4 vertex : SV_POSITION; // 描画位置
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 頂点変換
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 時間に基づいたノイズ用座標
                float2 uvTime = i.uv * 10.0 + _Time.y * _Speed;

                // 疑似ノイズ生成（ハッシュ関数）
                float noise = frac(sin(dot(uvTime.xy, float2(12.9898, 78.233))) * 43758.5453);

                // 強度適用
                float value = lerp(0.5, noise, _Strength);

                // 最終色：RGBにノイズ値、Alphaに指定値を適用
                return fixed4(value, value, value, _Alpha);
            }
            ENDCG
        }
    }}