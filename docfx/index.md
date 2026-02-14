## Adding Reference
Following platforms have their own versions of FontStashSharp that are available at the nuget:

Platform|Link
--------|----
[MonoGame](https://monogame.net/)|[FontStashSharp.MonoGame](https://www.nuget.org/packages/FontStashSharp.MonoGame)
[Stride](https://www.stride3d.net/)|[FontStashSharp.Stride](https://www.nuget.org/packages/FontStashSharp.Stride)
[Kni](https://github.com/kniEngine/kni)|[FontStashSharp.Kni](https://www.nuget.org/packages/FontStashSharp.Kni)
XNA|[FontStashSharp.XNA](https://www.nuget.org/packages/FontStashSharp.XNA)

See [this](docs/adding-reference-to-fna.md) on how to add the FontStashSharp reference to a FNA project.

## Basic Usage
Following code creates FontSystem from a ttf:
```c#
	private SpriteBatch _spriteBatch; 
	private FontSystem _fontSystem;

	/// <summary>
	/// LoadContent will be called once per game and is the place to load
	/// all of your content.
	/// </summary>
	protected override void LoadContent()
	{
		// Create a new SpriteBatch, which can be used to draw textures.
		_spriteBatch = new SpriteBatch(GraphicsDevice);

		_fontSystem = new FontSystem();
		_fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));
	}
```

Now the text could be drawn using following code:
```c#
	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.CornflowerBlue);

		// Render some text
		_spriteBatch.Begin();

		SpriteFontBase font18 = _fontSystem.GetFont(18);
		_spriteBatch.DrawString(font18, "The quick brown fox\njumps over\nthe lazy dog", new Vector2(0, 0), Color.White);

		SpriteFontBase font30 = _fontSystem.GetFont(30);
		_spriteBatch.DrawString(font30, "The quick brown fox\njumps over\nthe lazy dog", new Vector2(0, 80), Color.Yellow);

		_spriteBatch.End();

		base.Draw(gameTime);
	}
```

It would render following:
![alt text](~/images/getting-started-1.png)

## FontSystem from multiple ttfs

Sometimes a single ttf doesn't contain all required glyphs. In this case, it makes sense to create FontSystem from multiple ttfs. Using code like this:
```c#
		_fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));
		_fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSansJapanese.ttf"));
		_fontSystem.AddFont(File.ReadAllBytes(@"Fonts/Symbola-Emoji.ttf"));
```

Now the renderer will look for a requested glyph within all provided ttfs.
So, if the render code is
```c#
	   SpriteFontBase font18 = _fontSystem.GetFont(18);
		_spriteBatch.DrawString(font18, "The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog", new Vector2(0, 0), Color.White);

		SpriteFontBase font30 = _fontSystem.GetFont(30);
		_spriteBatch.DrawString(font30, "The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog", new Vector2(0, 80), Color.Yellow);
```

It would render following:
![alt text](~/images/getting-started-2.png)

## Colored Text

If you want to draw a colored text, then pass array of colors(a color for every rendered character). I.e.
```c#
	private static readonly Color[] ColoredTextColors = new Color[]
	{
		Color.Red,
		Color.Blue,
		Color.Green,
		Color.Aquamarine,
		Color.Azure,
		Color.Chartreuse,
		Color.Lavender,
		Color.OldLace,
		Color.PaleGreen,
		Color.SaddleBrown,
		Color.IndianRed,
		Color.ForestGreen,
		Color.Khaki
	};

	/// <summary>
	/// This is called when the game should draw itself.
	/// </summary>
	/// <param name="gameTime">Provides a snapshot of timing values.</param>
	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.CornflowerBlue);

		// Render some text
		_spriteBatch.Begin();

		SpriteFontBase font30 = _fontSystem.GetFont(30);
		_spriteBatch.DrawString(font30, "Colored Text", Vector2.Zero, ColoredTextColors);

		_spriteBatch.End();

		base.Draw(gameTime);
	}
```

It would render following:
![alt text](~/images/getting-started-3.png)

## Blurry and Stroked Text

If you want to draw blurry text, pass the corresponding parameter to the method DrawText. I.e.
```c#
_spriteBatch.DrawString(_font,
	"The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog",
	new Vector2(0, 80), Color.Yellow,
	effect: FontSystemEffect.Blurry, effectAmount: 1);
```

It would render following:
![alt text](~/images/getting-started-4.png)

The stroke effect could be renderered similar way:
```c#
_spriteBatch.DrawString(_font,
	"The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog",
	new Vector2(0, 80), Color.Yellow,
	 effect: FontSystemEffect.Stroked, effectAmount: 1);
  ```

It would render following:
![alt text](~/images/getting-started-5.png)