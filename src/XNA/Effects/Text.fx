#include "Macros.fxh"

#ifdef EFFECTSHADOW
	uniform float2 cShadowOffset;
	uniform float4 cShadowColor;
#endif

#ifdef EFFECTSTROKE
	uniform float4 cStrokeColor;
#endif

DECLARE_TEXTURE(SpriteTexture, 0);

float GetAlpha(float distance, float width)
{
	return smoothstep(0.5 - width, 0.5 + width, distance);
}

float4 PS(float2 iTexCoord : TEXCOORD0, float4 iColor : COLOR0) : SV_Target0
{
	float4 oColor;
	oColor.rgb = iColor.rgb;
	float distance = SAMPLE_TEXTURE(SpriteTexture, iTexCoord).a;

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
	if (SAMPLE_TEXTURE(SpriteTexture, iTexCoord - cShadowOffset).a > 0.5 && distance <= 0.5)
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

			float distance2 = SAMPLE_TEXTURE(SpriteTexture, square.xy).a;
			float distance3 = SAMPLE_TEXTURE(SpriteTexture, square.zw).a;
			float distance4 = SAMPLE_TEXTURE(SpriteTexture, square.xw).a;
			float distance5 = SAMPLE_TEXTURE(SpriteTexture, square.zy).a;

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

	return oColor;
}

TECHNIQUE(Default, PS);
