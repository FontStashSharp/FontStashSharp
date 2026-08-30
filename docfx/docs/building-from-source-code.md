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

2. **Make sure `fxc` is available in PATH** - the FNA compile scripts invoke `fxc` (the DirectX shader compiler). It ships with the Windows SDK under `C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64\`. The simplest way to have it in PATH is to run the commands from a Developer Command Prompt / Visual Studio environment.

3. **Run the FNA compile script** - this compiles all SDF effect variants into `src/XNA/Effects/FNA/bin/*.efb`:
```bash
src/XNA/Effects/FNA/compile_all.bat
```

4. **Rebuild the project** - the generated `Effects\FNA\bin\*.efb` files are picked up automatically by the `EmbeddedResource` item on the next build. 	
