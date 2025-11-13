using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using PhantomSector.Game.UI;

namespace PhantomSector.Game.Screens;

/// <summary>
/// Universal pause menu that overlays any screen
/// </summary>
public class PauseMenuScreen : GameScreen
{
    private readonly List<Button> _buttons = new();
    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;

    public PauseMenuScreen() : base("Pause Menu")
    {
        IsPopup = true; // Don't hide the screen below
        TransitionOnTime = 0.2f;
        TransitionOffTime = 0.2f;
    }

    public override void LoadContent()
    {
        base.LoadContent();

        // Initialize input states to current state to avoid detecting keys already held down
        _previousKeyboardState = Keyboard.GetState();
        _previousMouseState = Mouse.GetState();

        System.Console.WriteLine("[PauseMenu] Loading content");

        // Resume button
        var resumeButton = new Button("Resume", ScreenManager.DefaultFont, ScreenManager.WhiteTexture);
        resumeButton.OnClick += (sender, args) =>
        {
            System.Console.WriteLine("[PauseMenu] Resume");
            ExitScreen();
        };
        resumeButton.SetPosition(new Vector2(100, 250));
        _buttons.Add(resumeButton);

        // Return to Menu button
        var menuButton = new Button("Return to Menu", ScreenManager.DefaultFont, ScreenManager.WhiteTexture);
        menuButton.OnClick += (sender, args) =>
        {
            System.Console.WriteLine("[PauseMenu] Return to menu");
            // Exit pause menu and all other screens, then add menu
            ScreenManager.ClearScreens();
            ScreenManager.AddScreen(new MenuScreen());
        };
        menuButton.SetPosition(new Vector2(100, 340));
        _buttons.Add(menuButton);

        // Exit button
        var exitButton = new Button("Exit Game", ScreenManager.DefaultFont, ScreenManager.WhiteTexture);
        exitButton.OnClick += (sender, args) =>
        {
            System.Console.WriteLine("[PauseMenu] Exit game");
            Game.Exit();
        };
        exitButton.SetPosition(new Vector2(100, 430));
        _buttons.Add(exitButton);

        System.Console.WriteLine($"[PauseMenu] Created {_buttons.Count} buttons");
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (IsActive && !otherScreenHasFocus)
        {
            var mouseState = Mouse.GetState();

            // Update all buttons
            foreach (var button in _buttons)
            {
                button.Update(gameTime, mouseState, _previousMouseState);
            }

            _previousMouseState = mouseState;
        }
    }

    public override void HandleInput(GameTime gameTime)
    {
        var keyState = Keyboard.GetState();

        // ESC to resume
        if (keyState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
        {
            ExitScreen();
        }

        _previousKeyboardState = keyState;
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var graphicsDevice = ScreenManager.GraphicsDevice;

        spriteBatch.Begin();

        // Dark overlay to dim the screen below
        var overlay = new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
        spriteBatch.Draw(ScreenManager.WhiteTexture, overlay, Color.Black * 0.5f * TransitionAlpha);

        // Draw title
        var titleText = "PAUSED";
        var titleSize = ScreenManager.DefaultFont.MeasureString(titleText);
        var titlePos = new Vector2(
            (graphicsDevice.Viewport.Width - titleSize.X) / 2,
            150
        );
        spriteBatch.DrawString(ScreenManager.DefaultFont, titleText, titlePos, Color.White * TransitionAlpha);

        // Draw buttons
        foreach (var button in _buttons)
        {
            button.Draw(spriteBatch, TransitionAlpha);
        }

        // Draw instructions
        var instructions = "ESC to resume";
        var instSize = ScreenManager.DefaultFont.MeasureString(instructions);
        var instPos = new Vector2(
            (graphicsDevice.Viewport.Width - instSize.X) / 2,
            graphicsDevice.Viewport.Height - 50
        );
        spriteBatch.DrawString(ScreenManager.DefaultFont, instructions, instPos, Color.Gray * TransitionAlpha);

        spriteBatch.End();
    }
}
