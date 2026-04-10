Shader "Project Z/Outline"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _Degree ("Degree of Expansion", Float) = 1
        _StencilRef ("Stencil Reference", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }

        Stencil { Ref [_StencilRef] Comp GEqual }
        ZTest Always

        Pass
        {
            Name "OUTLINE"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                half4 color : COLOR;
                half3 normal : NORMAL;
            };

            struct v2f
            {
                half4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            half _Degree;

            v2f vert (appdata v)
            {
                v2f o;
                half4 vertex = UnityObjectToClipPos(v.vertex);
                half3 normalNDC = normalize(TransformViewToProjection(mul(UNITY_MATRIX_IT_MV, v.color.rgb).xyz)) * vertex.w;
                normalNDC.x *= _ScreenParams.y / _ScreenParams.x;
                vertex.xy += 0.01 * _Degree * normalNDC.xy;
                o.vertex = vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
