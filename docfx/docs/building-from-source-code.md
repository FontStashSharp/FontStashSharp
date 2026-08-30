1. Clone this repo.
2. Open a solution from the "build" folder.

### SDF Support(works only for MonoGame and FNA backends)

SDF text rendering requires the SDF effect shaders to be compiled into precompiled `.efb` blobs:

1. **Install or update efscriptgen** - [efscriptgen](https://github.com/rds1983/efscriptgen) is the tool that generates the `compile_*.bat` scripts for every effect variant declared in `src/XNA/Effects/Text.xml`:
```bash
dotnet tool install --global efscriptgen
# or, to update an existing installation:
dotnet tool update --global efscriptgen
```

2. **Generate the compile scripts** - navigate to the `src/XNA/Effects` folder and execute:
```bash
efscriptgen .
```
This scans the folder for `.fx` files (here `Text.fx`) together with the variant definitions in `Text.xml` and generates the `compile_*.bat` scripts under `FNA`, `MonoGameDX11` and `MonoGameOGL` subfolders.

Then compile the effects for the backend(s) you use:

#### FNA

3. **Make sure `fxc` is available in PATH** - the FNA compile script invokes `fxc` (the DirectX shader compiler). It ships with the Windows SDK under `C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64\`. The simplest way to have it in PATH is to run the commands from a Developer Command Prompt / Visual Studio environment.

4. **Run the FNA compile script** - this compiles all SDF effect variants into `src/XNA/Effects/FNA/bin/*.efb`:
```bash
src/XNA/Effects/FNA/compile_all.bat
```

#### MonoGame

5. **Install or update mgfxc** - [mgfxc](https://docs.monogame.net/articles/getting_started/tools/mgfxc.html) is the MonoGame effect compiler used by the `MonoGameDX11` and `MonoGameOGL` compile scripts:
```bash
dotnet tool install --global dotnet-mgfxc
# or, to update an existing installation:
dotnet tool update --global dotnet-mgfxc
```

6. **Run the MonoGame compile scripts** - this compiles all SDF effect variants into `src/XNA/Effects/MonoGameDX11/bin/*.efb` (mgfxc `/Profile:DirectX_11`) and `src/XNA/Effects/MonoGameOGL/bin/*.efb` (mgfxc `/Profile:OpenGL`):
```bash
src/XNA/Effects/MonoGameDX11/compile_all.bat
src/XNA/Effects/MonoGameOGL/compile_all.bat
```

Both folders are embedded into `FontStashSharp.MonoGame`; the one used at runtime depends on the graphics backend (`MonoGameOGL` for OpenGL/DesktopGL, `MonoGameDX11` for DirectX). Build both to cover every platform.

7. **Rebuild the project** - the generated `Effects\FNA\bin\*.efb`, `Effects\MonoGameDX11\bin\*.efb` and `Effects\MonoGameOGL\bin\*.efb` files are picked up automatically by the `EmbeddedResource` items on the next build.
