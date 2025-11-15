#!/bin/bash

# Build and run the PhantomNebula game (Raylib-based)

cd "$(dirname "$0")"

echo "Building PhantomNebula Game..."
dotnet build PhantomNebula/PhantomNebula.csproj

if [ $? -eq 0 ]; then
    echo "Build successful! Starting game..."
    cd PhantomNebula
    dotnet run
else
    echo "Build failed!"
    exit 1
fi
