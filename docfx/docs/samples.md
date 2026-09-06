# FontStashSharp Samples

This page provides an overview of all available samples in the FontStashSharp repository, demonstrating various features and use cases of the library.

## MonoGame/FNA Samples

### DynamicSpriteFont
**Location:** `FontStashSharp.Samples.DynamicSpriteFont`

Demonstrates the `DynamicSpriteFont` class for runtime font rendering with dynamic glyph generation. Features include:
- Loading fonts at runtime from TTF files
- Applying font effects (blur, shadow, stroke)
- Color effects and animated text coloring
- Dynamic font scaling and size changes
- Text alignment options (left, center, right)
- Keyboard controls for toggling features

This is the primary sample for getting started with FontStashSharp in MonoGame or FNA projects.

### StaticSpriteFont
**Location:** `FontStashSharp.Samples.StaticSpriteFont`

Demonstrates the `StaticSpriteFont` class for pre-rendered font atlases. Features include:
- Loading fonts from pre-baked bitmap font files (BMFont format)
- Performance comparison with dynamic fonts
- Lower memory overhead for static character sets
- Ideal for fixed-size fonts in production games

### CustomRasterizers
**Location:** `FontStashSharp.Samples.CustomRasterizers`

Shows how to use different font rasterizers and compares their rendering quality. Features include:
- StbTrueType rasterizer (default)
- StbTrueType old rasterizer variant
- FreeType rasterizer (Windows-only)
- Side-by-side visual comparison
- Multiple font types (emoji, Japanese characters, etc.)

### DisableAntialiasing
**Location:** `FontStashSharp.Samples.DisableAntialiasing`

Demonstrates pixel-perfect font rendering by disabling antialiasing. Features include:
- Antialiased rendering (default)
- Non-antialiased rendering with `GlyphRenderResult.NoAntialiasing`
- Side-by-side comparison for visual quality assessment
- Useful for retro pixel-art style games

### MeasureString
**Location:** `FontStashSharp.Samples.MeasureString`

Demonstrates text measurement and layout features. Features include:
- Using `MeasureString` to get text bounds before drawing
- Different resolution factors (1x and 3x)
- Various font sizes and scales
- Visual bounding box display
- Measuring text with different scale values

### HarfBuzz
**Location:** `FontStashSharp.Samples.HarfBuzz`

Demonstrates advanced text shaping with HarfBuzz for complex scripts. Features include:
- English text rendering
- Complex scripts: Hindi (Devanagari), Arabic (with RTL support), Hebrew (RTL), Japanese
- Emoji support including flag sequences and zero-width joiner sequences
- Mixed script rendering with automatic script detection
- Combining accents and diacritics
- Ligature support (ff, fi, fl, etc.)
- Bidirectional text mixing (LTR and RTL)

### RichText
**Location:** `FontStashSharp.Samples.RichText`

Demonstrates the RichText formatting system for styled text. Features include:
- Inline color specifications using `[red]`, hex codes `[#00f0fa]`, etc.
- Font switching with dynamic font files and sizes using `/f[FontFile.ttf, size]`
- Vertical scaling effects with `/v[value]`
- Subscript and superscript support
- Text emphasis effects (bold `/eb`, size multiplier `/es`, emphasis direction)
- Embedded images with `/i[image.png]`
- Complex mixed formatting demonstrations

### RichText.SDF
**Location:** `FontStashSharp.Samples.RichText.SDF`

SDF variant of the RichText sample for signed distance field rendering. Features include:
- All the RichText features: colors, font switching, vertical offsets, and embedded images
- Shadow `/ds` and stroke `/dt` SDF effects, controlled through the text commands
- Customizable SDF effect defaults via `RichTextDefaults.SDFShadowColor`, `RichTextDefaults.SDFShadowOffset`, `RichTextDefaults.SDFStrokeColor`, `RichTextDefaults.SDFStrokeThickness`, and `RichTextDefaults.SDFStrokeSmoothness`
- Rendering rich text with `SDFTextBatch`

### RotatingText
**Location:** `FontStashSharp.Samples.RotatingText`

Demonstrates text rotation with various effects. Features include:
- Rotating text around a center point
- Font effects (blur, shadow, stroke)
- Animated scaling during rotation
- Effect intensity control
- Multiple text lines with different angles

### MonoGameBackend
**Location:** `FontStashSharp.Samples.MonoGameBackend`

Basic introductory sample showing FontStashSharp integration with MonoGame. Features include:
- Simple font loading and rendering
- Basic text display
- Foundation for more complex samples

### MonoGameBackendRotating
**Location:** `FontStashSharp.Samples.MonoGameBackendRotating`

Specialized variant of the MonoGame sample focusing on rotated text rendering. Features include:
- Text rotation with MonoGame backend
- Rotation angle manipulation
- Transform and matrix operations

### Performance
**Location:** `FontStashSharp.Samples.Performance`

Benchmarks and compares performance of different font rendering approaches. Features include:
- MonoGame native `SpriteFont` performance baseline
- `StaticSpriteFont` rendering speed
- `DynamicSpriteFont` rendering speed
- HarfBuzz-shaped text performance
- Real-time FPS counter and frame time measurements
- Rendering 1000 strings per frame for stress testing

### TextureAtlasFull
**Location:** `FontStashSharp.Samples.TextureAtlasFull`

