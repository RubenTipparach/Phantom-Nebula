# Phantom Nebula WebAssembly Build Guide

This guide explains how to build and run Phantom Nebula in your web browser using WebAssembly.

## Overview

Phantom Nebula supports multiple approaches to web deployment:

1. **RaylibWasm (Recommended)** - Native Raylib-cs compiled to WebAssembly
2. **Blazor WebAssembly** - Full C# in the browser with custom rendering
3. **Emscripten** - Experimental conversion of native binary to WASM
4. **Web Rewrite** - Rewrite in JavaScript/TypeScript with Three.js or Babylon.js

## Quick Start (RaylibWasm)

### Prerequisites

- .NET 8.0 or higher
- `wasm-tools` workload installed

### Build Steps

1. **Install WASM Workload** (first time only):
   ```bash
   ./dotnet-wasm-install.sh
   ```

   Or manually:
   ```bash
   dotnet workload install wasm-tools
   ```

2. **Build WebAssembly**:
   ```bash
   ./build-wasm.sh
   ```

3. **Start Web Server**:
   ```bash
   ./bin/wasm/serve.sh
   ```

4. **Open in Browser**:
   ```
   http://localhost:8000
   ```

## Detailed Setup

### Using build-wasm.sh Script

The `build-wasm.sh` script automates the entire WASM build process:

```bash
./build-wasm.sh
```

This script will:
- Check your .NET version (8.0+ required)
- Install or verify `wasm-tools` workload
- Publish PhantomNebula for `browser-wasm` runtime
- Create a web server launch script at `bin/wasm/serve.sh`
- Display build information and next steps

### Manual Build (Advanced)

If you prefer to build manually:

1. **Install WASM Workload**:
   ```bash
   dotnet workload install wasm-tools
   ```

2. **Publish to WASM**:
   ```bash
   dotnet publish PhantomNebula -c Release -r browser-wasm
   ```

3. **Output Location**:
   ```
   PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/
   ```

4. **Serve Files**:
   ```bash
   # Using dotnet serve
   dotnet serve -p 8000 --path PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/

   # Using Python
   cd PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/
   python3 -m http.server 8000

   # Using Node.js
   npx http-server -p 8000 PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/
   ```

## Web Servers

### Available Options (in order of preference)

1. **dotnet serve** (recommended)
   ```bash
   dotnet serve -p 8000
   ```

2. **Python 3** (built-in on most systems)
   ```bash
   python3 -m http.server 8000
   ```

3. **Python 2** (legacy)
   ```bash
   python -m SimpleHTTPServer 8000
   ```

4. **Node.js**
   ```bash
   npx http-server -p 8000
   ```

## Troubleshooting

### Issue: "browser-wasm runtime not found"
**Solution**: Install WASM workload
```bash
dotnet workload install wasm-tools
```

### Issue: "WASM build failed" or cryptic errors
**Solution**: Use `dotnet publish` command, NOT Visual Studio publish feature
- The RaylibWasm documentation explicitly warns against using Visual Studio's UI
- Always use command line: `dotnet publish PhantomNebula -c Release -r browser-wasm`

### Issue: Game doesn't load in browser
**Solutions**:
1. Check browser console for errors (F12 → Console tab)
2. Verify web server is running
3. Check WASM files exist in `PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/`
4. Try a different web server option
5. Clear browser cache and reload

### Issue: Performance is slow
**Explanations**:
- WASM interpretation overhead varies by browser
- JavaScript/Native FFI calls have latency
- Initial load includes downloading ~3-5MB WASM binary
- Recommendations:
  - Use Chrome/Edge for best WASM performance
  - Wait for WASM binaries to load completely
  - Check for excessive logging in debug builds

### Issue: Graphics/Rendering issues
**Solutions**:
1. Verify Raylib-cs shaders are copied to build output
2. Check WebGL support in browser (most modern browsers support it)
3. Try different browser (Chrome/Edge recommended)
4. Check browser console for shader compilation errors

## Build Output

After successful build, you'll find:

```
PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/
├── index.html          # Entry point
├── app.wasm           # Main WASM binary
├── dotnet.wasm        # .NET runtime
├── dotnet.js          # WASM loader
├── dotnet.worker.js   # Worker support
└── ... (other support files)
```

## Alternative Approaches

### Blazor WebAssembly

Blazor provides a full C# development experience in the browser, but requires rewriting rendering code:

```bash
dotnet new blazor -n PhantomNebula.Web --interactivity Auto
cd PhantomNebula.Web
dotnet add package Raylib-cs  # Raylib-cs not officially supported in Blazor
```

See [Microsoft Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)

### JavaScript/TypeScript Rewrite

For best web performance, consider rewriting in JavaScript with:
- **Three.js** - Full 3D graphics
- **Babylon.js** - Advanced 3D features
- **PixiJS** - 2D rendering optimized for web
- **WebGL** - Direct graphics API

## Resources

- [RaylibWasm Repository](https://github.com/Kiriller12/RaylibWasm)
- [.NET WASM Documentation](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/wasm)
- [Raylib-cs GitHub](https://github.com/ChrisDill/Raylib-cs)
- [WebAssembly Specification](https://webassembly.org/)

## Performance Notes

WASM builds run slightly slower than native due to:
- Interpretation overhead (JIT compilation helps)
- JavaScript/WASM FFI latency
- Browser garbage collection pauses
- Graphics API translation (OpenGL → WebGL)

For production games, consider:
1. Optimizing hot paths (game loop, rendering)
2. Profiling with browser DevTools
3. Using Release builds (much faster than Debug)
4. Async resource loading
5. Progressive Web App (PWA) features for offline support

## Next Steps

1. Build with `./build-wasm.sh`
2. Test in browser at `http://localhost:8000`
3. Share the contents of `PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/` directory to deploy
4. Host on web server (GitHub Pages, Netlify, Vercel, etc.)
