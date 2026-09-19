// This shader was borrowed from https://u3d.io
// Stroke code was borrowed from https://github.com/suikki/sdf_text_sample

// The SDF distance is stored in the atlas ALPHA channel; 0.5 is the glyph outline.
// Selected variants are enabled with #defines (SUPERSAMPLING/EFFECTSHADOW/EFFECTSTROKE).
#ifdef GL_ES
	#define LOWP lowp
	precision mediump float;
#else
	#define LOWP
#endif

// Uniforms
uniform sampler2D TextureSampler;

#ifdef EFFECTSHADOW
	// Shadow displacement in normalized texture space
	uniform vec2 cShadowOffset;
	uniform vec4 cShadowColor;
#endif

#ifdef EFFECTSTROKE
	uniform vec4 cStrokeColor;
	uniform float cStrokeThickness;
#endif

// Varyings
varying vec4 v_color;
varying vec2 v_texCoords;

// Converts an SDF distance to coverage: smoothstep around the 0.5 cutoff by the edge width.
float GetAlpha(float distance, float width)
{
	return smoothstep(0.5 - width, 0.5 + width, distance);
}

void main()
{
	vec4 oColor;
	oColor.rgb = v_color.rgb;
	float distance = texture2D(TextureSampler, v_texCoords).a;

	// fwidth gives the pixel-space rate of change of the distance (anti-aliasing width).
	float width = fwidth(distance);
	float alpha = GetAlpha(distance, width);

	#ifdef SUPERSAMPLING
		// Simpler AA: average the coverage of 4 extra samples around the pixel in order to fix "holes".
		vec2 deltaUV = 0.354 * fwidth(v_texCoords);
		vec4 square = vec4(v_texCoords - deltaUV, v_texCoords + deltaUV);

		float distance2 = texture2D(TextureSampler, square.xy).a;
		float distance3 = texture2D(TextureSampler, square.zw).a;
		float distance4 = texture2D(TextureSampler, square.xw).a;
		float distance5 = texture2D(TextureSampler, square.zy).a;

		alpha += GetAlpha(distance2, width)
			   + GetAlpha(distance3, width)
			   + GetAlpha(distance4, width)
			   + GetAlpha(distance5, width);

		// Average of the 5 samples (mirrors the original SDF.fx).
		alpha = alpha * 0.25;
	#endif

	#ifdef EFFECTSHADOW
		// A pixel outside the glyph whose offset sample is inside it becomes the shadow.
		float shadowDistance = texture2D(TextureSampler, v_texCoords - cShadowOffset).a;
		if (shadowDistance > 0.5 && distance <= 0.5)
			oColor = cShadowColor;
		else
		{
			// Outside pixels are transparent unless supersampling smoothed them.
			#ifdef SUPERSAMPLING
				oColor.a = alpha;
			#else
				oColor.a = (distance <= 0.5) ? 0.0 : alpha;
			#endif
		}
	#else
		oColor.a = alpha;
	#endif

	#ifdef EFFECTSTROKE
		// Outline band just outside the glyph contour; blend by glyph coverage and
		// extend the shape's alpha beyond the contour.
		float outlineEdge = 0.5 - cStrokeThickness;
		float outlineOuterAlpha = smoothstep(outlineEdge - width, outlineEdge + width, distance);
		oColor.rgb = mix(cStrokeColor.rgb, oColor.rgb, alpha);
		oColor.a = max(oColor.a, cStrokeColor.a * outlineOuterAlpha);
	#endif

	gl_FragColor = oColor;
}