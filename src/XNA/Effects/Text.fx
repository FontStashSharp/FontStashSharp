#include "Macros.fxh"

#ifdef EFFECTSHADOW
	uniform float2 cShadowOffset;
	uniform float4 cShadowColor;
#endif

#ifdef EFFECTSTROKE
	uniform float4 cStrokeColor;
#endif

DECLARE_TEXTURECUBE_LINEAR_CLAMP(SpriteTexture);

float GetAlpha(float distance, float width)
{
	return smoothstep(0.5 - width, 0.5 + width, distance);
}

void PS(float2 iTexCoord : TEXCOORD0,
    float4 iColor : COLOR0,
    out float4 oColor : OUTCOLOR0)
{
	oColor.rgb = iColor.rgb;
	float distance = Sample2D(SpriteTexture, iTexCoord).a;

	#ifdef EFFECTSTROKE
		#ifdef SUPERSAMPLING
			float outlineFactor = smoothstep(0.5, 0.525, distance); // Border of glyph
			oColor.rgb = lerp(cStrokeColor.rgb, iColor.rgb, outlineFactor);
		#else
			if (distance < 0.525)
				oColor.rgb = cStrokeColor.rgb;
		#endif
	#endif

	#ifdef EFFECTSHADOW
	if (Sample2D(SpriteTexture, iTexCoord - cShadowOffset).a > 0.5 && distance <= 0.5)
		oColor = cShadowColor;
	#ifndef SUPERSAMPLING
	else if (distance <= 0.5)
		oColor.a = float4(0, 0, 0, 0);
	#endif
	else
	#endif
	{
		float width = fwidth(distance);
		float alpha = GetAlpha(distance, width);

		#ifdef SUPERSAMPLING
			float2 deltaUV = 0.354 * fwidth(iTexCoord); // (1.0 / sqrt(2.0)) / 2.0 = 0.354
			float4 square = float4(iTexCoord - deltaUV, iTexCoord + deltaUV);

			float distance2 = Sample2D(SpriteTexture, square.xy).a;
			float distance3 = Sample2D(SpriteTexture, square.zw).a;
			float distance4 = Sample2D(SpriteTexture, square.xw).a;
			float distance5 = Sample2D(SpriteTexture, square.zy).a;

			alpha += GetAlpha(distance2, width)
				   + GetAlpha(distance3, width)
				   + GetAlpha(distance4, width)
				   + GetAlpha(distance5, width);
		
			// For calculating of average correct would be dividing by 5.
			// But when text is blurred, its brightness is lost. Therefore divide by 4.
			alpha = alpha * 0.25;
		#endif

		oColor.a = alpha;
	}
}

technique Default
{
	pass P0
	{
		PixelShader = compile ps_3_0 PS();
	}
};