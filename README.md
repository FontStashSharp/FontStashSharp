# FontStashSharp
FontStashSharp is C# port of https://github.com/memononen/fontstash

Or - in other words - it is the engine-agnostic text rendering library with following features:
* Glyphs are rendered on demand on the texture atlas
* It's possible to have multiple ttf fonts per one FontSystem(i.e. one ttf with Latin characters, second with Japanese characters and third with emojis)
* Colored text
* Blurry and stroked text

# Installation
Package|NuGet
-------|-----
FontStashSharp|[![NuGet](https://img.shields.io/nuget/v/FontStashSharp.svg)](https://www.nuget.org/packages/FontStashSharp/)
FontStashSharp.StbTrueTypeSharp|[![NuGet](https://img.shields.io/nuget/v/FontStashSharp.StbTrueTypeSharp.svg)](https://www.nuget.org/packages/FontStashSharp.StbTrueTypeSharp/)

# Usage
In order to use FontStashSharp you need to provide implementations of 3 interfaces: [IFontRasterizer](src/FontStashSharp/Interfaces/IFontLoader.cs), [ITextureCreator](src/FontStashSharp/Interfaces/ITextureCreator.cs) and [IRenderer](src/FontStashSharp/Interfaces/IRenderer.cs). First two(IFontRasterizer and ITextureCreator) are provided during creation of [FontSystem](src/FontStashSharp/FontSystem.cs) and last one(IRenderer) is provided for every DrawText call.

Package FontStashSharp.StbTrueTypeSharp contains implementation of IFontRasterizer based on the [StbTrueTypeSharp](https://github.com/StbSharp/StbTrueTypeSharp)

# Sample
[FontStashSharp.Samples.MonoGame](samples/FontStashSharp.Samples.MonoGame) is full example of the FontStashSharp usage in the [MonoGame](https://www.monogame.net/).

# Screenshots
Ordinary Text:
![](/screenshots/simple.png)

Blurry Text:
![](/screenshots/blurry.png)

Stroked Text:
![](/screenshots/stroked.png)

## Credits
* [fontstash](https://github.com/memononen/fontstash)
* [stb](https://github.com/nothings/stb)
* [bartwe's fork of SpriteFontPlus](https://github.com/bartwe/SpriteFontPlus)
* [MonoGame](http://www.monogame.net/)

