Sometimes it makes sense to use existing texture as font glyphs atlas.

I.e. if you make a GUI library that uses FontStashSharp, then it makes sense to pass existing texture that holds GUI images to the `FontSystem`. Such approach is good for performance as it will minimize amount of texture swaps, since both GUI images and glyphs will be stored on a single texture.

That could be archieved by following code:
```c#
  var settings = new FontSystemSettings
  {
    ExistingTexture = texture,
    ExistingTextureUsedSpace = new Rectangle(0, 0, 160, 1024) // Rectangle that covers area already used by the GUI images
  };

  var fontSystem = new FontSystem(settings);
```

Now this `fontSystem` will use `texture` to store the font glyphs. It'll place them outside of the `ExistingTextureUsedSpace`.
If the space on the texture will run out, then FontStashSharp will create new texture of same size and place the new glyphs there.