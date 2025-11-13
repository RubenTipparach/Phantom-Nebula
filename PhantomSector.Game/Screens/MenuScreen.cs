using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using PhantomSector.Game.UI;

namespace PhantomSector.Game.Screens;

public class MenuScreen : GameScreen
{
    private readonly List<Button> _buttons = new();
    private MouseState _previousMouseState;

    public MenuScreen() : base("Main Menu")
    {
    }

    public override void LoadContent()
    {
        base.LoadContent();

        System.Console.WriteLine("[MenuScreen] Loading content");

        // Create buttons using global assets
        var physicsButton = new Button("Physics Demo", ScreenManager.DefaultFont, ScreenManager.WhiteTexture);
        physicsButton.OnClick += (sender, args) =>
        {
            System.Console.WriteLine("[Menu] Starting Physics Demo");
            ScreenManager.AddScreen(new PhysicsDemoScreen());
        };
        physicsButton.SetPosition(new Vector2(100, 200));
        _buttons.Add(physicsButton);

        var exitButton = new Button("Exit", ScreenManager.DefaultFont, ScreenManager.WhiteTexture);
        exitButton.OnClick += (sender, args) =>
        {
            System.Console.WriteLine("[Menu] Exiting game");
            Game.Exit();
        };
        exitButton.SetPosition(new Vector2(100, 290));
        _buttons.Add(exitButton);

        System.Console.WriteLine($"[MenuScreen] Created {_buttons.Count} buttons");
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

        // ESC to exit
        if (keyState.IsKeyDown(Keys.Escape))
        {
            Game.Exit();
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        Game.GraphicsDevice.Clear(new Color(100, 149, 237)); // CornflowerBlue background

        spriteBatch.Begin();

        // Draw title
        var titleText = "FALLEN TRIBES - MAIN MENU";
        var titleSize = ScreenManager.DefaultFont.MeasureString(titleText);
        var titlePos = new Vector2(
            (Game.GraphicsDevice.Viewport.Width - titleSize.X) / 2,
            100
        );
        spriteBatch.DrawString(ScreenManager.DefaultFont, titleText, titlePos, Color.White * TransitionAlpha);

        // Draw buttons
        foreach (var button in _buttons)
        {
            button.Draw(spriteBatch, TransitionAlpha);
        }

        // Draw instructions
        var instructions = "Hover and click buttons with mouse, ESC to exit";
        var instSize = ScreenManager.DefaultFont.MeasureString(instructions);
        var instPos = new Vector2(
            (Game.GraphicsDevice.Viewport.Width - instSize.X) / 2,
            Game.GraphicsDevice.Viewport.Height - 50
        );
        spriteBatch.DrawString(ScreenManager.DefaultFont, instructions, instPos, Color.Gray * TransitionAlpha);

        spriteBatch.End();
    }
}
