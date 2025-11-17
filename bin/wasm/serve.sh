#!/bin/bash
# Web server for WASM build
# Supports multiple methods

PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
WASM_DIR="$PROJECT_ROOT/PhantomNebula/bin/Release/net8.0/browser-wasm/AppBundle"

if [ ! -d "$WASM_DIR" ]; then
    echo "Error: WASM build not found at $WASM_DIR"
    echo "Run: ./build-wasm.sh first"
    exit 1
fi

cd "$WASM_DIR"

echo "Starting web server for Phantom Nebula WASM"
echo "Open browser to: http://localhost:8000"
echo ""

# Try dotnet serve first (best option)
if command -v dotnet &> /dev/null; then
    echo "Using: dotnet serve"
    dotnet serve -p 8000
# Fall back to Python
elif command -v python3 &> /dev/null; then
    echo "Using: Python 3 http.server"
    python3 -m http.server 8000
elif command -v python &> /dev/null; then
    echo "Using: Python 2 SimpleHTTPServer"
    python -m SimpleHTTPServer 8000
# Fall back to Node.js
elif command -v npx &> /dev/null; then
    echo "Using: npx http-server"
    npx http-server -p 8000
else
    echo "Error: No suitable web server found"
    echo "Install one of: .NET (dotnet serve), Python, or Node.js (http-server)"
    exit 1
fi
