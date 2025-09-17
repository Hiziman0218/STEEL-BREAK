Shader "Custom/AnimatedNoiseEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // ���C���e�N�X�`���i���g�p�j
        _Strength ("Noise Strength", Range(0,1)) = 0.5 // �m�C�Y�̋���
        _Speed ("Noise Animation Speed", Range(0.1, 10)) = 1.0 // �A�j���[�V�������x
        _Alpha ("Alpha (�����x)", Range(0,1)) = 1.0 // �m�C�Y�̓����x
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" } // �d�˕\���p
        Blend SrcAlpha OneMinusSrcAlpha // �A���t�@�u�����h�ݒ�
        ZWrite Off // �[�x�o�b�t�@�������݃I�t
        Cull Off // ���ʕ`��

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
                float4 vertex : POSITION; // ���_���W
                float2 uv : TEXCOORD0;    // UV���W
            };

            struct v2f
            {
                float2 uv : TEXCOORD0; // UV��n��
                float4 vertex : SV_POSITION; // �`��ʒu
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // ���_�ϊ�
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // ���ԂɊ�Â����m�C�Y�p���W
                float2 uvTime = i.uv * 10.0 + _Time.y * _Speed;

                // �^���m�C�Y�����i�n�b�V���֐��j
                float noise = frac(sin(dot(uvTime.xy, float2(12.9898, 78.233))) * 43758.5453);

                // ���x�K�p
                float value = lerp(0.5, noise, _Strength);

                // �ŏI�F�FRGB�Ƀm�C�Y�l�AAlpha�Ɏw��l��K�p
                return fixed4(value, value, value, _Alpha);
            }
            ENDCG
        }
    }}