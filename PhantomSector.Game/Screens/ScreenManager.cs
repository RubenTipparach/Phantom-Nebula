using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace PhantomSector.Game.Screens;

public class ScreenManager
{
    private readonly List<GameScreen> _screens = new();
    private readonly List<GameScreen> _screensToUpdate = new();

    public Game1 Game { get; private set; }
    public SpriteBatch SpriteBatch { get; private set; }
    public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;

    // Global UI assets
    public SpriteFont DefaultFont { get; private set; }
    public Texture2D WhiteTexture { get; private set; }

    public ScreenManager(Game1 game)
    {
        Game = game;
    }

    public void Initialize()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        System.Console.WriteLine("[ScreenManager] Initialized");
    }

    public void LoadGlobalAssets(SpriteFont font, Texture2D whiteTexture)
    {
        DefaultFont = font;
        WhiteTexture = whiteTexture;
        System.Console.WriteLine("[ScreenManager] Loaded global UI assets");
    }

    public void LoadContent()
    {
        foreach (var screen in _screens)
        {
            screen.LoadContent();
        }
    }

    public void UnloadContent()
    {
        foreach (var screen in _screens)
        {
            screen.UnloadContent();
        }
    }

    public void Update(GameTime gameTime)
    {
        _screensToUpdate.Clear();
        _screensToUpdate.AddRange(_screens);

        bool otherScreenHasFocus = !Game.IsActive;
        bool coveredByOtherScreen = false;

        // Update screens from top to bottom
        for (int i = _screensToUpdate.Count - 1; i >= 0; i--)
        {
            var screen = _screensToUpdate[i];

            screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active)
            {
                // Input only for top-most active screen
                if (!otherScreenHasFocus)
                {
                    screen.HandleInput(gameTime);
                    otherScreenHasFocus = true;
                }

                // If this is a popup, don't cover screens below it
                if (!screen.IsPopup)
                {
                    coveredByOtherScreen = true;
                }
            }
        }
    }

    public void Draw(GameTime gameTime)
    {
        // Draw screens from bottom to top
        foreach (var screen in _screens)
        {
            if (screen.ScreenState == ScreenState.Hidden)
                continue;

            screen.Draw(gameTime, SpriteBatch);
        }
    }

    public void AddScreen(GameScreen screen)
    {
        screen.SetScreenManager(this);
        screen.LoadContent();

        _screens.Add(screen);

        System.Console.WriteLine($"[ScreenManager] Added screen: {screen.Name}");
    }

    public void RemoveScreen(GameScreen screen)
    {
        screen.UnloadContent();
        _screens.Remove(screen);

        System.Console.WriteLine($"[ScreenManager] Removed screen: {screen.Name}");
    }

    public T GetScreen<T>() where T : GameScreen
    {
        return _screens.OfType<T>().FirstOrDefault();
    }

    public GameScreen GetScreen(string name)
    {
        return _screens.FirstOrDefault(s => s.Name == name);
    }

    public void ClearScreens()
    {
        foreach (var screen in _screens.ToList())
        {
            RemoveScreen(screen);
        }
    }

    /// <summary>
    /// Opens the pause menu as an overlay on the current screen
    /// </summary>
    public void ShowPauseMenu()
    {
        // Don't add pause menu if it's already open
        if (GetScreen<PauseMenuScreen>() != null)
            return;

        System.Console.WriteLine("[ScreenManager] Opening pause menu");
        AddScreen(new PauseMenuScreen());
    }

    /// <summary>
    /// Check if the pause menu is currently open
    /// </summary>
    public bool IsPaused()
    {
        return GetScreen<PauseMenuScreen>() != null;
    }
}
