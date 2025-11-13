#!/bin/bash

# Build and run the PhantomSector game

cd "$(dirname "$0")"

echo "Building PhantomSector Game..."
dotnet build PhantomSector.Game/PhantomSector.Game.csproj

if [ $? -eq 0 ]; then
    echo "Build successful! Starting game..."
    dotnet run --project PhantomSector.Game/PhantomSector.Game.csproj
else
    echo "Build failed!"
    exit 1
fi
