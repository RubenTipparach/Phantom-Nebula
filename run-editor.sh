#!/bin/bash

# Build and run the PhantomSector editor

cd "$(dirname "$0")"

echo "Building PhantomSector Editor..."
dotnet build PhantomSector.Editor/PhantomSector.Editor.csproj

if [ $? -eq 0 ]; then
    echo "Build successful! Starting editor..."
    dotnet run --project PhantomSector.Editor/PhantomSector.Editor.csproj
else
    echo "Build failed!"
    exit 1
fi
