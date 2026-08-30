1. Clone this repo.
2. Add src/XNA/FontStashSharp.FNA.csproj or src/XNA/FontStashSharp.FNA.Core.csproj to the solution.
3. The overall folder structure is expected to be following:
![alt text](~/images/adding-reference-to-fna.png)

### SDF Support

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

3. **Make sure `fxc` is available in PATH** - the FNA compile scripts invoke `fxc` (the DirectX shader compiler). It ships with the Windows SDK under `C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64\`. The simplest way to have it in PATH is to run the commands from a Developer Command Prompt / Visual Studio environment.

4. **Run the FNA compile script** - this compiles all SDF effect variants into `src/XNA/Effects/FNA/bin/*.efb`:
```bash
src/XNA/Effects/FNA/compile_all.bat
```

5. **Rebuild the project** - the generated `Effects\FNA\bin\*.efb` files are picked up automatically by the `EmbeddedResource` item on the next build.