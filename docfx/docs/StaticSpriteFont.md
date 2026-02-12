FontStashSharp can render fonts loaded from [AngelCode BMFont format](https://www.angelcode.com/products/bmfont/) using class StaticSpriteFont.

Example usage code:

```c#
var fntData = File.ReadAllText("font.fnt");
SpriteFontBase font = StaticSpriteFont.FromBMFont(fntData,
	fileName => File.OpenRead(fileName),
	GraphicsDevice);
```

Sample: [FontStashSharp.Samples.StaticSpriteFont](https://github.com/rds1983/FontStashSharp/tree/main/samples/FontStashSharp.Samples.StaticSpriteFont)