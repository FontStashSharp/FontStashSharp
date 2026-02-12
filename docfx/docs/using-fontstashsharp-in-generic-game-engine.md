# Adding Reference
There's engine agnostic version of FontStashStash available at nuget:
https://www.nuget.org/packages/FontStashSharp.PlatformAgnostic/

# Usage
In order to use FontStashSharp you need to provide an implementation for any of two interfaces: 
1. [IFontStashRenderer](https://github.com/rds1983/FontStashSharp/blob/main/src/FontStashSharp/Interfaces/IFontStashRenderer.cs)
2. [IFontStashRenderer2](https://github.com/rds1983/FontStashSharp/blob/main/src/FontStashSharp/Interfaces/IFontStashRenderer2.cs)

# IFontStashRenderer
IFontStashRenderer renders data to XNA-like SpriteBatch.

There are two samples demonstrating implementation of that interface:
1. [FontStashSharp.Samples.MonoGameBackend](https://github.com/rds1983/FontStashSharp/tree/main/samples/FontStashSharp.Samples.MonoGameBackend)
2. [FontStashSharp.Samples.Silk.NET.TrippyGL](https://github.com/FontStashSharp/FontStashSharp/tree/main/samples/FontStashSharp.Samples.Silk.NET.TrippyGL)

# IFontStashRenderer2
IFontStashRenderer2 renders the raw vertex data.

There are two samples demonstrating implementation of that interface:
1. [FontStashSharp.Samples.OpenTK](https://github.com/FontStashSharp/FontStashSharp/tree/main/samples/FontStashSharp.Samples.OpenTK)
2. [FontStashSharp.Samples.Silk.NET](https://github.com/FontStashSharp/FontStashSharp/tree/main/samples/FontStashSharp.Samples.Silk.NET)