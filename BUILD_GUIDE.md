# Phantom Nebula Build Guide

Complete guide for building Phantom Nebula for all platforms.

## 📋 Quick Navigation

- **Getting Started?** → See [Quick Start](#quick-start) below
- **WASM/Web Build?** → See [WASM_BUILD.md](WASM_BUILD.md)
- **Command Reference?** → See [BUILD_OPTIONS.md](BUILD_OPTIONS.md)
- **Setup Summary?** → See [WASM_SETUP_SUMMARY.md](WASM_SETUP_SUMMARY.md)

## Quick Start

### For Desktop (Native)
```bash
dotnet build PhantomNebula -c Release
./PhantomNebula/bin/Release/net8.0/PhantomNebula
```

### For Web (WebAssembly)
```bash
./dotnet-wasm-install.sh    # First time only
./build-wasm.sh
./bin/wasm/serve.sh
# Open http://localhost:8000
```

### Interactive Menu
```bash
./build-web.sh
```

## System Requirements

| Requirement | Minimum | Recommended |
|------------|---------|-------------|
| **.NET** | 8.0 | 9.0+ |
| **OS** | Windows 10+ / macOS 10.14+ / Linux | Latest |
| **RAM** | 2GB | 8GB+ |
| **Storage** | 500MB | 2GB+ |
| **GPU** | Intel UHD 630 | NVIDIA RTX 30+ / AMD 6000+ |

## Build Options

### Build Scripts (Recommended)

**build-web.sh** - Interactive menu
```bash
./build-web.sh
```
Choose from:
1. Native desktop build
2. WebAssembly build (RaylibWasm)
3. Blazor setup instructions
4. Common commands
5. Tool availability check

**build-wasm.sh** - Automated WASM build
```bash
./build-wasm.sh
```
Automatically handles:
- .NET version checking
- WASM workload installation
- WASM compilation
- Web server setup

### Command Line (Advanced)

Native build:
```bash
dotnet build PhantomNebula -c Release
```

WebAssembly build:
```bash
dotnet workload install wasm-tools  # First time only
dotnet publish PhantomNebula -c Release -r browser-wasm
```

## Platform-Specific Builds

### Windows
```bash
# 64-bit native
dotnet build PhantomNebula -c Release -r win-x64

# Single executable
dotnet publish PhantomNebula -c Release -r win-x64 `
  -p:PublishSingleFile=true
```

### macOS
```bash
# Intel
dotnet build PhantomNebula -c Release -r osx-x64

# Apple Silicon (M1/M2/M3)
dotnet build PhantomNebula -c Release -r osx-arm64

# Both architectures
./build_universal.sh  # If available
```

### Linux
```bash
# 64-bit
dotnet build PhantomNebula -c Release -r linux-x64

# ARM64 (Raspberry Pi, etc.)
dotnet build PhantomNebula -c Release -r linux-arm64
```

### WebAssembly (Browser)
```bash
./build-wasm.sh
# Output: PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/
```

## Development Workflow

### 1. Building
```bash
# Debug build (fast compile, slow execution)
dotnet build PhantomNebula

# Release build (slower compile, fast execution)
dotnet build PhantomNebula -c Release
```

### 2. Running
```bash
# Debug
dotnet run --project PhantomNebula

# Release
dotnet run --project PhantomNebula -c Release
```

### 3. Building for Distribution
```bash
# Self-contained executable (includes runtime)
dotnet publish PhantomNebula -c Release --self-contained

# Single-file executable
dotnet publish PhantomNebula -c Release `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true
```

## Web Deployment

### Local Testing
```bash
./build-wasm.sh          # Build once
./bin/wasm/serve.sh      # Run server
# Visit http://localhost:8000
```

### Deployment Platforms

| Platform | Cost | Setup Time | Auto-Deploy |
|----------|------|-----------|------------|
| GitHub Pages | Free | 5 min | Git push |
| Netlify | Free | 5 min | Git push |
| Vercel | Free | 5 min | Git push |
| AWS S3 | Pay-as-you-go | 10 min | Manual |
| Azure | Free tier | 10 min | Git push |
| DigitalOcean | $5+/mo | 15 min | Manual |

### Deployment Steps (GitHub Pages)

1. Build WASM:
   ```bash
   ./build-wasm.sh
   ```

2. Copy to deployment directory:
   ```bash
   cp -r PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/* docs/
   ```

3. Commit and push:
   ```bash
   git add docs/
   git commit -m "Deploy to GitHub Pages"
   git push
   ```

4. Enable Pages in GitHub repo settings
   - Source: `docs/` directory
   - Visit: `https://username.github.io/Phantom-Sector/`

## Troubleshooting

### Build Issues

**"dotnet command not found"**
- Install .NET from https://dotnet.microsoft.com/

**"Raylib-cs not found"**
- Run: `dotnet restore PhantomNebula`

**"Shader files not found"**
- Ensure running from project root directory
- Check file permissions

### WASM Build Issues

**"browser-wasm runtime not found"**
- Install workload: `dotnet workload install wasm-tools`

**"Permission denied"**
- Use: `sudo dotnet workload install wasm-tools`

**"WASM build hangs"**
- First-time builds can be slow (5-15 minutes)
- Wait or check disk space

**"Game won't load in browser"**
- Check browser console (F12 → Console)
- Verify web server is running
- Clear browser cache
- Try Chrome/Edge instead of Firefox

### Runtime Issues

**"Out of memory"**
- Close other applications
- Try release build (smaller memory footprint)

**"Very slow performance"**
- Normal for WASM; runs slower than native
- Use Release build (automatic with build-wasm.sh)
- Browser warms up over time

**"Graphics issues"**
- Verify WebGL 2.0 support
- Try different browser
- Check WASM files are complete

## Project Structure

```
Phantom-Sector/
├── PhantomNebula/              # Main game project
│   ├── PhantomNebula.csproj   # Project file
│   ├── Program.cs              # Entry point
│   ├── Core/                   # Game systems
│   │   ├── GameConfig.cs      # Configuration
│   │   └── Transform.cs       # Positioning
│   ├── Game/                   # Game entities
│   │   ├── Ship.cs            # Player/NPC ships
│   │   └── ITarget.cs         # Targetable interface
│   ├── Renderers/              # Graphics
│   │   └── ShipRenderer.cs
│   ├── Physics/                # Physics system
│   │   └── PhysicsWorld.cs
│   ├── Scenes/                 # Game scenes
│   │   └── StarfieldScene.cs
│   ├── Shaders/                # GLSL shaders
│   └── Resources/              # Game assets
│       ├── Models/
│       └── Textures/
├── build-web.sh               # Interactive build menu
├── build-wasm.sh              # WASM build automation
├── WASM_BUILD.md              # WASM guide
├── BUILD_OPTIONS.md           # Command reference
└── BUILD_GUIDE.md             # This file
```

## Build Configuration Files

### PhantomNebula.csproj
Project configuration with dependencies:
```xml
<TargetFramework>net9.0</TargetFramework>
<RollForward>Major</RollForward>
<PackageReference Include="Raylib-cs" Version="6.0.0" />
<PackageReference Include="BepuPhysics" Version="2.4.0" />
```

### game_config.yaml
Runtime game configuration:
```yaml
models:
  ship:
    model: "Resources/Models/shippy1.obj"
    albedo: "Resources/Models/shippy.png"
  satellite:
    model: "Resources/Models/satelite.obj"
    albedo: "Resources/default_1.png"
```

## Performance Optimization

### Build Time
- **Debug**: ~10-30 seconds
- **Release**: ~1-5 minutes
- **WASM**: ~5-15 minutes (first time, includes workload)

### Runtime Performance
- **Native Desktop**: Full FPS
- **WASM**: 60% of native speed (varies by browser)
- **Optimization Tips**:
  - Use Release configuration
  - Minimize logging
  - Profile with browser DevTools
  - Use Chrome/Edge for best WASM performance

## Advanced Topics

### Creating Custom Builds
Edit build-web.sh or build-wasm.sh to add custom options

### Publishing Different Configurations
```bash
# Trimmed (smaller, faster startup)
dotnet publish PhantomNebula -c Release -p:PublishTrimmed=true

# Ready-to-Run (pre-compiled)
dotnet publish PhantomNebula -c Release -p:PublishReadyToRun=true

# Combined
dotnet publish PhantomNebula -c Release `
  -p:PublishTrimmed=true `
  -p:PublishReadyToRun=true
```

### Debugging WASM
1. Open browser DevTools (F12)
2. Check Console tab for errors
3. Check Network tab for missing files
4. Use Chrome DevTools protocol for deeper debugging

### Creating Release Packages
```bash
# Build and package for distribution
./build-web.sh  # Run option 1
tar -czf PhantomNebula-v1.0-macos.tar.gz \
  PhantomNebula/bin/Release/net8.0/PhantomNebula
```

## Additional Resources

- **Official Documentation**:
  - [.NET CLI](https://learn.microsoft.com/en-us/dotnet/core/tools/)
  - [WASM Support](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/wasm)
  - [Raylib Documentation](https://www.raylib.com/)

- **Community**:
  - [Raylib-cs GitHub](https://github.com/ChrisDill/Raylib-cs)
  - [RaylibWasm GitHub](https://github.com/Kiriller12/RaylibWasm)
  - [.NET Discord](https://discord.gg/dotnet)

- **Tools**:
  - [VS Code](https://code.visualstudio.com/) with C# extension
  - [Visual Studio](https://visualstudio.microsoft.com/) Community Edition
  - [Rider](https://www.jetbrains.com/rider/) (commercial)

## Getting Help

1. **For Build Issues**: Check `BUILD_OPTIONS.md`
2. **For WASM Issues**: Check `WASM_BUILD.md`
3. **For Setup**: Check `WASM_SETUP_SUMMARY.md`
4. **For Errors**: Check browser console (F12)
5. **For Community**: GitHub Issues or Discord

---

**Last Updated**: November 17, 2025
**Status**: Production Ready
**Supported Platforms**: Windows, macOS, Linux, WebAssembly
