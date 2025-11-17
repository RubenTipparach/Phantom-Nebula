#!/bin/bash

################################################################################
# Phantom Nebula WASM Build Script
#
# Builds PhantomNebula for WebAssembly using RaylibWasm approach
# Requires: .NET 8.0+ with wasm-tools workload
#
# Build process:
# 1. Install wasm-tools workload
# 2. Publish project with WASM runtime identifier
# 3. Generate browser-ready artifacts
#
# See: https://github.com/Kiriller12/RaylibWasm
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
echo -e "${BLUE}║ Phantom Nebula WASM Build (RaylibWasm) ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════╝${NC}"
echo ""

# Check .NET version
check_dotnet() {
    if command -v dotnet &> /dev/null; then
        version=$(dotnet --version)
        echo -e "${GREEN}✓ .NET found: $version${NC}"

        # Check if version is 8.0 or higher
        major=$(echo "$version" | cut -d. -f1)
        if [ "$major" -lt 8 ]; then
            echo -e "${RED}✗ .NET 8.0 or higher required (found: $version)${NC}"
            return 1
        fi
        return 0
    else
        echo -e "${RED}✗ .NET not found${NC}"
        return 1
    fi
}

# Install wasm-tools workload
install_wasm_workload() {
    echo ""
    echo -e "${BLUE}Checking WASM workload...${NC}"

    # Try to list workloads
    if dotnet workload list 2>/dev/null | grep -q "wasm-tools-net8"; then
        echo -e "${GREEN}✓ wasm-tools-net8 workload already installed${NC}"
        return 0
    elif dotnet workload list 2>/dev/null | grep -q "wasm-tools"; then
        echo -e "${GREEN}✓ wasm-tools workload already installed${NC}"
        return 0
    else
        echo -e "${YELLOW}WASM workload not found${NC}"
        echo ""

        # Check if dedicated installer script exists
        if [ -f "./dotnet-wasm-install.sh" ]; then
            echo "Using dedicated installer script..."
            ./dotnet-wasm-install.sh
            return $?
        else
            # Fallback to direct installation (try net8 first, then fallback)
            echo -e "${YELLOW}Attempting direct installation...${NC}"
            dotnet workload install wasm-tools-net8 2>/dev/null || dotnet workload install wasm-tools
            if [ $? -eq 0 ]; then
                echo -e "${GREEN}✓ WASM workload installed${NC}"
                return 0
            else
                echo -e "${RED}✗ Failed to install WASM workload${NC}"
                echo "Try running: ./dotnet-wasm-install.sh"
                return 1
            fi
        fi
    fi
}

# Publish to WASM
build_wasm() {
    echo ""
    echo -e "${BLUE}Publishing PhantomNebula for WebAssembly...${NC}"
    echo "Configuration: $BUILD_CONFIG"
    echo ""

    # Important: Use 'dotnet publish' command, NOT Visual Studio
    # The RaylibWasm documentation explicitly warns against using VS publish feature
    if dotnet publish "$PROJECT_DIR" -c "$BUILD_CONFIG" -r browser-wasm; then
        echo -e "${GREEN}✓ WASM build successful${NC}"
        return 0
    else
        echo -e "${RED}✗ WASM build failed${NC}"
        return 1
    fi
}

# Install dotnet serve tool if not available
ensure_dotnet_serve() {
    if ! command -v dotnet &> /dev/null; then
        return 1
    fi

    if dotnet tool list -g 2>/dev/null | grep -q "dotnet-serve"; then
        return 0
    fi

    echo ""
    echo -e "${YELLOW}Installing dotnet serve tool...${NC}"

    if dotnet tool install -g dotnet-serve 2>/dev/null; then
        echo -e "${GREEN}✓ dotnet serve installed${NC}"
        return 0
    else
        echo -e "${YELLOW}⚠ Could not install dotnet serve (will fallback to other methods)${NC}"
        return 1
    fi
}

# Create web server script
create_web_server() {
    echo ""
    echo -e "${BLUE}Creating web server launch script...${NC}"

    mkdir -p "$OUTPUT_DIR"

    cat > "$OUTPUT_DIR/serve.sh" << 'EOF'
#!/bin/bash
# Web server for WASM build
# Supports multiple methods

# Get the directory where this script is located (bin/wasm/)
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
# Go up to project root: bin/wasm -> bin -> project root
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
# Build the WASM directory path from project root
WASM_DIR="$PROJECT_ROOT/PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle"

if [ ! -d "$WASM_DIR" ]; then
    echo "Error: WASM build not found"
    echo "Expected location: PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle/"
    echo "Run: ./build-wasm.sh first"
    exit 1
fi

cd "$WASM_DIR"

echo "Starting web server for Phantom Nebula WASM"
echo "Open browser to: http://localhost:8000"
echo ""

# Try dotnet serve first (best option)
if command -v dotnet serve &> /dev/null; then
    echo "Using: dotnet serve"
    dotnet serve -p 8000
# Fall back to Python 3
elif command -v python3 &> /dev/null; then
    echo "Using: Python 3 http.server"
    python3 -m http.server 8000
# Fall back to Python 2
elif command -v python &> /dev/null; then
    echo "Using: Python 2 SimpleHTTPServer"
    python -m SimpleHTTPServer 8000
# Fall back to Node.js
elif command -v npx &> /dev/null; then
    echo "Using: npx http-server"
    npx http-server -p 8000
else
    echo "Error: No suitable web server found"
    echo ""
    echo "Install one of the following:"
    echo "  • .NET: dotnet tool install -g dotnet-serve"
    echo "  • Python: https://www.python.org/downloads/"
    echo "  • Node.js: https://nodejs.org/en/download/"
    exit 1
fi
EOF

    chmod +x "$OUTPUT_DIR/serve.sh"
    echo -e "${GREEN}✓ Web server script created at: $OUTPUT_DIR/serve.sh${NC}"
}

# Show build information
show_info() {
    echo ""
    echo -e "${BLUE}═════════════════════════════════════════${NC}"
    echo -e "${BLUE}  WASM Build Information${NC}"
    echo -e "${BLUE}═════════════════════════════════════════${NC}"
    echo ""
    echo "Project: PhantomNebula"
    echo "Build Config: $BUILD_CONFIG"
    echo "Target Runtime: browser-wasm"
    echo "Output: PhantomNebula/bin/$BUILD_CONFIG/net8.0/browser-wasm/AppBundle/"
    echo ""
    echo "Next steps:"
    echo "  1. Build completed! WASM artifacts are ready."
    echo "  2. Run: $OUTPUT_DIR/serve.sh"
    echo "  3. Open: http://localhost:8000 in your browser"
    echo ""
    echo "Troubleshooting:"
    echo "  - Use 'dotnet publish' (NOT Visual Studio publish)"
    echo "  - .NET 8.0+ required for browser-wasm runtime"
    echo "  - wasm-tools workload must be installed"
    echo ""
}

# Main build process
main() {
    # Check .NET installation
    if ! check_dotnet; then
        exit 1
    fi

    # Install workload
    if ! install_wasm_workload; then
        exit 1
    fi

    # Build WASM
    if ! build_wasm; then
        exit 1
    fi

    # Ensure dotnet serve is available (optional, but convenient)
    ensure_dotnet_serve

    # Create web server script
    create_web_server

    # Show information
    show_info
}

main "$@"
