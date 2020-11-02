# FontStashSharp
[![NuGet](https://img.shields.io/nuget/v/FontStashSharp.svg)](https://www.nuget.org/packages/FontStashSharp/) 
[![Chat](https://img.shields.io/discord/628186029488340992.svg)](https://discord.gg/ZeHxhCY)

FontStashSharp is C# port of https://github.com/memononen/fontstash

Or - in other words - it is the engine-agnostic text rendering library with following features:
* Glyphs are rendered on demand on the texture atlas
* It's possible to have multiple ttf fonts per one FontSystem(i.e. one ttf with Latin characters, second with Japanese characters and third with emojis)
* Colored text
* Blurry and stroked text

# Usage
[Using FontStashSharp in MonoGame, FNA or Stride](https://github.com/rds1983/FontStashSharp/wiki/Using-FontStashSharp-in-MonoGame,-FNA-or-Stride)
[Using-FontStashSharp-in-generic-game-engine](https://github.com/rds1983/FontStashSharp/wiki/Using-FontStashSharp-in-generic-game-engine)

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
