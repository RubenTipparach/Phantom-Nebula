# Playing Phantom Nebula

Complete guide for running and playing Phantom Nebula on both desktop and web.

## Quick Start

### Desktop (Native)
```bash
dotnet build PhantomNebula -c Release
dotnet run --project PhantomNebula
```

### Web (WebAssembly)
```bash
./build-wasm.sh      # Build once (takes 5-15 minutes first time)
./bin/wasm/serve.sh  # Start web server
# Open browser to http://localhost:8000
```

## Where is index.html?

The `index.html` file is located at:
```
PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/index.html
```

This file is automatically created when you run the build and serves as the entry point for the web version of the game.

## Playing on the Web

### Step 1: Build the WebAssembly Version
```bash
./build-wasm.sh
```

This script will:
1. Check your .NET version (requires 8.0+)
2. Install the `wasm-tools` workload if needed
3. Compile PhantomNebula to WebAssembly
4. Create a web server launcher script
5. Output files to: `PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/`

**Note**: First-time build takes 5-15 minutes as it downloads and compiles all dependencies.

### Step 2: Start the Web Server
```bash
./bin/wasm/serve.sh
```

The `serve.sh` script will:
- Detect available web servers (dotnet serve, Python 3, Python 2, Node.js)
- Start the server on port 8000
- Display the URL to access the game

### Step 3: Open in Browser
Visit: **http://localhost:8000**

The game will load and display the Phantom Nebula title screen.

## Web Server Options

The `serve.sh` script automatically detects and uses the best available web server:

1. **dotnet serve** (recommended)
   - Automatically installed by build-wasm.sh
   - Best performance and features

2. **Python 3** (built-in on macOS/Linux)
   ```bash
   cd PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/
   python3 -m http.server 8000
   ```

3. **Python 2** (legacy)
   ```bash
   python -m SimpleHTTPServer 8000
   ```

4. **Node.js** (if installed)
   ```bash
   npx http-server -p 8000
   ```

## Playing Natively (Desktop)

### Build
```bash
dotnet build PhantomNebula -c Release
```

Output: `PhantomNebula/bin/Release/net8.0/PhantomNebula`

### Run
```bash
dotnet run --project PhantomNebula
```

Or directly execute the binary:
```bash
./PhantomNebula/bin/Release/net8.0/PhantomNebula
```

**Advantages of native desktop:**
- Full performance
- No browser limitations
- Direct access to system resources

## Troubleshooting

### Web Version Won't Load

**"Loading Phantom Nebula..." spinner stuck**
- Make sure `./build-wasm.sh` completed successfully
- Check browser console (F12 → Console tab) for errors
- Verify web server is running (check terminal output)
- Try a different browser (Chrome/Edge recommended for WASM)

**"Failed to Load Phantom Nebula" error**
- Check the error message in the browser
- Ensure WASM files exist: `PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/_framework/`
- Clear browser cache and reload
- Check browser supports WebAssembly (F12 → Application tab)

**Game is very slow**
- WASM runs 60-70% of native speed (normal)
- Wait a moment for initial load
- Use Chrome/Edge for better WASM performance
- Release builds are much faster than Debug (automatic with build-wasm.sh)

### Build Issues

**"dotnet command not found"**
- Install .NET from https://dotnet.microsoft.com/
- Requires .NET 8.0 or higher

**"browser-wasm runtime not found"**
- Run: `./dotnet-wasm-install.sh`
- Or manually: `dotnet workload install wasm-tools`

**Build takes forever**
- First WASM build downloads and compiles ~100MB of dependencies
- Subsequent builds are much faster (10-30 seconds)
- Check disk space (needs ~2GB free)

**"Permission denied" on build-wasm.sh**
- Make executable: `chmod +x build-wasm.sh`
- Then run: `./build-wasm.sh`

### Game Won't Start

**Native version crashes**
- Check .NET version: `dotnet --version` (need 8.0+)
- Verify graphics drivers are up-to-date
- Check available system resources

**Web version won't initialize**
- Check browser console (F12) for specific error
- Ensure all WASM files downloaded successfully (Network tab in F12)
- Try different browser or incognito window (no cache/extensions)

## File Structure

After building for web, you'll have:

```
PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/
├── index.html                    # ← Entry point (this is what you open)
├── package.json                  # Package metadata
├── PhantomNebula.runtimeconfig.json
└── _framework/                   # WASM runtime and assemblies
    ├── dotnet.js                # WASM loader
    ├── dotnet.wasm              # .NET runtime
    ├── PhantomNebula.wasm       # Game code
    ├── Raylib-cs.wasm           # Graphics library
    └── ... (other framework files)
```

## Browser Support

| Browser | WASM Support | Recommended |
|---------|--------------|-------------|
| Chrome 79+ | ✓ | **Best** |
| Edge 79+ | ✓ | **Best** |
| Firefox 79+ | ✓ | Good |
| Safari 14+ | ✓ | Good |
| Opera 66+ | ✓ | Good |

## Performance Tips

### For Web Build
1. Use Release configuration (automatic with build-wasm.sh)
2. Browser warms up over time - let it run a bit before judging performance
3. Close other browser tabs to reduce resource competition
4. Chrome/Edge generally faster for WASM than Firefox/Safari

### For Native Build
1. Use Release configuration: `dotnet build PhantomNebula -c Release`
2. Close background applications
3. Ensure graphics drivers are updated
4. On macOS, run on native architecture (M1/M2 if available)

## Development Workflow

### For Testing Web Changes
```bash
# Make code changes
# Rebuild WASM
./build-wasm.sh

# Refresh browser (hard refresh: Ctrl+Shift+R or Cmd+Shift+R)
# Go to http://localhost:8000
```

### For Testing Native Changes
```bash
# Make code changes
# Rebuild and run
dotnet run --project PhantomNebula
```

## Deployment

### Publishing to GitHub Pages
```bash
# Build WASM
./build-wasm.sh

# Copy to docs folder
cp -r PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/* docs/

# Commit and push
git add docs/
git commit -m "Update game build"
git push

# Enable GitHub Pages in repo settings (source: /docs directory)
# Game will be at: https://username.github.io/Phantom-Sector/
```

### Publishing to Netlify
```bash
# Build WASM
./build-wasm.sh

# Compress the build
tar -czf phantom-nebula-wasm.tar.gz PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/

# Upload to Netlify or use their CLI
netlify deploy --dir=PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/
```

## Getting Help

**For build errors**: Check [BUILD_GUIDE.md](BUILD_GUIDE.md)

**For WASM issues**: Check [WASM_BUILD.md](WASM_BUILD.md)

**For command reference**: Check [BUILD_OPTIONS.md](BUILD_OPTIONS.md)

**For browser console errors**: F12 → Console tab (shows detailed error messages)

---

**Have fun playing Phantom Nebula!** 🚀

For more info on building and deployment, see the other documentation files in the project root.
