`FontSystemDefaults` contains 3 parameters that could be used to make fonts shaper and better at scaling: `FontResolutionFactor`, `KernelWidth` and `KernelHeight`.

`FontResolutionFactor`(default value 1.0f) contains scale at which glyphs are rendered to the texture atlas. So setting it to i.e. 2.0f will make the font better at scaling. However such approach has a downside as well - font glyphs would occupy more space at the texture atlas and thus it'll be out of space faster.

`KernelWidth`/`KernelHeight`(default value is 0) are passed to stb_truetype methods `stbtt__h_prefilter`/`stbtt__v_prefilter`.

Empirically it's discovered that - setting all three properties `FontResolutionFactor`, `KernelWidth` and `KernelHeight` to 2 - would result in fonts becoming way better at scaling:
```c#
FontSystemDefaults.FontResolutionFactor = 2.0f;
FontSystemDefaults.KernelWidth = 2;
FontSystemDefaults.KernelHeight = 2;
```

Also there's a sample [FontStashSharp.Samples.Scaling](https://github.com/rds1983/FontStashSharp/tree/main/samples/FontStashSharp.Samples.Scaling), which could be used to tune those properties:
[[images/Scaling.png]]


