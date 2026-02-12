By default, FontStashSharp uses SpriteBatch for the rendering. However it is possible to supply custom renderer instead.

In order to archieve that you need to provide an implementation for any of two interfaces: 
1. [IFontStashRenderer](https://github.com/rds1983/FontStashSharp/blob/main/src/FontStashSharp/Interfaces/IFontStashRenderer.cs) (renders data to XNA-like SpriteBatch)
2. [IFontStashRenderer2](https://github.com/rds1983/FontStashSharp/blob/main/src/FontStashSharp/Interfaces/IFontStashRenderer2.cs) (renders the raw vertex data)

Once the implementation is ready, you need to pass it to SpriteFontBase.DrawText. I.e.
```c#
  _font.DrawText(myRenderer, "Hello, World!", new Vector2(100, 100), Color.White);
```