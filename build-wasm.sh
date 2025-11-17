#!/bin/bash

################################################################################
# Phantom Nebula WASM Build Script
#
# This script builds the game into WebAssembly format.
# Note: Raylib-cs doesn't officially support WASM yet. This script provides
# alternative approaches:
# 1. Build as native binary and use Emscripten to compile to WASM
# 2. Port to Blazor WebAssembly
# 3. Use a JavaScript/WebGL alternative
################################################################################

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_DIR="PhantomNebula"
OUTPUT_DIR="bin/wasm"
BUILD_CONFIG="${1:-Release}"

echo -e "${BLUE}╔════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Phantom Nebula WASM Build Script      ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════╝${NC}"
echo ""

# Check if Emscripten is installed
check_emscripten() {
    if command -v emcc &> /dev/null; then
        echo -e "${GREEN}✓ Emscripten found${NC}"
        return 0
    else
        echo -e "${YELLOW}⚠ Emscripten not found${NC}"
        echo "  Install from: https://emscripten.org/docs/getting_started/downloads.html"
        return 1
    fi
}

# Build as native executable first
build_native() {
    echo ""
    echo -e "${BLUE}Building native executable...${NC}"
    dotnet build "$PROJECT_DIR" -c "$BUILD_CONFIG" -o "$OUTPUT_DIR/native"
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Native build successful${NC}"
        return 0
    else
        echo -e "${RED}✗ Native build failed${NC}"
        return 1
    fi
}

# Convert to WASM using Emscripten
build_emscripten_wasm() {
    if ! check_emscripten; then
        echo -e "${YELLOW}Skipping Emscripten build${NC}"
        return 1
    fi

    echo ""
    echo -e "${BLUE}Compiling to WASM with Emscripten...${NC}"

    local native_exe="$OUTPUT_DIR/native/PhantomNebula"

    if [ ! -f "$native_exe" ]; then
        echo -e "${RED}✗ Native executable not found: $native_exe${NC}"
        return 1
    fi

    mkdir -p "$OUTPUT_DIR/emscripten"

    emcc "$native_exe" -O3 \
        -s WASM=1 \
        -s ALLOW_MEMORY_GROWTH=1 \
        -s USE_SDL=2 \
        -s USE_OPENGL=1 \
        -o "$OUTPUT_DIR/emscripten/game.html"

    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ WASM build successful${NC}"
        echo "  Output: $OUTPUT_DIR/emscripten/"
        return 0
    else
        echo -e "${RED}✗ WASM compilation failed${NC}"
        return 1
    fi
}

# Create a Blazor WASM project template info
create_blazor_template() {
    echo ""
    echo -e "${BLUE}Creating Blazor WASM project template...${NC}"

    mkdir -p "$OUTPUT_DIR/blazor"

    cat > "$OUTPUT_DIR/blazor/index.html" << 'EOF'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Phantom Nebula</title>
    <link href="css/app.css" rel="stylesheet" />
</head>
<body>
    <div id="app">Loading...</div>
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
EOF

    cat > "$OUTPUT_DIR/blazor/README.md" << 'EOF'
# Blazor WASM Port

To port PhantomNebula to Blazor WebAssembly:

1. Create new Blazor Web App:
   ```bash
   dotnet new blazor -n PhantomNebula.Wasm
   ```

2. Add dependencies:
   ```bash
   cd PhantomNebula.Wasm
   dotnet add package Raylib-cs
   ```

3. Create Canvas-based renderer using HTML5 Canvas/WebGL
   - Raylib-cs doesn't officially support WASM
   - Alternative: Use SkiaSharp or manual WebGL bindings

4. Main program structure:
   ```csharp
   // Components/GameCanvas.razor
   @using System.Numerics
   @implements IAsyncDisposable

   <canvas @ref="canvasRef" width="1280" height="720"></canvas>

   @code {
       private ElementReference canvasRef;
       // Game logic here
   }
   ```

See: https://learn.microsoft.com/en-us/aspnet/core/blazor/
EOF

    echo -e "${GREEN}✓ Blazor template created at: $OUTPUT_DIR/blazor/${NC}"
}

# Create a simple web server wrapper
create_web_server() {
    echo ""
    echo -e "${BLUE}Creating web server launch script...${NC}"

    cat > "$OUTPUT_DIR/serve.sh" << 'EOF'
#!/bin/bash
# Simple web server for WASM build
# Requires Python 3

if command -v python3 &> /dev/null; then
    echo "Starting web server on http://localhost:8000"
    cd "$(dirname "$0")"
    python3 -m http.server 8000
elif command -v python &> /dev/null; then
    echo "Starting web server on http://localhost:8000"
    cd "$(dirname "$0")"
    python -m SimpleHTTPServer 8000
else
    echo "Error: Python not found. Install Python to run web server."
    exit 1
fi
EOF

    chmod +x "$OUTPUT_DIR/serve.sh"
    echo -e "${GREEN}✓ Web server script created${NC}"
}

# Main build process
main() {
    echo -e "${YELLOW}Build Configuration: $BUILD_CONFIG${NC}"
    echo ""

    # Try native build first
    if ! build_native; then
        exit 1
    fi

    # Try Emscripten WASM conversion
    if build_emscripten_wasm; then
        create_web_server
    fi

    # Create Blazor template info
    create_blazor_template

    echo ""
    echo -e "${GREEN}╔════════════════════════════════════════╗${NC}"
    echo -e "${GREEN}║  Build Complete!                       ║${NC}"
    echo -e "${GREEN}╚════════════════════════════════════════╝${NC}"
    echo ""
    echo "Output location: $OUTPUT_DIR"
    echo ""
    echo "Next steps:"
    echo "1. Emscripten WASM: Open $OUTPUT_DIR/emscripten/game.html in browser"
    echo "2. Blazor WASM: Follow instructions in $OUTPUT_DIR/blazor/README.md"
    echo "3. Web server: Run '$OUTPUT_DIR/serve.sh' to serve files locally"
    echo ""
    echo "Raylib-cs WASM Status:"
    echo "  ⚠ Official WASM support is experimental"
    echo "  → Consider using Emscripten or porting to Blazor"
    echo "  → Or use JavaScript/WebGL alternatives like Babylon.js"
}

main
