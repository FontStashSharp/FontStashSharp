### Overview
By default, FontStashSharp uses [StbTrueTypeSharp](https://github.com/StbSharp/StbTrueTypeSharp) for the font loading & rasterization.

Additional rasterizers are available in separate assemblies:
- [FontStashSharp.Rasterizers.FreeType](https://www.nuget.org/packages/FontStashSharp.Rasterizers.FreeType/)
- [FontStashSharp.Rasterizers.SharpAstro](https://www.nuget.org/packages/FontStashSharp.Rasterizers.SharpAstro/)

### Available Rasterizers
- **StbTrueTypeSharp** (default) - fully managed port of stb_truetype. Requires no native dependencies and works everywhere .NET does. Does not perform TrueType hinting.
- **FreeType** - wrapper over the [FreeType](https://freetype.org/) library through [FreeTypeSharp](https://github.com/ryancheung/FreeTypeSharp) native bindings. Produces hinted output.
- **SharpAstro** - based on [SharpAstro.Fonts](https://www.nuget.org/packages/SharpAstro.Fonts/), a pure-managed MIT licensed OpenType/TrueType font loader & rasterizer. Fully managed, AOT compatible, requires no native dependencies. Targets .NET 10 or higher.

### Using FontStashSharp.Rasterizers.FreeType
1. Add reference to [FontStashSharp.Rasterizers.FreeType](https://www.nuget.org/packages/FontStashSharp.Rasterizers.FreeType/)
2. Add following code before the creation of FontSystems:
```c#
FontSystemDefaults.FontLoader = new FreeTypeLoader();
```

### Using FontStashSharp.Rasterizers.SharpAstro
1. Add reference to [FontStashSharp.Rasterizers.SharpAstro](https://www.nuget.org/packages/FontStashSharp.Rasterizers.SharpAstro/)
2. Add following code before the creation of FontSystems:
```c#
FontSystemDefaults.FontLoader = new SharpAstroLoader();
```

### Using Custom Font Rasterizers
It's possible to use custom rasterizer instead by implementing [IFontLoader](https://github.com/FontStashSharp/FontStashSharp.Base/blob/main/src/FontStashSharp.Base/IFontLoader.cs) interface.

The implementation should be passed to the FontSystemDefaults before the creation of FontSystems:
```c#
FontSystemDefaults.FontLoader = new MyFontLoader();
```

The [FontStashSharp.Samples.CustomRasterizers sample](https://github.com/rds1983/FontStashSharp/tree/main/samples/FontStashSharp.Samples.CustomRasterizers) demonstrates usage of 2 font rasterizers: StbTrueTypeSharp(default) and FreeType.

![alt text](~/images/custom-font-rasterizer.png)
