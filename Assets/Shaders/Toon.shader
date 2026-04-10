Shader "Project Z/Toon"
{
    Properties
    {
        _StencilRef ("Stencil Reference", Int) = 0
        _SpecRange ("Specular Range", Range(0, 0.1)) = 0.01
        [HDR] _SpecCol ("Specular Color", Color) = (0.1, 0.1, 0.1, 1)
        _SpecSmooth ("Specular Smoothness", Range(0, 1)) = 0
        _RimRange ("Rim Light Range", Range(0, 1)) = 0.5
        [HDR] _RimCol ("Rim Light Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Tint ("Tint", Color) = (1, 1, 1, 1)
        _DarkMap ("Darkness Map", 2D) = "black" {}
        _DarkBright ("Darkness Brightness", Range(0, 1)) = 0
        _DarkPow ("Darkness Power", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Stencil { Ref [_StencilRef] Pass Replace }

        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma multi_compile_fwdbase
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
                half3 normal : NORMAL;
            };

            struct v2f
            {
                half4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
                half3 normalW : TEXCOORD1;
                half3 posW : TEXCOORD2;
                half3 lightDir : TEXCOORD3;
                half3 viewDir : TEXCOORD4;
                SHADOW_COORDS(5)
            };

            sampler2D _MainTex;
            sampler2D _DarkMap;
            half4 _MainTex_ST;
            half4 _DarkMap_ST;
            half4 _SpecCol;
            half4 _RimCol;
            half4 _Tint;
            half _SpecRange;
            half _SpecSmooth;
            half _RimRange;
            half _DarkBright;
            half _DarkPow;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normalW = UnityObjectToWorldNormal(v.normal);
                o.posW = mul(unity_ObjectToWorld, v.vertex);

                o.lightDir = WorldSpaceLightDir(v.vertex).xyz;
                o.viewDir = WorldSpaceViewDir(v.vertex).xyz;

                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half3 lightDir = normalize(i.lightDir);
                half3 viewDir = normalize(i.viewDir);
                half3 normal = normalize(i.normalW);

                fixed3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;

                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * albedo;

                fixed difDir = 0.5h * dot(normal, lightDir) + 0.5h;
                fixed3 diffuse = _LightColor0.rgb * albedo * saturate(pow(tex2D(_DarkMap, half2(difDir * _DarkMap_ST.x + _DarkMap_ST.z, 0)).r, _DarkPow) + _DarkBright);

                fixed specDir = dot(normal, normalize(lightDir + viewDir));
                fixed width = fwidth(specDir) * 2;
                specDir = lerp(step(0, specDir + _SpecRange - 1), lerp(0, 1, smoothstep(-width, width, specDir + _SpecRange - 1)), _SpecSmooth);
                fixed3 specular = _SpecCol.rgb * specDir * step(0.0001h, _SpecRange);

                fixed rimDir = pow(max(0.0001h, dot(normal, viewDir)), max(0.1h, normalize(_LightColor0).r) * _RimRange);

                UNITY_LIGHT_ATTENUATION(atten, i, i.posW);
                return fixed4(ambient + (lerp(_RimCol, diffuse, rimDir) + specular) * atten, 1);
            }
            ENDCG
        }

        Pass
        {
            Tags { "LightMode"="ForwardAdd" }
            Blend One One

            CGPROGRAM
            #pragma multi_compile_fwdadd_fullshadows
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
                half3 normal : NORMAL;
            };

            struct v2f
            {
                half4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
                half3 normalW : TEXCOORD1;
                half3 posW : TEXCOORD2;
                half3 lightDir : TEXCOORD3;
                half3 viewDir : TEXCOORD4;
                SHADOW_COORDS(5)
            };

            sampler2D _MainTex;
            sampler2D _DarkMap;
            half4 _MainTex_ST;
            half4 _DarkMap_ST;
            half4 _SpecCol;
            half4 _Tint;
            half _SpecRange;
            half _SpecSmooth;
            half _DarkBright;
            half _DarkPow;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normalW = UnityObjectToWorldNormal(v.normal);
                o.posW = mul(unity_ObjectToWorld, v.vertex);

                o.lightDir = WorldSpaceLightDir(v.vertex).xyz;
                o.viewDir = WorldSpaceViewDir(v.vertex).xyz;

                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half3 lightDir = normalize(i.lightDir);
                half3 viewDir = normalize(i.viewDir);
                fixed3 normal = normalize(i.normalW);

                fixed3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;

                fixed difDir = 0.5h * dot(normal, lightDir) + 0.5h;
                fixed3 diffuse = _LightColor0.rgb * albedo * saturate(pow(tex2D(_DarkMap, half2(difDir * _DarkMap_ST.x + _DarkMap_ST.z, 0)).r, _DarkPow) + _DarkBright);

                fixed specDir = dot(normal, normalize(lightDir + viewDir));
                fixed width = fwidth(specDir) * 2;
                specDir = lerp(step(0, specDir + _SpecRange - 1), lerp(0, 1, smoothstep(-width, width, specDir + _SpecRange - 1)), _SpecSmooth);
                fixed3 specular = _SpecCol.rgb * specDir * step(0.0001h, _SpecRange);

                UNITY_LIGHT_ATTENUATION(atten, i, i.posW);
                return fixed4((diffuse + specular) * atten, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
