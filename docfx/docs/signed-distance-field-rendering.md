### Purpose

Signed Distance Field (SDF) is a technique for rendering fonts at a consistent quality regardless of scale.

With the default (sprite) rendering path, every glyph is rasterized once into the texture atlas as a bitmap that stores the *coverage* of the glyph outline (the alpha value of every pixel). When such a bitmap is drawn at a size other than the one it was rasterized for, the edges are simply stretched by the GPU and quickly become blurry or pixelated.

SDF rendering changes what is stored in the atlas. Instead of per-pixel alpha coverage, each texel stores the **signed distance** to the glyph outline — a value that is negative inside the glyph, positive outside it, and close to zero near the outline. At draw time, a fragment shader converts this distance back into alpha (by thresholding it against the pixel's location relative to the outline). Because the inside/outside transition is recomputed per-pixel as the text is scaled, the edges of the glyph stay crisp at any scale, and effects such as shadows are computed from the same distance data.

### Enabling SDF Rasterization

SDF is enabled per `FontSystem` by setting `FontSystemSettings.FontRasterizationMode` to `FontRasterizationMode.SDF`:

```c#
var settings = new FontSystemSettings
{
	FontRasterizationMode = FontRasterizationMode.SDF
};
var fontSystem = new FontSystem(settings);
```

### Enabling SDF For All FontSystems

To enable SDF for every `FontSystem` created afterwards, set the default once before creating any `FontSystem`:

```c#
FontSystemDefaults.FontRasterizationMode = FontRasterizationMode.SDF;
```

### Rendering With SDFTextBatch

Because the atlas now contains distance data instead of colors, the regular `SpriteBatch.DrawString` extension methods would render it incorrectly. The `SDFTextBatch` class (available in the MonoGame and FNA packages) applies the SDF effect and draws the text:

```c#
// Create once (typically in LoadContent)
_sdfTextBatch = new SDFTextBatch(GraphicsDevice);

// Create a font from the SDF-enabled FontSystem
SpriteFontBase font = _fontSystem.GetFont(64);

// Draw as many strings as needed between Begin/End
_sdfTextBatch.Begin();
_sdfTextBatch.DrawString(font, "Hello, SDF!", new Vector2(10, 10), Color.White);
_sdfTextBatch.DrawString(font, "Scaled up", new Vector2(10, 80), Color.Yellow, scale: new Vector2(4.0f));
_sdfTextBatch.End();
```

The `scale` parameter of `DrawString` is the primary way to resize SDF text — even large scale factors keep the edges sharp. Call `Dispose()` when the batch is no longer needed.

### Font Effects (Shadow & Stroke)

Use `SetShadowEffect` or `SetStrokeEffect` after `Begin()` and before `DrawString` to select the effect for everything drawn until the next effect change.

```c#
_sdfTextBatch.Begin();

// Plain text (or switch back from an effect)
_sdfTextBatch.ResetEffect();
_sdfTextBatch.DrawString(font, "No effect", new Vector2(10, 10), Color.White);

// Shadow: casts a colored shadow offset by the given distance (in pixels)
_sdfTextBatch.SetShadowEffect(Color.Black, new Vector2(2, 2));
_sdfTextBatch.DrawString(font, "Drop shadow", new Vector2(10, 60), Color.White);

// Outline (stroke): draws a colored outline around the glyphs.
// thickness and smoothness are expressed in normalized SDF-space units
// (typical values are around 0.5 and 0.025 respectively).
_sdfTextBatch.SetStrokeEffect(Color.Black, 0.5f, 0.025f);
_sdfTextBatch.DrawString(font, "Outlined", new Vector2(10, 110), Color.White);

_sdfTextBatch.End();
```

### Sample

The [FontStashSharp.Samples.SDF](https://github.com/FontStashSharp/FontStashSharp/tree/main/samples/FontStashSharp.Samples.SDF) sample renders the same text side by side using SDF and a super-sampled standard `FontSystem`, and lets you resize both live to compare the quality difference. The **top** text is rendered with SDF, the **bottom** text with standard rasterization:

![alt text](~/images/sdf.png)