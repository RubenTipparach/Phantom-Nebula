using System;
using System.Numerics;
using Raylib_cs;
using PhantomNebula.Core;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Scenes;

/// <summary>
/// Starfield scene with ship, planet, and mouse-based camera control
/// Renders procedural starfield background with planets and ship entity
/// </summary>
public class StarfieldScene
{
    private BackgroundRenderer background;
    private TestMesh testMesh;
    private Entity planet;
    private PlanetRenderer planetRenderer;
    private Ship ship;
    private ShipRenderer shipRenderer;
    private CameraController cameraController;
    private GifRecorder gifRecorder;
    private RenderTexture2D sceneTexture;
    private Vector3 lightPosition = new Vector3(100.0f, 50.0f, 0.0f);

    // Input state
    private float shipTargetSpeed = 0f;
    private Vector2 shipTargetHeading = Vector2.UnitY;
    private int cameraTarget = 0; // 0 = planet, 1 = ship

    public StarfieldScene()
    {
        // Initialize background renderer with starfield shader
        background = new BackgroundRenderer();

        // Initialize test mesh (red cube)
        testMesh = new TestMesh();

        // Create planet entity with transform - DOUBLED SIZE
        planet = new Entity(new Vector3(0, 0, 0), new Vector3(4.0f, 4.0f, 4.0f), "Planet");
        planetRenderer = new PlanetRenderer(planet.Transform.Position, 1.0f, 32);

        // Create ship
        ship = new Ship(new Vector3(5, 0, 0), 0.5f);

        // Create ship renderer
        shipRenderer = new ShipRenderer(ship.Transform.Position, 0.5f);

        // Create camera controller orbiting the ship (not the planet)
        cameraController = new CameraController(ship.Transform);

        // Initialize GIF recorder
        gifRecorder = new GifRecorder();

        // Create render texture for GIF capture (1280x720)
        sceneTexture = Raylib.LoadRenderTexture(1280, 720);

        Console.WriteLine("[StarfieldScene] Initialized with Voronoi starfield shader, 1 planet, 1 ship");
    }

    public void Update(float deltaTime)
    {
        HandleInput();

        // Update entities
        planet.Update(deltaTime);
        ship.Update(deltaTime);

        // Update camera controller
        cameraController.Update(deltaTime);
    }

    private void HandleInput()
    {
        // WASD controls for ship movement
        if (Raylib.IsKeyDown(KeyboardKey.W))
        {
            shipTargetSpeed = 1.0f;
        }
        else if (Raylib.IsKeyDown(KeyboardKey.S))
        {
            shipTargetSpeed = -0.5f;
        }
        else
        {
            shipTargetSpeed = 0f;
        }

        // A/D for ship rotation
        if (Raylib.IsKeyDown(KeyboardKey.A))
        {
            shipTargetHeading = new Vector2(
                (float)Math.Cos(Math.PI / 2),
                (float)Math.Sin(Math.PI / 2)
            );
        }
        else if (Raylib.IsKeyDown(KeyboardKey.D))
        {
            shipTargetHeading = new Vector2(
                (float)Math.Cos(-Math.PI / 2),
                (float)Math.Sin(-Math.PI / 2)
            );
        }
        else
        {
            shipTargetHeading = Vector2.UnitY;
        }

        ship.SetTargetSpeed(shipTargetSpeed);
        ship.SetHeading(shipTargetHeading);

        // T key to switch camera target
        if (Raylib.IsKeyPressed(KeyboardKey.T))
        {
            cameraTarget = (cameraTarget + 1) % 2;
            if (cameraTarget == 0)
            {
                cameraController.SetTarget(planet.Transform);
            }
            else
            {
                cameraController.SetTarget(ship.Transform);
            }
        }

        // R key to reset camera
        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            cameraController.Reset();
            Console.WriteLine("[Camera] Reset to default position");
        }

        // Ctrl+8 to start GIF recording
        if (Raylib.IsKeyPressed(KeyboardKey.Eight) && Raylib.IsKeyDown(KeyboardKey.LeftControl))
        {
            gifRecorder.StartRecording();
        }

