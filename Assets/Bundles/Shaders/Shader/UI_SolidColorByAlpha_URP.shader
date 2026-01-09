Shader "UI/URP SolidColor By Alpha (Maskable+Outline)"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Color   ("Fill Color", Color) = (1,1,1,1)

        // ==== Outline Settings ====
        [Toggle] _UseOuterOutline ("Enable Outer Outline", Float) = 0
        _OuterColor ("Outer Outline Color", Color) = (0,0,0,1)
        _OuterSize  ("Outer Outline Size (px)", Range(0,8)) = 2

        [Toggle] _UseInnerOutline ("Enable Inner Outline", Float) = 0
        _InnerColor ("Inner Outline Color", Color) = (0,0,0,1)
        _InnerSize  ("Inner Outline Size (px)", Range(0,8)) = 2

        // —— 以下是 UGUI Mask/RectMask2D 需要的标准属性 ——
        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID", Float) = 0
        _StencilOp        ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask", Float) = 255
        _ColorMask        ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalRenderPipeline"
        }

        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize; // (1/w, 1/h, w, h)

            fixed4 _Color;

            float _UseOuterOutline;
            fixed4 _OuterColor;
            float _OuterSize;

            float _UseInnerOutline;
            fixed4 _InnerColor;
            float _InnerSize;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                fixed4 color    : COLOR;
                float4 worldPos : TEXCOORD1; // for RectMask2D clip
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.uv       = TRANSFORM_TEX(v.uv, _MainTex);
                o.color    = v.color; // 与 _Color 在片元里再相乘，便于分别控制
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            inline fixed sampleAlpha(float2 uv)
            {
                return tex2D(_MainTex, uv).a;
            }

            // 近邻取样（十字 + 对角）做一次性膨胀/腐蚀近似
            inline void getNeighborUVs(float2 uv, float radiusPx, out float2 uvN[8])
            {
                float2 offs = radiusPx * _MainTex_TexelSize.xy;
                uvN[0] = uv + float2( offs.x,  0); // R
                uvN[1] = uv + float2(-offs.x,  0); // L
                uvN[2] = uv + float2( 0,  offs.y); // U
                uvN[3] = uv + float2( 0, -offs.y); // D

                // 斜向略小一点，让边缘更圆滑
                float2 offsDiag = offs * 0.7071;
                uvN[4] = uv + float2( offsDiag.x,  offsDiag.y); // RU
                uvN[5] = uv + float2(-offsDiag.x,  offsDiag.y); // LU
                uvN[6] = uv + float2( offsDiag.x, -offsDiag.y); // RD
                uvN[7] = uv + float2(-offsDiag.x, -offsDiag.y); // LD
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 基础：原图 alpha（形状）
                fixed a0 = sampleAlpha(i.uv);

                // ===== 计算外描边区域：dilate =====
                fixed aOuter = 0;
                if (_UseOuterOutline > 0.5 && _OuterSize > 0.001)
                {
                    float2 uvs[8];
                    getNeighborUVs(i.uv, _OuterSize, uvs);
                    aOuter = a0;
                    [unroll] for (int k=0; k<8; k++)
                        aOuter = max(aOuter, sampleAlpha(uvs[k]));
                    // 外描边环 = 膨胀后 - 原alpha
                    aOuter = saturate(aOuter - a0);
                }

                // ===== 计算内描边区域：erode =====
                fixed aInner = 0;
                if (_UseInnerOutline > 0.5 && _InnerSize > 0.001)
                {
                    float2 uvs2[8];
                    getNeighborUVs(i.uv, _InnerSize, uvs2);
                    fixed aErode = a0;
                    [unroll] for (int k=0; k<8; k++)
                        aErode = min(aErode, sampleAlpha(uvs2[k]));
                    // 内描边环 = 原alpha - 腐蚀后
                    aInner = saturate(a0 - aErode);
                }

                // ===== 组合颜色 =====
                // UGUI Tint 与自定义颜色叠乘（RGB）；透明度统一乘 i.color.a
                fixed4 fillCol  = fixed4((_Color.rgb * i.color.rgb), 1.0);
                fixed4 outerCol = fixed4((_OuterColor.rgb * i.color.rgb), 1.0);
                fixed4 innerCol = fixed4((_InnerColor.rgb * i.color.rgb), 1.0);

                // 优先级：内描边 > 外描边 > 填充
                fixed3 rgb = fillCol.rgb;
                fixed  a   = a0;

                // 外描边覆盖仅在“描边环区域”显示
                if (_UseOuterOutline > 0.5)
                {
                    rgb = lerp(rgb, outerCol.rgb, aOuter);
                    a   = max(a, aOuter);
                }

                // 内描边覆盖仅在“内环区域”显示
                if (_UseInnerOutline > 0.5)
                {
                    rgb = lerp(rgb, innerCol.rgb, aInner);
                    a   = max(a, aInner);
                }

                // 乘上 UGUI 的顶点色透明度
                fixed4 col = fixed4(rgb, a * i.color.a);

                #ifdef UNITY_UI_CLIP_RECT
                    col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(col.a - 0.001);
                #endif
                return col;
            }
            ENDCG
        }
    }
}
