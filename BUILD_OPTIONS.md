# Phantom Nebula Build Options Reference

Quick reference for building Phantom Nebula for different platforms and purposes.

## Build Scripts

### build-web.sh - Interactive Build Menu
Launch the interactive menu for build options:
```bash
./build-web.sh
```

Features:
- Interactive menu system
- System tool detection
- Documentation for each build option
- Quick access to all build methods

### build-wasm.sh - WebAssembly Build
Automated WASM build using RaylibWasm approach:
```bash
./build-wasm.sh
```

Automatically:
- Checks .NET version (requires 8.0+)
- Installs wasm-tools workload if needed
- Publishes to browser-wasm runtime
- Creates web server launcher script

## Quick Commands

### Install WASM Workload (One-Time Setup)
```bash
./dotnet-wasm-install.sh
```
Or manually:
```bash
dotnet workload install wasm-tools
```

### Native Desktop Build
```bash
dotnet build PhantomNebula -c Release
```
Output: `PhantomNebula/bin/Release/net8.0/PhantomNebula`

### WebAssembly Build (RaylibWasm - Recommended)
```bash
./build-wasm.sh
```
Output: `PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/`

### WebAssembly Build (Manual)
```bash
dotnet publish PhantomNebula -c Release -r browser-wasm
```

### Debug Build
```bash
dotnet build PhantomNebula -c Debug
```

### Release with Performance Optimizations
```bash
dotnet build PhantomNebula -c Release -p:PublishTrimmed=true
```

### Publish Self-Contained Executable
```bash
dotnet publish PhantomNebula -c Release -o bin/publish --self-contained
```

### Single-File Executable (Windows)
```bash
dotnet publish PhantomNebula -c Release -o bin/dist `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true
```

## Platform-Specific Builds

### Windows
```bash
# Native desktop
dotnet build PhantomNebula -c Release -r win-x64

# Single-file executable
dotnet publish PhantomNebula -c Release -r win-x64 `
  -p:PublishSingleFile=true
```

### macOS
```bash
# Intel
dotnet build PhantomNebula -c Release -r osx-x64

# Apple Silicon
dotnet build PhantomNebula -c Release -r osx-arm64

# Universal (Intel + ARM)
dotnet publish PhantomNebula -c Release `
  -p:PublishSingleFile=true `
  -r osx-x64 -o bin/osx-x64
dotnet publish PhantomNebula -c Release `
  -p:PublishSingleFile=true `
  -r osx-arm64 -o bin/osx-arm64
```

### Linux
```bash
# x64
dotnet build PhantomNebula -c Release -r linux-x64

# ARM64
dotnet build PhantomNebula -c Release -r linux-arm64
```

### WebAssembly
```bash
# RaylibWasm approach (recommended)
./build-wasm.sh

# Manual publish
dotnet publish PhantomNebula -c Release -r browser-wasm
```

## Web Hosting

### Local Development
```bash
# Using provided script
./bin/wasm/serve.sh

# Manual methods
cd PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/

# Python 3
python3 -m http.server 8000

# Python 2
python -m SimpleHTTPServer 8000

# dotnet serve
dotnet serve -p 8000

# Node.js
npx http-server -p 8000
```

Visit: `http://localhost:8000`

### Deployment Options

1. **GitHub Pages** - Free, GitHub integrated
2. **Netlify** - Easy deployment from GitHub
3. **Vercel** - Optimized for web apps
4. **AWS S3 + CloudFront** - Scalable, global
5. **Azure Static Web Apps** - Microsoft cloud
6. **Heroku** - Simple deployment (deprecated free tier)
7. **Firebase Hosting** - Google's solution
8. **DigitalOcean** - VPS hosting with web server

## Configuration

### Build Configuration Profiles

- **Debug** - Full symbols, no optimizations, slower
- **Release** - Optimized, smaller output, faster

### Publish Configurations

Common publish options:

```bash
# Trimmed assembly (removes unused code)
-p:PublishTrimmed=true

# Single file executable
-p:PublishSingleFile=true

# Include native libraries
-p:IncludeNativeLibrariesForSelfExtract=true

# Ready to run (pre-compiled)
-p:PublishReadyToRun=true
```

## Runtime Identifiers (RID)

Common platform identifiers for `dotnet publish -r`:

- `win-x64` - Windows 64-bit
- `win-arm64` - Windows ARM64
- `osx-x64` - macOS Intel
- `osx-arm64` - macOS Apple Silicon
- `linux-x64` - Linux 64-bit
- `linux-arm64` - Linux ARM64
- `browser-wasm` - WebAssembly (browser)

Full list: https://learn.microsoft.com/en-us/dotnet/core/rid-catalog

## Project Structure

```
PhantomNebula/
├── PhantomNebula.csproj       # Project configuration
├── Program.cs                 # Entry point
├── Core/                      # Core game systems
├── Game/                      # Game entities
├── Renderers/                 # Rendering systems
├── Scenes/                    # Game scenes
├── Physics/                   # Physics system
├── Shaders/                   # GLSL shaders
├── Resources/                 # Game assets
│   ├── Models/               # 3D models
│   ├── Textures/             # Textures
│   └── planet.png
└── game_config.yaml          # Game configuration
```

## Troubleshooting Build Issues

### Build fails with shader errors
- Ensure shader files are in `Shaders/` directory
- Check file extensions (.vs, .fs, .glsl)
- Verify `CopyToOutputDirectory` in .csproj

### WASM build fails
```bash
# Clear workload cache and reinstall
dotnet workload repair
dotnet workload install wasm-tools
```

### Package reference issues
```bash
# Restore dependencies
dotnet restore PhantomNebula

# Update packages
dotnet add package --update-all
```

### Platform-specific build errors
- Verify correct runtime identifier (RID)
- Check required system libraries are installed
- For Linux: `sudo apt-get install libgl1-mesa-dev` (graphics)

## Performance Tips

1. **Use Release Configuration**: ~3-5x faster than Debug
2. **Enable Ready-to-Run**: `-p:PublishReadyToRun=true`
3. **Trim Assemblies**: `-p:PublishTrimmed=true` (saves 20-30%)
4. **Profile with DevTools**: Check frame times and bottlenecks
5. **WASM Optimization**: Modern browsers JIT compile WASM for better performance

## Resources

- [.NET Publish Documentation](https://learn.microsoft.com/en-us/dotnet/core/deploying/deploy-with-cli)
- [RaylibWasm GitHub](https://github.com/Kiriller12/RaylibWasm)
- [Raylib-cs Documentation](https://github.com/ChrisDill/Raylib-cs)
- [.NET RID Catalog](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)
