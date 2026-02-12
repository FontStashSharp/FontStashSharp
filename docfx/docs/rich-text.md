### Basic Usage
FontStashSharp supports simple rich text functionality through special class RichTextLayout.

The following code demonstrates it's basic usage with text that has one simple command '/n'(line break):
```c#
RichTextLayout rtl = new RichTextLayout
{
  Font = fontSystem.GetFont(32), // fontSystem is default Arial
  Text = "First line./nSecond line.",
};
```
Now it could be rendered using following code:
```c#
rtl.Draw(_spriteBatch, position, Color.White);
```
Which would result in this:

![alt text](~/images/rich-text-1.png)

Use property RichTextLayout.Size to find out the size in pixels of the rendered rich text.

### Commands
Following chapter lists all supported commands. Later chapters would describe each in the detail.

Name|Description|Examples
----|-----------|--------
//|Symbol '/'|
/c[_color_]|Changes the current color|/c[red] or /c[#ff123456]
/cd|Changes the current color back to default(one that is passed to the RichTextLayout.Draw method)|
/eb[_effectAmount]|Turns on the blurry text effect. If amount parameter is ommited, then effect amount is set to 1 by default.|/eb or /eb2 or /eb[2]
/es[_effectAmount]|Turns on the stroked text effect. If amount parameter is ommited, then effect amount is set to 1 by default.|/es or /es2 or /es[2]
/ed|Turns off the text effect|
/f[_fontString_]|Changes the current font|/f[arialbd.ttf,32]
/fd|Changes the current font to default(RichTextLayout.Font)|
/i[_imageString_]|Inserts an image|/i[image.png]
/n|Line break|
/s[_size_]|Insert a space with size specified in pixels|/s100 or /s[100]
/tu|Sets text style to underline|
/ts|Sets text style to strikethrough|
/td|Sets text style to default|
/v[_offset_]|Sets the vertical offset in pixels|/v-10 or /v[-10]
/vd|Sets the vertical offset to zero|

### Commands '/c' and '/cd'
Command '/c[_color_]' changes the current color. The '_color_' could be either color name or its hex code(in RGB or RGBA format). In latter case, it should be preceded by symbol '#'. 

Command '/cd' changes the color back to the default, one that is passed to the RichTextLayout.Draw method.

I.e. if we pass the following text to RichTextLayout: 
```
This is /c[red]colored /c[#00f0fa]ext, /cdcolor could be set either /c[lightGreen]by name or /c[#fa9000ff]by hex code.
```

It would render this(if the default color passed to RichTextLayout.Draw is white):

![alt text](~/images/rich-text-2.png)

### Commands '/f' and '/fd'
Command '/f[_fontString_]' changes the current font. The '_fontString_' argument should be parsed by the developer. That is archieved by implementing a handler function that accepts string argument and returns value of type SpriteFontBase. The function should be assigned to static property RichTextDefaults.FontResolver.

I.e. following code implements the font resolver - that parses the font file name and size separated by the comma - and creates the font from "C:\Windows\Fonts":
```c#
RichTextDefaults.FontResolver = p =>
{
        // Parse font name and size
	var args = p.Split(',');
	var fontName = args[0].Trim();
	var fontSize = int.Parse(args[1].Trim());

	// _fontCache is field of type Dictionary<string, FontSystem>
	// It is used to cache fonts
	FontSystem fontSystem;
	if (!_fontCache.TryGetValue(fontName, out fontSystem))
	{
                // Load and cache the font system
		fontSystem = new FontSystem();
		fontSystem.AddFont(File.ReadAllBytes(Path.Combine(@"C:\Windows\Fonts", fontName)));
		_fontCache[fontName] = fontSystem;
	}

        // Return the required font
	return fontSystem.GetFont(fontSize);
};
```

Now if we pass the following text to RichTextLayout:
```
Text in default font./n/f[arialbd.ttf, 24]Bold and smaller font. /f[ariali.ttf, 48]Italic and larger font./n/fdBack to the default font.
```

It would render following:

![alt text](~/images/rich-text-3.png)

### Commands '/v' and '/vd'
Command '/v[_offset_]' sets the vertical offset in pixels. Command '/vd' sets it back to zero.

If we pass the following text to RichTextLayout:
```
E=mc/v[-8]2/n/vdMass–energy equivalence.
```

It would render following:

![alt text](~/images/rich-text-4.png)

### Command '/i'
Command '/i[_imageString_]' inserts an image. Similarly to handling fonts, it's the developer responsibility to implement the image resolver function, which accepts string and returns object of type [IRenderable](https://github.com/FontStashSharp/FontStashSharp/blob/main/src/FontStashSharp/RichText/IRenderable.cs). The function should be assigned to the static property RichTextDefaults.ImageResolver.

There's class [TextureFragment](https://github.com/FontStashSharp/FontStashSharp/blob/main/src/FontStashSharp/RichText/TextureFragment.cs) that implements IRenderable by simply rendering fragment of the texture.

Hence the sample implementation of the image resolver could look like this:
```c#
RichTextDefaults.ImageResolver = p =>
{
	Texture2D texture;

	// _textureCache is field of type Dictionary<string, Texture2D>
	// it is used to cache textures
	if (!_textureCache.TryGetValue(p, out texture))
	{
		using (var stream = File.OpenRead(Path.Combine(@"D:\Temp\DCSSTiles\dngn\trees\", p)))
		{
			texture = Texture2D.FromStream(GraphicsDevice, stream);
		}

		_textureCache[p] = texture;
	}

	return new TextureFragment(texture);
};
```

Now if we pass the following text to RichTextLayout:
```
A small tree: /i[mangrove1.png]
```

It would render following:

![alt text](~/images/rich-text-5.png)

Images take into account '/v' command, but ignore '/c' command. Hence if pass following text:
```
A small /c[red]tree: /v8/i[mangrove1.png]
```

It would render this:

![alt text](~/images/rich-text-6.png)

### Word Wrapping
If you set RichTextLayout.Width to some value, then the text would be word-wrapped accordingly.

I.e. if we pass following text:
```
This is the first line. This is the second line. This is the third line.
```

And set RichTextLayout.Width to 300.

Following would be rendered:

![alt text](~/images/rich-text-7.png)

### Auto Ellipsis
The ellipsis could be added to the end of the text, if it doesnt fit the specified dimensions.
This feature is used when both RichTextLayout.Width and RichTextLayout.Height are set. And RichTextLayout.AutoEllipsisMethod is set to either Word or Character. Also RichTextLayout.AutoEllipsisString determines the string to use as the ellipsis.

If in the above example we set Width to 250 and Height to 100. And AutoEllipsisMethod is set to Character, then the following would be rendered:

![alt text](~/images/rich-text-8.png)

If AutoEllipsisMethod is set to Word, then following:

![alt text](~/images/rich-text-9.png)

### Sample
https://github.com/FontStashSharp/FontStashSharp/tree/main/samples/FontStashSharp.Samples.RichText
