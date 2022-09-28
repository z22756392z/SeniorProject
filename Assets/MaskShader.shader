Shader "Custom/Fog"
{
    Properties
    {
        [NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
        [NoScaleOffset]_Pattern("Pattern", 2D) = "white"{}
        _Health("Health",Range(0,1)) = 1.0
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
            //_BloodGlossiness("BloodSmoothness", Range(0,1)) = 0.5
            //_BloodMetallic("BloodMetallic", Range(0,1)) = 0.0
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard fullforwardshadows

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            sampler2D _MainTex;
            sampler2D _Pattern;
            struct Input
            {
                float2 uv_MainTex;
            };

            float _Health;
            float _Glossiness;
            float _Metallic;
            //float _BloodGlossiness;
            //float _BloodMetallic;
            float4 _Color;

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            UNITY_INSTANCING_BUFFER_START(Props)
                // put more per-instance properties here
            UNITY_INSTANCING_BUFFER_END(Props)

            float InverseLerp(float4 a, float4 b, float t) {
                return ((t - a) / (b - a));
            }
            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Albedo comes from a texture tinted by color
                float4 texCol = tex2D(_MainTex, IN.uv_MainTex);
                float4 pattern = tex2D(_Pattern, IN.uv_MainTex);
                
                float4 finalColor = lerp(pattern + 0.3, texCol, saturate(InverseLerp(pattern,0, (_Health) * 0.6)));
                o.Albedo = finalColor.rgb;
                // Metallic and smoothness come from slider variables
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = finalColor.a;
            }
            ENDCG
        }
            FallBack "Diffuse"
}