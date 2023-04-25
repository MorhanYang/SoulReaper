Shader "Custom/SpritePixelate" {
    Properties {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Range(1, 50)) = 10
    }
 
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100
 
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float _PixelSize;
 
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target {
                float2 pixelUV = floor(i.uv * _PixelSize) / _PixelSize;
                fixed4 color = tex2D(_MainTex, pixelUV);
                return color;
            }
            ENDCG
        }
    }
 
    FallBack "Diffuse"
    CustomEditor "UnityEditor.ShaderGraph.SurfaceShaderEditor"
    Dependency "AddPassShader" = "Hidden/Custom/AddPass"
    Dependency "SampleTexture2D" = "Hidden/Custom/SampleTexture2D"
 
    CGINCLUDE
    #include "UnityCG.cginc"
 
    sampler2D _CameraDepthTexture;
    float4 _MainTex_ST;
 
    half4 fragCustom (appdata_full v, sampler2D tex) : SV_Target
    {
        half4 color = tex2D(tex, TRANSFORM_TEX(v.uv, _MainTex));
        float2 pixelUV = floor(v.uv * _PixelSize) / _PixelSize;
        return tex2D(tex, pixelUV);
    }
    ENDCG
 
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100
 
        Pass {
            Name "FORWARD"
            Tags {"LightMode"="ForwardBase"}
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
 
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
 
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
