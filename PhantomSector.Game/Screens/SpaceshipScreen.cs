using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhantomSector.Game.Core;
using System;

namespace PhantomSector.Game.Screens;

public class SpaceshipScreen : GameScreen
{
    private OrbitCamera _camera;
    private BasicEffect _basicEffect;

    // Spaceship entity
    private Entity _shipEntity;

    // Wireframe cube (double size)
    private VertexPositionColor[] _wireframeCubeVertices;
    private short[] _wireframeCubeIndices;
    private BasicEffect _wireframeEffect;

    // Planet (sphere)
    private VertexPositionNormalTexture[] _sphereVertices;
    private int[] _sphereIndices;

    // Shaders
    private Effect _starfieldEffect;
    private Effect _planetEffect;
    private Effect _shipEffect;
    private Texture2D _shipEmissiveTexture;

    // Full-screen quad for starfield
    private VertexPositionTexture[] _fullScreenQuad;

    private float _time;
    private Vector3 _spaceshipPosition = new Vector3(10, 0, 0);

    public SpaceshipScreen() : base("Spaceship Scene")
    {
    }

    public override void LoadContent()
    {
        base.LoadContent();

        Console.WriteLine("[SpaceshipScreen] Loading content");

        // Initialize camera to orbit around the spaceship
        _camera = new OrbitCamera(Game.GraphicsDevice);
        _camera.Distance = 5f;
        _camera.SetTarget(_spaceshipPosition);

        // Initialize basic effect for spaceship
        _basicEffect = new BasicEffect(Game.GraphicsDevice);
        _basicEffect.VertexColorEnabled = true;
        _basicEffect.LightingEnabled = false;

        // Initialize wireframe effect
        _wireframeEffect = new BasicEffect(Game.GraphicsDevice);
        _wireframeEffect.VertexColorEnabled = true;
        _wireframeEffect.LightingEnabled = false;

        // Load shaders
        LoadShaders();

        // Load ship model
        LoadShipModel();

        // Create geometry
        CreateWireframeCube();
        CreatePlanet();
        CreateFullScreenQuad();

        Console.WriteLine("[SpaceshipScreen] Content loaded");
    }

    private void LoadShaders()
    {
        try
        {
            _starfieldEffect = Game.Content.Load<Effect>("Shaders/Starfield");
            Console.WriteLine("[SpaceshipScreen] Loaded Starfield shader");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SpaceshipScreen] Failed to load Starfield shader: {e.Message}");
        }

