Shader "Custom/InstancedIndirectColor" {
    SubShader {
        Tags { "RenderType" = "Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
            }; 

            struct MeshProperties {
                float4x4 mat;
                float4 color;
            };

            StructuredBuffer<MeshProperties> _Properties;

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) { // 각 mesh의 index. 
                v2f o;

                float4 pos = mul(_Properties[instanceID].mat, i.vertex); // 각 mesh 별로 properties 가 넘어감. 
                o.vertex = UnityObjectToClipPos(pos); 
                o.color = _Properties[instanceID].color;
                // 모델은 하나. 1000개의 매트릭스, 칼라가 넘어간다. 
                // low level 에서 하던 방식을 추가해서 퍼포먼스를 높히는 것임. 
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                return i.color;
            }
            
            ENDCG
        }
    }
}