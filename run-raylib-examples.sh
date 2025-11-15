#!/bin/bash

# raylib-cs Examples Runner Script
# This script builds and runs the raylib-cs Examples project with optional example selection

set -e

RAYLIB_EXAMPLES_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/raylib-cs/Examples"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Print header
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}raylib-cs Examples Runner${NC}"
echo -e "${BLUE}========================================${NC}"

# Check if Examples directory exists
if [ ! -d "$RAYLIB_EXAMPLES_DIR" ]; then
    echo -e "${RED}Error: Examples directory not found at $RAYLIB_EXAMPLES_DIR${NC}"
    exit 1
fi

cd "$RAYLIB_EXAMPLES_DIR"

# Show available examples if requested
if [ "$1" == "--list" ] || [ "$1" == "-l" ]; then
    echo -e "${YELLOW}Available examples:${NC}"
    echo "Run with: $0 <example_name>"
    echo ""
    echo -e "${GREEN}Core examples:${NC}"
    echo "  - Camera2dPlatformer"
    echo "  - Camera2dDemo"
    echo "  - Camera3dFirstPerson"
    echo "  - Camera3dFree"
    echo "  - Camera3dMode"
    echo "  - Picking3d"
    echo "  - BasicWindow"
    echo "  - InputKeys"
    echo "  - InputMouse"
    echo ""
    echo -e "${GREEN}Shapes examples:${NC}"
    echo "  - ShapesBasicShapes"
    echo "  - ShapesDrawRing"
    echo ""
    echo -e "${GREEN}Shaders examples:${NC}"
    echo "  - BasicLighting"
    echo "  - BloomEffect"
    echo "  - PixelPerfect"
    echo ""
    echo -e "${GREEN}Models examples:${NC}"
    echo "  - ModelsCube"
    echo "  - ModelsBox"
    echo ""
    echo -e "${GREEN}Run interactive menu:${NC}"
    echo "  - (no arguments)"
    exit 0
fi

# If example name is provided, run it directly
if [ -n "$1" ]; then
    echo -e "${YELLOW}Building Examples project...${NC}"
    dotnet build 2>&1 | tail -3

    echo -e "${YELLOW}Running example: $1${NC}"
    dotnet run -- "$1"
    exit 0
fi

# Interactive menu mode (default)
echo -e "${YELLOW}Building Examples project...${NC}"
dotnet build 2>&1 | tail -3

echo ""
echo -e "${GREEN}Examples loaded! The application will show an interactive menu.${NC}"
echo -e "${GREEN}Select an example from the menu to run.${NC}"
echo ""

# Run the interactive examples
dotnet run
