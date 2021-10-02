Shader "Pine/ToonShading"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color ("Main Color", Color) = (1,1,1,1)
        _GradientTex("Gradient Texture", 2D) = "white" {}
        [HDR] _AmbientColor ("Ambient Color", Color) = (0.4, 0.4, 0.4, 1)
        [HDR] _SpecularColor ("Specular Color", Color) = (0.9, 0.9, 0.9, 1)
        _Glossiness ("Glossiness", Range(0, 100)) = 32
        [HDR] _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimAmount ("Rim Amount", Range(0, 1)) = 0.716
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags 
        {
            "RenderType"="Opaque"
            "LightMode" = "UniversalForward" 
            "PassFlags" = "OnlyDirectional"
            "RenderPipeline" = "UniversalPipeline"
        } 

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           	#pragma multi_compile_fwdbase
            #pragma multi_compile _MAIN_LIGHT_SHADOWS


            //#include "UnityCG.cginc"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : NORMAL;
                float3 viewDir : TEXCOORD1;
                float4 shadowCoord : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _GradientTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _AmbientColor;
            float _Glossiness;
            float4 _SpecularColor;
            float4 _RimColor;
            float _RimAmount;
            float _RimThreshold;

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.viewDir = GetWorldSpaceViewDir(v.vertex.xyz);
                float3 worldPos = GetVertexPositionInputs(v.vertex.xyz).positionWS; // world space position of this vertex
                o.shadowCoord = TransformWorldToShadowCoord(worldPos);
                return o;
            }


            float4 frag (Interpolators i) : SV_Target
            {
                /* Get lighting information */
                Light mainLight = GetMainLight(i.shadowCoord); // get main lighting info (including shadows)
                float shadow = mainLight.shadowAttenuation;
                float3 N = normalize(i.worldNormal); // worldspace normal
                float3 L = mainLight.direction; // mainlight direction (normalized)
                float4 mainLightColor = float4(mainLight.color, 1);
                float3 viewDir = normalize(i.viewDir);
                

                /* Diffuse */
                float NDotL = max(0, dot(N, L)); // Normal • Light direction
                float lightIntensity = smoothstep(0, 0.1, NDotL * shadow);
                //float lightIntensity = tex2D(_GradientTex, NDotL * shadow).r;
                float4 light = lightIntensity * mainLightColor;

                /* Specular */
                float3 halfVector = normalize(L + viewDir);
                float NDotH = max(0, dot(N, halfVector));
                float specularIntensity = pow(NDotH * lightIntensity, _Glossiness * _Glossiness); // <- multiply glossiness by itself here just to make inspector values nicer to work with
                float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
                float4 specular = specularIntensitySmooth * _SpecularColor;

                /* Rim Lighting (Fresnel) */
                float rimDot = 1 - dot(viewDir, N);
                float rimIntensity = rimDot * pow(NDotL, _RimThreshold); // only apply fresnel where object is already illuminated
                rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
                float4 rim = rimIntensity * _RimColor;

                float4 maintex = tex2D(_MainTex, i.uv);
                return _Color * maintex * (light + _AmbientColor + specular + rim);
            }
            ENDHLSL
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

    }
}
