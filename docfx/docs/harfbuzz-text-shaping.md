HarfBuzz text shaping is required to properly render some languages such as Hindi or Arabic.

In order to enable it, add reference to the following package:
https://www.nuget.org/packages/FontStashSharp.TextShapers.HarfBuzz

Then create the FontSystem using following code:
```c#
var settings = new FontSystemSettings();
settings.TextShaper = new HarfBuzzTextShaper();

fontSystem = new FontSystem(settings);
```

Now, that `fontSystem` will render HarfBuzz shaped text.

If you want to enable HarfBuzz text shaping for all the text rendering, add following line somewhere in the beginning of the application:
```c#
FontSystemDefaults.TextShaper = new HarfBuzzTextShaper();
```
It's important to note, that doing that isn't recommended. Since the shaped rendering is slower than ordinary. So it's better to do shaped rendering only for the languages that require it.

Following samples demonstrate the feature:

[MonoGame/FNA sample](https://github.com/FontStashSharp/FontStashSharp/tree/main/samples/FontStashSharp.Samples.HarfBuzz)

[Silk.NET sample](https://github.com/FontStashSharp/FontStashSharp/tree/main/samples/FontStashSharp.Samples.Silk.NET.HarfBuzz)

![alt text](~/images/harfbuzz-text-shaping.png)