Shader "Custom/CartoonWater" {
    Properties {
        _Color ("Water Color", Color) = (1, 1, 1, 1)
        _MainTex ("Water Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 1
        _WaveStrength ("Wave Strength", Range(0, 1)) = 0.5
    }
 
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Opaque" }
 
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
            float4 _Color;
            float _WaveSpeed;
            float _WaveStrength;
 
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target {
                float2 uv = i.uv;
 
                // Add some simple wave distortion to the UV coordinates
                uv += _Time.y * _WaveSpeed;
                uv.y += sin(uv.x * 10 + _Time.y * _WaveSpeed) * _WaveStrength;
                uv.x += sin(uv.y * 10 + _Time.y * _WaveSpeed) * _WaveStrength;
 
                // Sample the texture using the distorted UV coordinates
                fixed4 tex = tex2D(_MainTex, uv);
 
                // Apply the color and return the final output color
                return tex * _Color;
            }
            ENDCG
        }
    }
}