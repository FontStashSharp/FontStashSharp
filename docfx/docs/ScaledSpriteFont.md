## ScaledSpriteFont

`ScaledSpriteFont` is a `SpriteFontBase` that renders another font ("base font") at a different scale. It does not rasterize any glyphs itself — it simply wraps an existing font and scales all drawing operations by a fixed `Scale` factor.

## How it works

When created, `ScaledSpriteFont` wraps a `baseFont` and a `scale`:

```c#
var scaledFont = new ScaledSpriteFont(baseFont, scale);
```

All text rendering operations delegate to the base font, adjusting parameters so the visual result matches the requested size:

- **Scale** — the overall scale is multiplied by `Scale` when drawing glyphs.
- **Origin, character spacing, and line spacing** — divided by `Scale` (i.e. multiplied by the inverse scale) before being passed to the base font, so the measured properties stay consistent with the displayed size.
- **Kerning** — the base font's kerning is multiplied by `Scale`.
- **Text bounds** — computed from the base font at the inverse scale, then scaled up by `Scale`.

Because the wrapped font is cached, creating a `ScaledSpriteFont` around the same base font with the same size returns the same instance on subsequent requests.