Shader "Custom/FoamWater" {
    Properties {
        _MainTex ("Water Texture", 2D) = "white" {}
        _FoamTex ("Foam Texture", 2D) = "white" {}
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamThreshold ("Foam Threshold", Range(0, 1)) = 0.5
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
            sampler2D _FoamTex;
            float4 _FoamColor;
            float _FoamThreshold;
 
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target {
                // Sample the water texture
                float2 uv = i.uv;
                fixed4 water = tex2D(_MainTex, uv);
 
                // Sample the foam texture
                fixed4 foam = tex2D(_FoamTex, uv);
 
                // Determine if this pixel should be a foam pixel or a water pixel
                float foamValue = foam.r;
                float threshold = _FoamThreshold;
                float isFoam = step(foamValue, threshold);
 
                // Apply the foam color to the foam pixels
                fixed4 color = lerp(water, _FoamColor, isFoam);
 
                // Add some extra white pixels along the edges of the foam
                float edgeThreshold = 0.5;
                float edgeValue = max(max(max(foam.r, foam.g), foam.b), foam.a);
                float isEdge = step(edgeValue, edgeThreshold);
                color.rgb += isEdge * _FoamColor.rgb;
 
                // Return the final output color
                return color;
            }
            ENDCG
        }
    }
}