## Text scaling issues

When a font is drawn at a different size than the resolution at which its glyphs were rasterized, it looks blurry or jagged. Supersampling addresses this by rasterizing each glyph at a higher resolution than the displayed size, then scaling it down via a [ScaledSpriteFont](ScaledSpriteFont.md) — blending source pixels smooths edges and preserves detail.

## Enabling supersampling in FSS

- **`FontResolutionFactor`** — the scale at which glyphs are rasterized relative to the requested size. Any non-null value (e.g. `2.0f`) enables supersampling.
- **`KernelWidth`** / **`KernelHeight`** — passed to `stbtt__h_prefilter`/`stbtt__v_prefilter`; a small blur pre-filter that helps reduce aliasing. The best results are typically obtained with all three set to 2. These only apply to the default (StbTrueTypeSharp) rasterizer.

## Setting it for a specific FontSystem or all systems

Set on `FontSystemDefaults` to apply to all font systems:

```c#
FontSystemDefaults.FontResolutionFactor = 2.0f;
FontSystemDefaults.KernelWidth = 2;
FontSystemDefaults.KernelHeight = 2;
```

Or set on `FontSystemSettings` for a specific system:

```c#
var settings = new FontSystemSettings();
settings.FontResolutionFactor = 2.0f;
settings.KernelWidth = 2;
settings.KernelHeight = 2;
var fontSystem = new FontSystem(settings);
```