        // Ctrl+9 to stop GIF recording
        if (Raylib.IsKeyPressed(KeyboardKey.Nine) && Raylib.IsKeyDown(KeyboardKey.LeftControl))
        {
            gifRecorder.StopRecording();
        }
    }

    public void Draw()
    {
        // Get camera first
        var camera = cameraController.GetCamera();

        // Render to texture for GIF capture
        Raylib.BeginTextureMode(sceneTexture);
        {
            // Clear background
            Raylib.ClearBackground(Color.Black);

            // 3D mode with camera controller
            Raylib.BeginMode3D(camera);

            // Draw background starfield
            background.Draw(camera);

            // Draw planet with procedural shader
            planetRenderer.Draw(camera);

            // Draw test mesh (red cube) - offset from planet
            // testMesh.Draw(new Vector3(3.0f, 0, 0));

            // Draw ship with model and shader
            shipRenderer.Draw(camera, lightPosition);

            Raylib.EndMode3D();

            // Draw UI
            DrawUI();
        }
        Raylib.EndTextureMode();

        // Draw texture to screen
        Raylib.DrawTextureRec(sceneTexture.Texture,
            new Rectangle(0, 0, sceneTexture.Texture.Width, -sceneTexture.Texture.Height),
            Vector2.Zero, Color.White);

        // Capture frame for GIF recording (from texture)
        if (gifRecorder.IsRecording)
        {
            gifRecorder.CaptureFrameFromTexture(sceneTexture);
        }
    }

    private void DrawUI()
    {
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        // Title
        Raylib.DrawText("PHANTOM NEBULA", 10, 10, 20, Color.White);

        // Ship stats
        Vector3 shipPos = ship.Transform.Position;
        Raylib.DrawText($"Ship Position: {shipPos.X:F2}, {shipPos.Y:F2}, {shipPos.Z:F2}", 10, 40, 12, Color.White);
        Raylib.DrawText($"Ship Speed: {ship.Systems.Speed:F2}", 10, 60, 12, Color.White);
        Raylib.DrawText($"Ship Health: {ship.Health.Percent:P0}", 10, 80, 12, Color.White);

        // Planet stats
        Raylib.DrawText($"Planet Position: {planet.Transform.Position.X:F2}, {planet.Transform.Position.Y:F2}, {planet.Transform.Position.Z:F2}", 10, 100, 12, Color.White);

        // Camera info
        string targetName = cameraTarget == 0 ? "Planet" : "Ship";
        Raylib.DrawText($"Camera Target: {targetName}", 10, 140, 12, Color.Yellow);
        Raylib.DrawText($"Orbit Distance: {cameraController.OrbitDistance:F2}", 10, 160, 12, Color.Yellow);

        // GIF Recording/Saving status
        if (gifRecorder.IsRecording)
        {
            Raylib.DrawText($"REC - {gifRecorder.FrameCount} frames", screenWidth - 200, 10, 14, Color.Red);
        }
        else if (gifRecorder.IsSaving)
        {
            Raylib.DrawText("SAVING...", screenWidth - 200, 10, 14, Color.Orange);
        }

        // Controls
        Raylib.DrawText("Controls:", 10, screenHeight - 140, 12, Color.Gray);
        Raylib.DrawText("WASD - Move/Turn Ship | Left Click+Drag - Orbit Camera | Scroll - Zoom", 10, screenHeight - 120, 12, Color.Gray);
        Raylib.DrawText("T - Toggle Camera Target | R - Reset Camera | Ctrl+8 - Record | Ctrl+9 - Stop", 10, screenHeight - 100, 12, Color.Gray);
        Raylib.DrawText("ESC - Exit", 10, screenHeight - 80, 12, Color.Gray);
    }

    public void Dispose()
    {
        background.Dispose();
        testMesh.Dispose();
        planetRenderer.Dispose();
        planet.Dispose();
        ship.Dispose();
        gifRecorder.Dispose();
        Raylib.UnloadRenderTexture(sceneTexture);
    }
}
