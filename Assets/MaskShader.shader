Shader "Unlit/BloodStain"
{
    Properties
    {
        _MainTex("Main", 2D) = "white" {}
        [NoScaleOffset] _BloodStain("Blood Stain",2D) = "white" {}
        _I("Intensity",Range(0,1)) = 0
        _T("Time",Range(0.9,1.0011)) = 0
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent+2000" }

            Pass
            {
                ZWrite off
                Cull Back
                Blend SrcAlpha OneMinusSrcAlpha
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex: POSITION;
                    float2 uv : TEXCOORD0;
                };

                sampler2D _BloodStain;
                float _T;
                float _I;
                float4 _MainTex_ST;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }


                fixed4 frag(v2f i) : SV_Target
                {
                    // sample the texture
                    float4 col = tex2D(_BloodStain, i.uv);
                    col = col + float4(_I, _I, _I,0);
                    float t = _T;
                    if (t >= col.a)  return 0;

                    return col;
                }
                ENDCG
            }
        }
}
