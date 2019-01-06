Shader "Hidden/Deformable Surface/Surface Paint"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_brush("BaseMap", 2D) = "white" {}
		_baseMap ("BaseMap", 2D) = "white" {}
		_hardness ("Hardness", Float) = 1
		_mode("Paint Mode", Int) = 0
	}

	SubShader
	{
		Pass
		{			
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_img
			#pragma fragment frag

			sampler2D _MainTex;
			sampler2D _brush;
			sampler2D _baseMap;
			uniform float _hardness;
			uniform int _mode;
			
			fixed4 frag(v2f_img IN) : COLOR
			{				
				fixed4 base = tex2D(_baseMap, IN.uv);
				fixed4 brush = tex2D(_brush, IN.uv);

				if (_mode == 0) 
					base.r = base.r + (brush.r *  _hardness);
				else 
					base.g = base.g + (brush.r * _hardness);

				return base;
			}
			ENDCG
		}
	}
}
