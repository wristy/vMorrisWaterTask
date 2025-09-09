Shader "Unlit/quicksha"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,0.5,0.2)
        _MainTex ("Albedo", 2D) = "white" {}
        _EmissionColor ("Emission", Color) = (1,1,0.5,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            fixed4 _EmissionColor;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color + _EmissionColor;
            }
            ENDCG
        }
    }
}