Demonstrates handling texture atlas overflow and dynamic texture management. Features include:
- Texture atlas creation and management
- Handling overflow when atlas is full
- Dynamic texture resizing
- Multiple atlas pages
- Edge case handling for large character sets

## Silk.NET Samples

Silk.NET samples use OpenGL for cross-platform rendering without MonoGame/FNA dependencies.

### Silk.NET
**Location:** `FontStashSharp.Samples.Silk.NET`

Basic Silk.NET sample demonstrating FontStashSharp with OpenGL. Features include:
- Window creation with Silk.NET
- OpenGL context initialization
- Basic font loading and rendering
- Text rotation with animated angles
- Keyboard input handling (ESC to close)
- Support for multiple font types (ASCII, Japanese, emoji)

### Silk.NET.HarfBuzz
**Location:** `FontStashSharp.Samples.Silk.NET.HarfBuzz`

Advanced text shaping demonstration with Silk.NET and HarfBuzz. Features include:
- All complex script support from the MonoGame HarfBuzz sample
- English, Hindi, Arabic, Hebrew, and Japanese text
- Emoji rendering with sequences
- Bidirectional text handling
- Mixed script rendering with automatic detection
- Optimized for Silk.NET/OpenGL rendering pipeline

### Silk.NET.RichText
**Location:** `FontStashSharp.Samples.Silk.NET.RichText`

Rich text formatting demonstration with Silk.NET. Features include:
- Color formatting with color names and hex codes
- Font switching and sizing
- Images in text
- Vertical scaling and subscripts
- Text emphasis effects
- OpenGL-optimized rendering

### Silk.NET.TrippyGL
**Location:** `FontStashSharp.Samples.Silk.NET.TrippyGL`

Demonstrates integration with TrippyGL renderer. Features include:
- TrippyGL rendering backend
- Silk.NET window and input management
- Custom rendering interface implementation
- Advanced graphics pipeline integration

## Other Frameworks

### OpenTK
**Location:** `FontStashSharp.Samples.OpenTK`

Demonstrates FontStashSharp integration with OpenTK framework. Features include:
- OpenTK window creation and management
- OpenGL rendering with OpenTK
- Font loading and text rendering
- Keyboard and mouse input handling

## Platform-Specific Samples

### DynamicSpriteFont.Android
**Location:** `FontStashSharp.Samples.DynamicSpriteFont.Android`

Android-specific implementation of the DynamicSpriteFont sample. Features include:
- Android platform integration
- Touch input handling
- Mobile-optimized font rendering
- Screen size adaptation

## Feature Matrix

| Sample | Dynamic Font | Static Font | HarfBuzz | Rich Text | Effects | Rotation | MonoGame | FNA | Stride | Silk.NET | OpenTK |
|--------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| DynamicSpriteFont | ✓ | | | | ✓ | | ✓ | ✓ | ✓ | | |
| StaticSpriteFont | | ✓ | | | | | ✓ | ✓ | ✓ | | |
| CustomRasterizers | ✓ | | | | | | ✓ | | | | |
| DisableAntialiasing | ✓ | | | | | | ✓ | | | | |
| MeasureString | ✓ | | | | | | ✓ | | | | |
| HarfBuzz | ✓ | | ✓ | | | | ✓ | ✓ | ✓ | | |
| RichText | ✓ | | | ✓ | | | ✓ | ✓ | ✓ | | |
| RichText.SDF | ✓ | | | ✓ | | | ✓ | ✓ | | | |
| RotatingText | ✓ | | | | ✓ | ✓ | ✓ | ✓ | ✓ | | |
| MonoGameBackend | ✓ | | | | | | ✓ | | | | |
| MonoGameBackendRotating | ✓ | | | | | ✓ | ✓ | | | | |
| Performance | ✓ | ✓ | ✓ | | | | ✓ | | | | |
| TextureAtlasFull | ✓ | | | | | | ✓ | | | | |
| OpenTK | ✓ | | | | | | | | | | ✓ |
| Silk.NET | ✓ | | | | | | | | | ✓ | |
| Silk.NET.HarfBuzz | ✓ | | ✓ | | | | | | | ✓ | |
| Silk.NET.RichText | ✓ | | | ✓ | | | | | | ✓ | |
| Silk.NET.TrippyGL | ✓ | | | | | | | | | ✓ | |
| DynamicSpriteFont.Android | ✓ | | | | | | ✓ | | | | |

## Getting Started

1. **Beginners:** Start with the `DynamicSpriteFont` sample for MonoGame or `Silk.NET` for a framework-agnostic approach.

2. **Complex Scripts:** Use the `HarfBuzz` sample to learn about text shaping for international text support.

3. **Styling:** Explore the `RichText` sample to understand how to apply formatting to text.

4. **Performance:** Check the `Performance` sample if you need to render large amounts of text efficiently.

5. **Framework Integration:** Choose the sample matching your graphics framework (MonoGame, FNA, Stride, Silk.NET, or OpenTK).

## Building and Running Samples

Each sample is a complete project that can be built independently:

```bash
dotnet build samples/FontStashSharp.Samples.DynamicSpriteFont/FontStashSharp.Samples.DynamicSpriteFont.MonoGame.csproj
dotnet run --project samples/FontStashSharp.Samples.DynamicSpriteFont/FontStashSharp.Samples.DynamicSpriteFont.MonoGame.csproj
```

Most samples come in multiple framework variants (MonoGame, FNA, Stride, etc.). Select the appropriate project file for your target framework.
