#!/bin/bash

################################################################################
# .NET WASM Tools Installation Script
#
# Installs the wasm-tools workload required for WebAssembly builds.
# This is a one-time setup requirement for WASM development.
#
# Usage:
#   ./dotnet-wasm-install.sh
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

echo -e "${BLUE}╔════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  .NET WASM Tools Installation         ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════╝${NC}"
echo ""

# Check if .NET is installed
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
        echo "  Install from: https://dotnet.microsoft.com/download"
        return 1
    fi
}

# Check if wasm-tools workload is already installed
check_existing_workload() {
    if dotnet workload list 2>/dev/null | grep -q "wasm-tools"; then
        echo -e "${GREEN}✓ wasm-tools workload already installed${NC}"
        return 0
    elif dotnet workload list 2>/dev/null | grep -q "wasm-tools-net8"; then
        echo -e "${GREEN}✓ wasm-tools-net8 workload already installed${NC}"
        return 0
    else
        return 1
    fi
}

# Attempt installation with elevated privileges if needed
install_wasm_workload() {
    echo ""
    echo -e "${BLUE}Installing wasm-tools-net8 workload...${NC}"
    echo ""

    # Try installation without sudo first
    if dotnet workload install wasm-tools-net8 2>/dev/null; then
        echo -e "${GREEN}✓ wasm-tools-net8 workload installed successfully${NC}"
        return 0
    else
        # If it fails, try with sudo
        echo -e "${YELLOW}Installation requires elevated privileges. Attempting with sudo...${NC}"
        echo ""

        if sudo dotnet workload install wasm-tools-net8; then
            echo ""
            echo -e "${GREEN}✓ wasm-tools-net8 workload installed successfully${NC}"
            return 0
        else
            echo -e "${RED}✗ Failed to install wasm-tools-net8 workload${NC}"
            return 1
        fi
    fi
}

# Verify installation
verify_installation() {
    echo ""
    echo -e "${BLUE}Verifying installation...${NC}"

    if dotnet workload list 2>/dev/null | grep -q "wasm-tools-net8"; then
        echo -e "${GREEN}✓ wasm-tools-net8 workload verified${NC}"
        return 0
    elif dotnet workload list 2>/dev/null | grep -q "wasm-tools"; then
        echo -e "${GREEN}✓ wasm-tools workload verified${NC}"
        return 0
    else
        echo -e "${RED}✗ Verification failed${NC}"
        return 1
    fi
}

# Show next steps
show_next_steps() {
    echo ""
    echo -e "${BLUE}═════════════════════════════════════════${NC}"
    echo -e "${BLUE}  Installation Complete!${NC}"
    echo -e "${BLUE}═════════════════════════════════════════${NC}"
    echo ""
    echo "Next steps to build WebAssembly:"
    echo "  1. Run build script:"
    echo "     $ ./build-wasm.sh"
    echo ""
    echo "  2. Start web server:"
    echo "     $ ./bin/wasm/serve.sh"
    echo ""
    echo "  3. Test in browser:"
    echo "     Visit http://localhost:8000"
    echo ""
}

# Show troubleshooting
show_troubleshooting() {
    echo ""
    echo -e "${YELLOW}Troubleshooting:${NC}"
    echo ""
    echo "If installation failed:"
    echo "  • Check that .NET 8.0 or higher is installed"
    echo "  • Try running with sudo: sudo dotnet workload install wasm-tools-net8"
    echo "  • Clear workload cache: dotnet workload repair"
    echo "  • Or try older workload: dotnet workload install wasm-tools"
    echo ""
    echo "For more info:"
    echo "  https://learn.microsoft.com/en-us/dotnet/core/runtime-config/wasm"
    echo ""
}

# Main installation process
main() {
    # Check .NET installation
    if ! check_dotnet; then
        echo ""
        echo -e "${RED}Please install .NET 8.0 or higher:${NC}"
        echo "  https://dotnet.microsoft.com/download"
        exit 1
    fi

    echo ""

    # Check if already installed
    if check_existing_workload; then
        echo ""
        echo -e "${GREEN}wasm-tools workload is ready for use!${NC}"
        show_next_steps
        exit 0
    fi

    # Install workload
    if ! install_wasm_workload; then
        show_troubleshooting
        exit 1
    fi

    # Verify installation
    if ! verify_installation; then
        show_troubleshooting
        exit 1
    fi

    # Show next steps
    show_next_steps
}

main "$@"
