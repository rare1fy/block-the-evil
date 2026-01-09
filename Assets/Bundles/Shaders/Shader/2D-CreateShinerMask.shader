Shader "2D/CreateShinerMask"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_OriTex("Ori Tex", 2D) = "white" {}
		_EdgeAlpha("Edge Alpha", float) = 0.99
		_BlurSize("BlurSize", float) = 1.0
	}

	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True"}

		Cull Off
		Lighting Off
		ZWrite Off

		Pass
		{
			Name "0"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 texcoord  : TEXCOORD0;
			};

			sampler2D _OriTex;
			float4 _OriTex_ST;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _OriTex);

				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 color = tex2D(_OriTex, IN.texcoord);

				return color;
			}
			ENDCG
		}

		Pass
		{
			Name "1"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 texcoord  : TEXCOORD0;
			};

			sampler2D _OriTex;
			float4 _OriTex_ST;
			float _EdgeAlpha;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _OriTex);

				return OUT;
			}

			float4 frag(v2f IN) : SV_Target
			{
				float4 color = tex2D(_OriTex, IN.texcoord);

				if (IN.texcoord.x < 0.001 || IN.texcoord.x > 0.999)
					color.a = 0;
				if (IN.texcoord.y < 0.001 || IN.texcoord.y > 0.999)
					color.a = 0;
				
				if (color.a > _EdgeAlpha)
					color = float4(0, 0, 0, 0);
				else
					color = float4(0, 0, 0, 1);
				
				return color;
			}
			ENDCG
		}

		Pass
		{
			Name "2"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0			
			
			#include "UnityCG.cginc"
		
				struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 uv[5]  : TEXCOORD0;
			};

			sampler2D _MainTex;
			float2 _MainTex_TexelSize;
			float _BlurSize;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				float2 uv = IN.uv;
				OUT.uv[0] = uv;
				OUT.uv[1] = uv + float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
				OUT.uv[2] = uv - float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
				OUT.uv[3] = uv + float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;
				OUT.uv[4] = uv - float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;

				return OUT;
			}

			float4 frag(v2f i) : SV_Target
			{
				float weight[3] = { 0.4026, 0.2442, 0.0545 };
				float4 color = tex2D(_MainTex, i.uv[0]) * weight[0];

				for (int it = 1; it < 3; it++)
				{
					color += tex2D(_MainTex, i.uv[it * 2 - 1]) * weight[it];
					color += tex2D(_MainTex, i.uv[it * 2]) * weight[it];
				}

				return color;
			}
			ENDCG
		}
		Pass
		{
			Name "3"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0			
			
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 uv[5]  : TEXCOORD0;
			};

			sampler2D _MainTex;
			float2 _MainTex_TexelSize;
			float _BlurSize;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				float2 uv = IN.uv;
				OUT.uv[0] = uv;
				OUT.uv[1] = uv + float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
				OUT.uv[2] = uv - float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
				OUT.uv[3] = uv + float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;
				OUT.uv[4] = uv - float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;

				return OUT;
			}

			float4 frag(v2f i) : SV_Target
			{
				float weight[3] = { 0.4026, 0.2442, 0.0545 };
				float4 color = tex2D(_MainTex, i.uv[0]) * weight[0];

				for (int it = 1; it < 3; it++)
				{
					color += tex2D(_MainTex, i.uv[it * 2 - 1]) * weight[it];
					color += tex2D(_MainTex, i.uv[it * 2]) * weight[it];
				}

				return color;
			}
			ENDCG
		}
		Pass
		{
			Name "4"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0			
			
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 uv[9]  : TEXCOORD0;
			};

			sampler2D _MainTex;
			float2 _MainTex_TexelSize;
			float _BlurSize;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				float2 uv = IN.uv;

				OUT.uv[0] = uv;
				OUT.uv[1] = uv + float2(_MainTex_TexelSize.x * _BlurSize, 0);
				OUT.uv[2] = uv + float2(-_MainTex_TexelSize.x * _BlurSize, 0);

				OUT.uv[3] = uv + float2(0, _MainTex_TexelSize.y * _BlurSize);
				OUT.uv[4] = uv + float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * _BlurSize;
				OUT.uv[5] = uv + float2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y) * _BlurSize;

				OUT.uv[6] = uv + float2(0, -_MainTex_TexelSize.y * _BlurSize) ;
				OUT.uv[7] = uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _BlurSize;
				OUT.uv[8] = uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _BlurSize;

				return OUT;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 color = tex2D(_MainTex, i.uv[0]) * 0.147761;
				color += tex2D(_MainTex, i.uv[1]) * 0.118318;
				color += tex2D(_MainTex, i.uv[2]) * 0.118318;

				color += tex2D(_MainTex, i.uv[3]) * 0.118318;
				color += tex2D(_MainTex, i.uv[4]) * 0.0947416;
				color += tex2D(_MainTex, i.uv[5]) * 0.0947416;

				color += tex2D(_MainTex, i.uv[6]) * 0.118318;
				color += tex2D(_MainTex, i.uv[7]) * 0.0947416;
				color += tex2D(_MainTex, i.uv[8]) * 0.0947416;

				return color;
			}
			ENDCG
		}
	}
}