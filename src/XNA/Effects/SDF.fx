// This shader was borrowed from https://u3d.io
#include "Macros.fxh"

#ifdef EFFECTSHADOW
	uniform float2 cShadowOffset;
	uniform float4 cShadowColor;
#endif

#ifdef EFFECTSTROKE
	uniform float4 cStrokeColor;
	uniform float cStrokeThickness;
	uniform float cStrokeSmoothness;
#endif

#ifdef EFFECTGLOW
	uniform float4 cGlowColor;
	uniform float cGlowRange;
	uniform float cGlowSmoothness;
#endif

DECLARE_TEXTURE(SpriteTexture, 0);

struct VSOutput
{
	float4 position		: SV_Position;
	float4 color		: COLOR0;
	float2 texCoord		: TEXCOORD0;
};

float GetAlpha(float distance, float width)
{
	return smoothstep(0.5 - width, 0.5 + width, distance);
}

float4 PS(VSOutput input) : SV_Target0
{
	float4 oColor;
	oColor.rgb = input.color.rgb;
	float distance = SAMPLE_TEXTURE(SpriteTexture, input.texCoord).a;

	#ifdef EFFECTSTROKE
		float outlineFactor = smoothstep(cStrokeThickness, cStrokeThickness + cStrokeSmoothness, distance);
		oColor.rgb = lerp(cStrokeColor.rgb, input.color.rgb, outlineFactor);
	#endif

	#ifdef EFFECTSHADOW
	if (SAMPLE_TEXTURE(SpriteTexture, input.texCoord - cShadowOffset).a > 0.5 && distance <= 0.5)
		oColor = cShadowColor;
	#ifndef SUPERSAMPLING
	else if (distance <= 0.5)
		oColor.a = 0.0;
	#endif
	else
	#endif
	{
		float width = fwidth(distance);
		float alpha = GetAlpha(distance, width);

		#ifdef SUPERSAMPLING
			float2 deltaUV = 0.354 * fwidth(input.texCoord); // (1.0 / sqrt(2.0)) / 2.0 = 0.354
			float4 square = float4(input.texCoord - deltaUV, input.texCoord + deltaUV);

			float distance2 = SAMPLE_TEXTURE(SpriteTexture, square.xy).a;
			float distance3 = SAMPLE_TEXTURE(SpriteTexture, square.zw).a;
			float distance4 = SAMPLE_TEXTURE(SpriteTexture, square.xw).a;
			float distance5 = SAMPLE_TEXTURE(SpriteTexture, square.zy).a;

			alpha += GetAlpha(distance2, width)
				   + GetAlpha(distance3, width)
				   + GetAlpha(distance4, width)
				   + GetAlpha(distance5, width);
		
			alpha = alpha * 0.25;
		#endif

		oColor.a = alpha;
	}

	#ifdef EFFECTGLOW
		float glowFactor = smoothstep(0.5 - cGlowRange, 0.5 + cGlowSmoothness, distance);
		oColor.rgb = lerp(cGlowColor.rgb, oColor.rgb, saturate(oColor.a));
		oColor.a = max(oColor.a, glowFactor * cGlowColor.a);
	#endif

	return oColor;
}

TECHNIQUE(Default, PS);