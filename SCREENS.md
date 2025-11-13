# Screen Management System

This project uses a screen/scene management system inspired by the FNA Starter Kit pattern, allowing for easy management of game states, menus, and scenes.

## Architecture

### Core Components

- **GameScreen** - Abstract base class for all screens/scenes
- **ScreenManager** - Manages the stack of screens, handles updates, drawing, and transitions
- **Game1** - Main game class that hosts the ScreenManager

### Screen Lifecycle

1. **TransitionOn** - Screen is fading in
2. **Active** - Screen is fully visible and accepting input
3. **TransitionOff** - Screen is fading out
4. **Hidden** - Screen is not visible (covered by another screen)

## Configuration

Edit `game_config.json` to control which scene launches on startup:

```json
{
  "StartupScene": "menu"
}
```

Available scenes:
- `"menu"` - Main menu (default)
- `"physics"` or `"physicsdemo"` - Physics demo scene

## Creating a New Screen

1. Create a new class in `Screens/` folder
2. Extend `GameScreen` base class
3. Implement required methods:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhantomSector.Game.Screens;

public class MyNewScreen : GameScreen
{
    public MyNewScreen() : base("My Screen Name")
    {
        // Optional: Configure transitions
        TransitionOnTime = 0.5f;
        TransitionOffTime = 0.5f;

        // Optional: Mark as popup (won't cover screens below)
        IsPopup = false;
    }

    public override void LoadContent()
    {
        // Load resources here
        System.Console.WriteLine("[MyScreen] Loading content");
    }

    public override void UnloadContent()
    {
        // Clean up resources here
        System.Console.WriteLine("[MyScreen] Unloading content");
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        // Your update logic here
    }

    public override void HandleInput(GameTime gameTime)
    {
        var keyState = Keyboard.GetState();

        // Handle input here

        // Exit screen (return to previous screen)
        if (keyState.IsKeyDown(Keys.Escape))
        {
            ExitScreen();
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        Game.GraphicsDevice.Clear(Color.Black);

        // Your drawing code here
    }
}
```

## Adding Screen to Game1

Update the `LoadStartupScene` method in [Game1.cs](PhantomSector.Game/Game1.cs):

```csharp
case "mynewscreen":
    _screenManager.AddScreen(new MyNewScreen());
    break;
```

## Screen Transitions

Screens automatically transition in/out based on their state:

- **TransitionPosition**: 0.0 (fully on) to 1.0 (fully off)
- Use `TransitionPosition` in your Draw method for fade effects:

```csharp
var alpha = 1.0f - TransitionPosition;
spriteBatch.Draw(texture, position, Color.White * alpha);
```

## Screen Stack

The ScreenManager maintains a stack of screens:

- Screens are drawn from bottom to top
- Input is only handled by the topmost active screen
- Non-popup screens cover screens below them

### Example Usage

```csharp
// Add a new screen on top
ScreenManager.AddScreen(new PhysicsDemoScreen());

// Return to previous screen
ExitScreen();

// Access ScreenManager from any screen
ScreenManager.GetScreen<MenuScreen>();
ScreenManager.GetScreen("Main Menu");
```

## Existing Screens

### MenuScreen
- Main menu with scene selection
- Navigate with UP/DOWN arrow keys
- SELECT with ENTER
- ESC to exit game

### PhysicsDemoScreen
- Physics simulation with BepuPhysics2
- Pyramid of cubes
- Left-click to shoot spheres
- Right-click + drag to rotate camera
- Mouse wheel to zoom
- ESC to return to menu

## Debug Output

The game runs with console output enabled. You'll see:

- Screen additions/removals
- Config loading
- FPS and physics stats (in PhysicsDemoScreen)
- User actions

## Tips

1. **Pause Menus**: Create a popup screen with `IsPopup = true` to overlay on top of game screens
2. **Loading Screens**: Use short transition times and load content during TransitionOn state
3. **Resource Management**: Clean up in `UnloadContent()` to prevent memory leaks
4. **Testing**: Change `StartupScene` in config to jump directly to your screen during development
