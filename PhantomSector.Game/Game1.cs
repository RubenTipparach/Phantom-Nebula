using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhantomSector.Game.Screens;

namespace PhantomSector.Game;

public class Game1 : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private ScreenManager _screenManager;
    private GameConfig _config;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
    }

    protected override void Initialize()
    {
        System.Console.WriteLine("=== PhantomSector - Debug Mode ===");
        System.Console.WriteLine("Game initialized successfully");

        // Load config
        _config = GameConfig.Load();

        // Initialize screen manager
        _screenManager = new ScreenManager(this);
        _screenManager.Initialize();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Load global assets for UI
        var font = Content.Load<SpriteFont>("Fonts\\ntr");
        var whiteTexture = Content.Load<Texture2D>("Textures\\white");

        _screenManager.LoadGlobalAssets(font, whiteTexture);
        _screenManager.LoadContent();

        // Load startup scene based on config
        LoadStartupScene(_config.StartupScene);
    }

    private void LoadStartupScene(string sceneName)
    {
        System.Console.WriteLine($"[Game] Loading startup scene: '{sceneName}'");

        switch (sceneName.ToLower())
        {
            case "menu":
                _screenManager.AddScreen(new MenuScreen());
                break;

            case "spaceship":
            case "spaceshipscene":
                _screenManager.AddScreen(new SpaceshipScreen());
                break;

            case "physics":
            case "physicsdemo":
                _screenManager.AddScreen(new PhysicsDemoScreen());
                break;

            default:
                System.Console.WriteLine($"[Game] Unknown scene '{sceneName}', defaulting to menu");
                _screenManager.AddScreen(new MenuScreen());
                break;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        _screenManager.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _screenManager.Draw(gameTime);
        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        _screenManager.UnloadContent();
        base.UnloadContent();
    }
}