        try
        {
            _planetEffect = Game.Content.Load<Effect>("Shaders/PlanetNoise");
            Console.WriteLine("[SpaceshipScreen] Loaded PlanetNoise shader");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SpaceshipScreen] Failed to load PlanetNoise shader: {e.Message}");
        }

        try
        {
            _shipEffect = Game.Content.Load<Effect>("Shaders/ShipShader");
            Console.WriteLine("[SpaceshipScreen] Loaded ShipShader");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SpaceshipScreen] Failed to load ShipShader: {e.Message}");
        }

        try
        {
            _shipEmissiveTexture = Game.Content.Load<Texture2D>("Textures/shippy_em");
            Console.WriteLine("[SpaceshipScreen] Loaded ship emissive texture");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SpaceshipScreen] Failed to load ship emissive texture: {e.Message}");
        }
    }

    private void LoadShipModel()
    {
        try
        {
            _shipEntity = new Entity(Game.Content, "Models/shippy1");
            _shipEntity.Position = _spaceshipPosition;
            _shipEntity.Scale = 0.1f;

            // Set custom shader and emissive texture
            if (_shipEffect != null)
            {
                _shipEntity.SetCustomEffect(_shipEffect);
            }
            if (_shipEmissiveTexture != null)
            {
                _shipEntity.SetEmissiveTexture(_shipEmissiveTexture);
            }

            Console.WriteLine("[SpaceshipScreen] Loaded ship entity with custom shader");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SpaceshipScreen] Failed to load ship model: {e.Message}");
        }
    }


    private void CreateWireframeCube()
    {
        // Create a wireframe cube that is double the size (2f instead of 1f)
        float size = 2f;

        // Define cube vertices
        _wireframeCubeVertices = new VertexPositionColor[8];
        Color wireColor = Color.Cyan;

        _wireframeCubeVertices[0] = new VertexPositionColor(new Vector3(-size, -size, -size), wireColor);
        _wireframeCubeVertices[1] = new VertexPositionColor(new Vector3(size, -size, -size), wireColor);
        _wireframeCubeVertices[2] = new VertexPositionColor(new Vector3(size, size, -size), wireColor);
        _wireframeCubeVertices[3] = new VertexPositionColor(new Vector3(-size, size, -size), wireColor);
        _wireframeCubeVertices[4] = new VertexPositionColor(new Vector3(-size, -size, size), wireColor);
        _wireframeCubeVertices[5] = new VertexPositionColor(new Vector3(size, -size, size), wireColor);
        _wireframeCubeVertices[6] = new VertexPositionColor(new Vector3(size, size, size), wireColor);
        _wireframeCubeVertices[7] = new VertexPositionColor(new Vector3(-size, size, size), wireColor);

        // Define wireframe indices (lines, not triangles - 24 indices for 12 edges)
        _wireframeCubeIndices = new short[]
        {
            // Bottom face edges
            0, 1,  1, 2,  2, 3,  3, 0,
            // Top face edges
            4, 5,  5, 6,  6, 7,  7, 4,
            // Vertical edges
            0, 4,  1, 5,  2, 6,  3, 7
        };
    }

    private void CreatePlanet()
    {
        // Generate a sphere using the geometry generator
        var sphere = GeometryGenerator.CreateSphere(5f, 32, 32);
        _sphereVertices = sphere.Vertices;
        _sphereIndices = sphere.Indices;
    }

    private void CreateFullScreenQuad()
    {
        _fullScreenQuad = new VertexPositionTexture[4];
        _fullScreenQuad[0] = new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0));
        _fullScreenQuad[1] = new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0));
        _fullScreenQuad[2] = new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1));
        _fullScreenQuad[3] = new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1));
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (IsActive && !otherScreenHasFocus)
        {
            _time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update camera
            _camera.HandleInput(gameTime);
            _camera.Update(gameTime);
        }
    }

    public override void HandleInput(GameTime gameTime)
    {
        var keyState = Keyboard.GetState();

        // ESC to return to menu
        if (keyState.IsKeyDown(Keys.Escape))
        {
            ExitScreen();
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        Game.GraphicsDevice.Clear(Color.Black);

        // Draw starfield background
        DrawStarfield();

        // Set up 3D rendering states
        Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        Game.GraphicsDevice.BlendState = BlendState.Opaque;
        Game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        // Draw planet
        DrawPlanet();

        // Draw spaceship
        DrawSpaceship();

        // Draw wireframe cube
        DrawWireframeCube();

        // Draw UI
        spriteBatch.Begin();

        var instructions = "Right Mouse: Rotate Camera | Scroll: Zoom | ESC: Back to Menu";
        var textSize = ScreenManager.DefaultFont.MeasureString(instructions);
        var textPos = new Vector2(
            (Game.GraphicsDevice.Viewport.Width - textSize.X) / 2,
            Game.GraphicsDevice.Viewport.Height - 50
        );
        spriteBatch.DrawString(ScreenManager.DefaultFont, instructions, textPos, Color.White * TransitionAlpha);

        spriteBatch.End();
    }

    private void DrawStarfield()
    {
        if (_starfieldEffect == null) return;

        // Disable depth test for background
        Game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
        Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        // Get camera matrices
        Matrix view = _camera.View;
        Matrix projection = _camera.Projection;

        // Invert the view matrix to get the inverse rotation (transpose for rotation matrices)
        Matrix inverseView = Matrix.Invert(view);

        // Set shader parameters - pass inverse of view for ray direction calculation
        _starfieldEffect.Parameters["View"]?.SetValue(inverseView);
        _starfieldEffect.Parameters["Projection"]?.SetValue(Matrix.Invert(projection));

        // Draw full-screen quad
        foreach (var pass in _starfieldEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            Game.GraphicsDevice.DrawUserPrimitives(
                PrimitiveType.TriangleStrip,
                _fullScreenQuad,
                0,
                2
            );
        }
    }

    private void DrawPlanet()
    {
        if (_sphereVertices == null || _sphereIndices == null) return;

        Matrix world = Matrix.CreateTranslation(0, 0, 0);
        Matrix view = _camera.View;
        Matrix projection = _camera.Projection;

        if (_planetEffect != null)
        {
            // Use custom planet shader
            _planetEffect.Parameters["World"]?.SetValue(world);
            _planetEffect.Parameters["View"]?.SetValue(view);
            _planetEffect.Parameters["Projection"]?.SetValue(projection);
            _planetEffect.Parameters["Time"]?.SetValue(_time);
            _planetEffect.Parameters["CameraPosition"]?.SetValue(_camera.Position);

            // Set orbiting sun position
            float angle = _time * 0.1f;
            Vector3 sunPos = new Vector3(
                (float)System.Math.Cos(angle) * 100f,
                50f,
                (float)System.Math.Sin(angle) * 100f
            );
            _planetEffect.Parameters["SunPosition"]?.SetValue(sunPos);

            foreach (var pass in _planetEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _sphereVertices,
                    0,
                    _sphereVertices.Length,
                    _sphereIndices,
                    0,
                    _sphereIndices.Length / 3
                );
            }
        }
        else
        {
            // Fallback to basic effect
            var basicEffect = new BasicEffect(Game.GraphicsDevice);
            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.DiffuseColor = new Vector3(0.2f, 0.5f, 0.8f);
            basicEffect.EnableDefaultLighting();

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _sphereVertices,
                    0,
                    _sphereVertices.Length,
                    _sphereIndices,
                    0,
                    _sphereIndices.Length / 3
                );
            }
        }
    }

    private void DrawSpaceship()
    {
        _shipEntity?.Draw(_camera);
    }

    private void DrawWireframeCube()
    {
        if (_wireframeCubeVertices == null || _wireframeCubeIndices == null) return;

        // Position wireframe cube at the same location as spaceship
        Matrix world = Matrix.CreateTranslation(_spaceshipPosition);
        Matrix view = _camera.View;
        Matrix projection = _camera.Projection;

        _wireframeEffect.World = world;
        _wireframeEffect.View = view;
        _wireframeEffect.Projection = projection;

        foreach (var pass in _wireframeEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            Game.GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.LineList,
                _wireframeCubeVertices,
                0,
                _wireframeCubeVertices.Length,
                _wireframeCubeIndices,
                0,
                _wireframeCubeIndices.Length / 2
            );
        }
    }

    public override void UnloadContent()
    {
        _basicEffect?.Dispose();
        _wireframeEffect?.Dispose();
        _starfieldEffect?.Dispose();
        _planetEffect?.Dispose();

        base.UnloadContent();
    }
}
