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
