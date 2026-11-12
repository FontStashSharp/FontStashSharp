### Overview
By default, FontStashSharp uses [StbTrueTypeSharp](https://github.com/StbSharp/StbTrueTypeSharp) for the font loading & rasterization.

Additional rasterizers are available in separate assemblies:
- [FontStashSharp.Rasterizers.StbTrueTypeNext](https://www.nuget.org/packages/FontStashSharp.Rasterizers.StbTrueTypeNext/)
- [FontStashSharp.Rasterizers.FreeType](https://www.nuget.org/packages/FontStashSharp.Rasterizers.FreeType/)
- [FontStashSharp.Rasterizers.SharpAstro](https://www.nuget.org/packages/FontStashSharp.Rasterizers.SharpAstro/)

### Available Rasterizers
- **StbTrueTypeSharp** (default) - fully managed port of stb_truetype. Requires no native dependencies and works everywhere .NET does. Does not perform TrueType hinting.
- **StbTrueTypeNext** - based on [StbTrueTypeNext](https://github.com/StbSharp/StbTrueTypeNext), a safe, refactored fork of StbTrueTypeSharp. Fully managed, requires no native dependencies and works everywhere .NET does. Renders with a newer rasterizer implementation by default, while the legacy one remains selectable through `UseOldRasterizer`. Also loads CFF/OpenType and TTC fonts.
- **FreeType** - wrapper over the [FreeType](https://freetype.org/) library through [FreeTypeSharp](https://github.com/ryancheung/FreeTypeSharp) native bindings. Produces hinted output.
- **SharpAstro** - based on [SharpAstro.Fonts](https://www.nuget.org/packages/SharpAstro.Fonts/), a pure-managed MIT licensed OpenType/TrueType font loader & rasterizer. Fully managed, AOT compatible, requires no native dependencies. Targets .NET 10 or higher.

### Using FontStashSharp.Rasterizers.StbTrueTypeNext
1. Add reference to [FontStashSharp.Rasterizers.StbTrueTypeNext](https://www.nuget.org/packages/FontStashSharp.Rasterizers.StbTrueTypeNext/)
2. Add following code before the creation of FontSystems:
```c#
FontSystemDefaults.FontLoader = new StbTrueTypeNextLoader();
```

The loader behavior can be tuned through `StbTrueTypeNextSettings`:
```c#
FontSystemDefaults.FontLoader = new StbTrueTypeNextLoader(new StbTrueTypeNextSettings
{
	KernelWidth = 2,
	KernelHeight = 2,
	UseOldRasterizer = false
});
```

`UseOldRasterizer = true` selects the older rasterizer implementation; leave it `false` to use the current one.

Note that `FontSystemSettings.StbTrueTypeUseOldRasterizer` only affects the default loader. When a custom loader is supplied, configure `UseOldRasterizer` on its own settings instead.

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

The [FontStashSharp.Samples.CustomRasterizers sample](https://github.com/rds1983/FontStashSharp/tree/main/samples/FontStashSharp.Samples.CustomRasterizers) demonstrates usage of several font rasterizers: StbTrueTypeSharp (default), SharpAstro and FreeType.

![alt text](~/images/custom-font-rasterizer.png)
