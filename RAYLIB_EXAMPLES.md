# raylib-cs Examples Runner

A convenient script to build and run the raylib-cs Examples project.

## Quick Start

### Interactive Menu Mode (Recommended)
Run the examples with an interactive menu:
```bash
./run-raylib-examples.sh
```

This will build the project and launch the Examples application with a menu where you can select which example to run.

### Run Specific Example
Run a specific example directly:
```bash
./run-raylib-examples.sh BasicLighting
```

### List Available Examples
View all available examples:
```bash
./run-raylib-examples.sh --list
# or
./run-raylib-examples.sh -l
```

## Usage Examples

**Run the Basic Lighting shader example:**
```bash
./run-raylib-examples.sh BasicLighting
```

**Run the Camera 3D Free camera example:**
```bash
./run-raylib-examples.sh Camera3dFree
```

**Run the Input Keys example:**
```bash
./run-raylib-examples.sh InputKeys
```

## Available Examples

### Core Examples
- `Camera2dPlatformer` - 2D platformer camera
- `Camera2dDemo` - 2D camera demo
- `Camera3dFirstPerson` - First-person 3D camera
- `Camera3dFree` - Free-flying 3D camera
- `Camera3dMode` - 3D camera modes
- `Picking3d` - 3D object picking
- `BasicWindow` - Basic window setup
- `InputKeys` - Keyboard input handling
- `InputMouse` - Mouse input handling
- `InputGamepad` - Gamepad input
- `RandomValues` - Random number generation
- And many more...

### Shapes Examples
- `ShapesBasicShapes` - Basic shape drawing
- `ShapesDrawRing` - Ring drawing

### Shaders Examples
- `BasicLighting` - Phong lighting shader ⭐ (Recommended!)
- `BloomEffect` - Bloom post-processing
- `PixelPerfect` - Pixel-perfect rendering

### Models Examples
- `ModelsCube` - Cube model
- `ModelsBox` - Box model

### Textures Examples
- Various texture loading and rendering examples

### Text Examples
- Font and text rendering examples

### Audio Examples
- Sound and music playback examples

## Project Structure

```
raylib-cs/
├── Examples/
│   ├── Program.cs          # Main entry point with example list
│   ├── Examples.csproj     # Project file
│   ├── Core/               # Core examples (camera, input, window, etc.)
│   ├── Shapes/             # Shape drawing examples
│   ├── Shaders/            # Shader examples (lighting, effects, etc.)
│   ├── Models/             # 3D model examples
│   ├── Textures/           # Texture loading examples
│   ├── Text/               # Text rendering examples
│   ├── Audio/              # Audio examples
│   ├── Shared/             # Shared utilities (Rlights.cs, etc.)
│   └── resources/          # Example assets (images, models, shaders, etc.)
```

## Recommended Examples to Explore

1. **BasicLighting** - Learn about shader-based lighting with Phong model
2. **Camera3dFree** - Understand 3D camera controls
3. **Picking3d** - Learn ray-picking for 3D object selection
4. **InputMouse** - Mouse input handling
5. **ShapesBasicShapes** - Basic 2D drawing

## Troubleshooting

**Script not found:**
```bash
chmod +x run-raylib-examples.sh
```

**Build failures:**
Make sure you're in the Phantom-Sector root directory and have .NET 8.0 installed.

**No examples appear in menu:**
The Examples.csproj file may need to be updated. Check `raylib-cs/Examples/Program.cs` for the current example list.

## Notes

- The interactive menu allows you to browse and select examples with arrow keys
- Examples can be closed by pressing ESC or closing the window
- Shader examples like BasicLighting demonstrate professional graphics techniques used in PhantomNebula
- The Rlights helper in `raylib-cs/Examples/Shared/Rlights.cs` is what PhantomNebula's planet lighting uses
