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

    // Spaceship (cube)
    private VertexPositionColor[] _cubeVertices;
    private short[] _cubeIndices;

    // Planet (sphere)
    private VertexPositionNormalTexture[] _sphereVertices;
    private int[] _sphereIndices;

    // Shaders
    private Effect _starfieldEffect;
    private Effect _planetEffect;

    // Full-screen quad for starfield
    private VertexPositionTexture[] _fullScreenQuad;

    private float _time;

    public SpaceshipScreen() : base("Spaceship Scene")
    {
    }

    public override void LoadContent()
    {
        base.LoadContent();

        Console.WriteLine("[SpaceshipScreen] Loading content");

        // Initialize camera
        _camera = new OrbitCamera(Game.GraphicsDevice);
        _camera.Distance = 20f;
        // Target is set internally by OrbitCamera

        // Initialize basic effect for spaceship
        _basicEffect = new BasicEffect(Game.GraphicsDevice);
        _basicEffect.VertexColorEnabled = true;
        _basicEffect.LightingEnabled = false;

        // Load shaders
        LoadShaders();

        // Create geometry
        CreateSpaceship();
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
    }

    private void CreateSpaceship()
    {
        // Create a simple cube for the spaceship
        _cubeVertices = new VertexPositionColor[8];
        float size = 1f;

        // Define cube vertices with colors
        _cubeVertices[0] = new VertexPositionColor(new Vector3(-size, -size, -size), Color.Red);
        _cubeVertices[1] = new VertexPositionColor(new Vector3(size, -size, -size), Color.Green);
        _cubeVertices[2] = new VertexPositionColor(new Vector3(size, size, -size), Color.Blue);
        _cubeVertices[3] = new VertexPositionColor(new Vector3(-size, size, -size), Color.Yellow);
        _cubeVertices[4] = new VertexPositionColor(new Vector3(-size, -size, size), Color.Cyan);
        _cubeVertices[5] = new VertexPositionColor(new Vector3(size, -size, size), Color.Magenta);
        _cubeVertices[6] = new VertexPositionColor(new Vector3(size, size, size), Color.White);
        _cubeVertices[7] = new VertexPositionColor(new Vector3(-size, size, size), Color.Orange);

        // Define cube indices (36 indices for 12 triangles, 6 faces)
        _cubeIndices = new short[]
        {
            // Front face
            0, 1, 2, 0, 2, 3,
            // Back face
            4, 6, 5, 4, 7, 6,
            // Left face
            0, 3, 7, 0, 7, 4,
            // Right face
            1, 5, 6, 1, 6, 2,
            // Top face
            3, 2, 6, 3, 6, 7,
            // Bottom face
            0, 4, 5, 0, 5, 1
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

        // Calculate inverse view-projection matrix for ray reconstruction
        Matrix view = _camera.View;
        Matrix projection = _camera.Projection;
        Matrix viewProjection = view * projection;
        Matrix inverseViewProjection = Matrix.Invert(viewProjection);

        // Set shader parameters
        _starfieldEffect.Parameters["InverseViewProjection"]?.SetValue(inverseViewProjection);
        _starfieldEffect.Parameters["CameraPosition"]?.SetValue(_camera.Position);

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
        if (_cubeVertices == null || _cubeIndices == null) return;

        // Position spaceship offset from planet
        Matrix world = Matrix.CreateTranslation(10, 0, 0);
        Matrix view = _camera.View;
        Matrix projection = _camera.Projection;

        _basicEffect.World = world;
        _basicEffect.View = view;
        _basicEffect.Projection = projection;

        foreach (var pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            Game.GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                _cubeVertices,
                0,
                _cubeVertices.Length,
                _cubeIndices,
                0,
                _cubeIndices.Length / 3
            );
        }
    }

    public override void UnloadContent()
    {
        _basicEffect?.Dispose();
        _starfieldEffect?.Dispose();
        _planetEffect?.Dispose();

        base.UnloadContent();
    }
}
