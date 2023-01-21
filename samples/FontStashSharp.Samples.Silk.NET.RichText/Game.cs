using FontStashSharp.Platform;
using FontStashSharp.RichText;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using Rectangle = System.Drawing.Rectangle;

namespace FontStashSharp
{
	internal class Game
	{
		private readonly string[] Strings = new[]
		{
			"First line./nSecond line.",
			"This is /c[red]colored /c[#00f0fa]ext, /cdcolor could be set either /c[lightGreen]by name or /c[#fa9000ff]by hex code.",
			"Text in default font./n/f[arialbd.ttf, 24]Bold and smaller font. /f[ariali.ttf, 48]Italic and larger font./n/fdBack to the default font.",
			"E=mc/v[-8]2/n/vdMass–energy equivalence.",
			"A small tree: /i[mangrove1.png]",
			"A small /c[red]tree: /v[8]/i[mangrove1.png]/vd/cd/tuand some text",
			"/tu/ebThis /v[-3]/es3is the /vd/edfirst line. /tsThis is the second line. /tdThis is the third line.",
		};

		private IWindow window;
		private Renderer renderer;

		private RichTextLayout _richText;
		private int _stringIndex = 0;
		private readonly Dictionary<string, FontSystem> _fontCache = new Dictionary<string, FontSystem>();
		private readonly Dictionary<string, Texture> _textureCache = new Dictionary<string, Texture>();

		public void Run()
		{

			var options = WindowOptions.Default;
			options.Size = new Vector2D<int>(1200, 800);
			options.Title = "FontStashSharp.Silk.NET.RichText";
			window = Window.Create(options);

			window.Load += OnLoad;
			window.Render += OnRender;
			window.Closing += OnClose;

			window.Run();
		}

		private void OnLoad()
		{
			IInputContext input = window.CreateInput();
			for (int i = 0; i < input.Keyboards.Count; i++)
			{
				input.Keyboards[i].KeyDown += KeyDown;
			}

			Env.Gl = GL.GetApi(window);
			renderer = new Renderer();

			var fontSystem = new FontSystem();
			fontSystem.AddFont(File.ReadAllBytes(@"C:/Windows/Fonts/arial.ttf"));

            var richTextSettings = new RichTextSettings
            {
                FontResolver = p =>
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
                },
                ImageResolver = p =>
                {
                    Texture texture;

                    // _textureCache is field of type Dictionary<string, Texture2D>
                    // it is used to cache textures
                    if (!_textureCache.TryGetValue(p, out texture))
                    {
                        ImageResult imageResult;
                        using (var stream = File.OpenRead(Path.Combine(@"D:\Temp\DCSSTiles\dngn\trees\", p)))
                        {
                            imageResult = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                        }

                        // Premultiply Alpha
                        unsafe
                        {
                            fixed (byte* b = imageResult.Data)
                            {
                                byte* ptr = b;
                                for (var i = 0; i < imageResult.Data.Length; i += 4, ptr += 4)
                                {
                                    var falpha = ptr[3] / 255.0f;
                                    ptr[0] = (byte)(ptr[0] * falpha);
                                    ptr[1] = (byte)(ptr[1] * falpha);
                                    ptr[2] = (byte)(ptr[2] * falpha);
                                }
                            }
                        }

                        // Create the texture
                        texture = new Texture(imageResult.Width, imageResult.Height);
                        texture.SetData(new Rectangle(0, 0, texture.Width, texture.Height), imageResult.Data);

                        _textureCache[p] = texture;
                    }

                    return new TextureFragment(texture, new Rectangle(0, 0, texture.Width, texture.Height));
                }
            };

            _richText = new RichTextLayout(richTextSettings)
            {
                Font = fontSystem.GetFont(32),
                Text = Strings[_stringIndex],
                VerticalSpacing = 8
            };

			GC.Collect();
		}

		private void OnRender(double obj)
		{
			Env.Gl.ClearColor(Color.CornflowerBlue);
			Env.Gl.Clear((uint)ClearBufferMask.ColorBufferBit);

			renderer.Begin();
				
			_richText.Font.DrawText(renderer, "Press 'Space' to switch between strings.", Vector2.Zero, FSColor.White);

			Vector2 scale = Vector2.One;
			var viewportSize = new Point(window.Size.X, window.Size.Y);
			if (_stringIndex != 6)
			{
				_richText.Width = viewportSize.X;
			}
			else
			{
				_richText.Width = 300;
			}

			var position = new Vector2(0, viewportSize.Y / 2 - _richText.Size.Y / 2);
			_richText.Draw(renderer, position, FSColor.White, scale);

			renderer.End();
		}

		private void OnClose()
		{
			renderer.Dispose();
		}

		private void KeyDown(IKeyboard arg1, Key arg2, int arg3)
		{
			switch(arg2)
			{
				case Key.Escape:
					window.Close();
					break;
				case Key.Space:
					++_stringIndex;
					if (_stringIndex >= Strings.Length)
					{
						_stringIndex = 0;
					}

					_richText.Text = Strings[_stringIndex]; 
					break;
			}
		}
	}
}
