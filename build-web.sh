#!/bin/bash

################################################################################
# Phantom Nebula Web Build Script
#
# Quick reference for building for web platforms
# Note: Direct Raylib-cs to WASM conversion is complex
# Recommended approach: Use a web-compatible graphics library
################################################################################

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo -e "${BLUE}  Phantom Nebula Web Build Options${NC}"
echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo ""

echo -e "${YELLOW}Current Setup:${NC}"
echo "  Project: PhantomNebula (.NET 8)"
echo "  Graphics: Raylib-cs"
echo "  Target: WebAssembly (WASM)"
echo ""

echo -e "${YELLOW}Available Build Options:${NC}"
echo ""
echo "1. ${GREEN}Native Desktop Build${NC} (Current)"
echo "   Command: dotnet build PhantomNebula -c Release"
echo "   Output:  bin/Release/PhantomNebula"
echo ""

echo "2. ${YELLOW}Blazor WebAssembly${NC} (Recommended for WASM)"
echo "   Pros:"
echo "     ✓ Runs in browser"
echo "     ✓ Direct .NET to WASM"
echo "     ✓ Full C# support"
echo "   Cons:"
echo "     ✗ Raylib-cs not supported - need to rewrite rendering"
echo "     ✗ Larger file size (~5-10MB+)"
echo "   "
echo "   Setup steps:"
echo "     1. dotnet new blazor -n PhantomNebula.Web"
echo "     2. Use Canvas/WebGL APIs for rendering"
echo "     3. Port game logic to Blazor components"
echo ""

echo "3. ${YELLOW}Emscripten Conversion${NC} (Experimental)"
echo "   Pros:"
echo "     ✓ Can convert existing C/C++ to WASM"
echo "   Cons:"
echo "     ✗ Raylib-cs (C#) doesn't compile with Emscripten"
echo "     ✗ Complex setup"
echo ""

echo "4. ${GREEN}Web-Native Rewrite${NC} (Best Performance)"
echo "   Alternatives to Raylib-cs for web:"
echo "     • Three.js (JavaScript/TypeScript)"
echo "     • Babylon.js (JavaScript/TypeScript)"
echo "     • PixiJS (2D rendering)"
echo "     • Canvas API (Native HTML5)"
echo "     • WebGL directly"
echo ""

echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo ""

# Show command examples
show_commands() {
    echo -e "${GREEN}Common Commands:${NC}"
    echo ""
    echo "Build Release (Desktop):"
    echo "  dotnet build PhantomNebula -c Release"
    echo ""
    echo "Build & Run:"
    echo "  dotnet run -c Release --project PhantomNebula"
    echo ""
    echo "Publish Self-Contained Executable:"
    echo "  dotnet publish PhantomNebula -c Release -o bin/publish --self-contained"
    echo ""
    echo "Create Standalone Distribution:"
    echo "  dotnet publish PhantomNebula -c Release -o bin/dist \\
    -p:PublishSingleFile=true \\
    -p:IncludeNativeLibrariesForSelfExtract=true"
    echo ""
}

# Create Blazor project template
create_blazor_project() {
    echo -e "${YELLOW}Creating Blazor Web App template...${NC}"
    echo ""
    echo "Run this to create a new Blazor project:"
    echo "  dotnet new blazor -n PhantomNebula.Web --interactivity Auto --empty"
    echo ""
    echo "Then copy game logic to:"
    echo "  Components/Game.razor"
    echo "  Components/GameCanvas.razor"
    echo ""
}

# Check for Emscripten
check_tools() {
    echo -e "${YELLOW}System Check:${NC}"
    echo ""

    if command -v dotnet &> /dev/null; then
        version=$(dotnet --version)
        echo -e "${GREEN}✓${NC} .NET: $version"
    else
        echo -e "${RED}✗${NC} .NET not found"
    fi

    if command -v node &> /dev/null; then
        version=$(node --version)
        echo -e "${GREEN}✓${NC} Node.js: $version (for web dev tools)"
    else
        echo -e "${YELLOW}○${NC} Node.js not found (optional for web tools)"
    fi

    if command -v emcc &> /dev/null; then
        version=$(emcc --version | head -1)
        echo -e "${GREEN}✓${NC} Emscripten: $version"
    else
        echo -e "${YELLOW}○${NC} Emscripten not installed (optional)"
    fi

    echo ""
}

# Menu system
show_menu() {
    echo -e "${YELLOW}What would you like to do?${NC}"
    echo ""
    echo "1) Build native desktop executable"
    echo "2) Show Blazor setup instructions"
    echo "3) Show common build commands"
    echo "4) Check available tools"
    echo "5) Exit"
    echo ""
    read -p "Select option (1-5): " choice

    case $choice in
        1)
            echo ""
            echo -e "${BLUE}Building native executable...${NC}"
            dotnet build PhantomNebula -c Release
            echo -e "${GREEN}✓ Build complete!${NC}"
            echo "Output: PhantomNebula/bin/Release/net8.0/PhantomNebula"
            ;;
        2)
            create_blazor_project
            ;;
        3)
            show_commands
            ;;
        4)
            check_tools
            ;;
        5)
            echo "Exiting..."
            exit 0
            ;;
        *)
            echo -e "${RED}Invalid option${NC}"
            ;;
    esac

    echo ""
    read -p "Press Enter to continue..."
    clear
    show_menu
}

# Main
main() {
    # If argument provided, execute that command
    if [ $# -gt 0 ]; then
        case "$1" in
            build)
                dotnet build PhantomNebula -c Release
                ;;
            blazor)
                create_blazor_project
                ;;
            commands)
                show_commands
                ;;
            check)
                check_tools
                ;;
            *)
                echo "Unknown command: $1"
                echo "Usage: ./build-web.sh [build|blazor|commands|check|menu]"
                exit 1
                ;;
        esac
    else
        show_menu
    fi
}

main "$@"
