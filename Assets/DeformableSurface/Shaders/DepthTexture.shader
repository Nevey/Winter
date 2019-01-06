Shader "Hidden/Deformable Surface/Depth Texture"
{    
    SubShader
    {
		Pass
        {
			Lighting Off Fog { Mode Off }

            CGPROGRAM
			#pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            uniform sampler2D _LastCameraDepthTexture;
             
			fixed4 frag(v2f_img IN) : COLOR
            {
				float depth = UNITY_SAMPLE_DEPTH(tex2D(_LastCameraDepthTexture, IN.uv));				
				return fixed4(1 - depth, ceil(depth), 1, 1);
            }            
            ENDCG
        }
    } 
}