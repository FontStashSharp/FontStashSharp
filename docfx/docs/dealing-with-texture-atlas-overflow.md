FontStashSharp renders glyphs on demand to the underlying texture atlas.
Which means when a glyph with the specified codepoint and the size is being rendered for the first time, the texture atlas is being updated with it.

This animation demonstrates how a sample texture atlas is being updated during the run-time:
![alt text](~/images/dealing-with-texture-atlas-overflow.gif)

If there's no more place on the texture atlas, then the new one is being created. `FontSystem` has special event `CurrentAtlasFull` that is fired, when that happens. 

`FontSystem`'s texture atlases could be accessed through extension method EnumerateTextures(works only for MonoGame/FNA/Stride):
```
  IEnumerable<Texture2D> textures = fontSystem.EnumeratesTextures();
```

Unfortunately, if a `FontSystem` has multiple textures, then the rendering performance would slightly go down, since it would need to swap between different textures.

One way of addressing the performance drop would be to reset(remove all texture atlases) `FontSystem` on `CurrentAtlasFull`:
```
  fontSystem.CurrentAtlasFull += (e, a) => fontSystem.Reset();
```
Such approach would require FontSystem to start filling the texture atlas from scratch, however it would eliminate the texture swaps.

