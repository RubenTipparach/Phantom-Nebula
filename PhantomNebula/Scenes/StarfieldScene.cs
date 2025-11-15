using System;
using System.Numerics;
using Raylib_cs;
using PhantomNebula.Core;

namespace PhantomNebula.Scenes;

/// <summary>
/// Starfield scene with ship, planet, and mouse-based camera control
/// Renders procedural starfield background with planets and ship entity
/// </summary>
public class StarfieldScene
{
    private BackgroundRenderer background;
    private Entity planet;
    private PlanetRenderer planetRenderer;
    private Ship ship;
    private CameraController cameraController;

    // Input state
    private float shipTargetSpeed = 0f;
    private Vector2 shipTargetHeading = Vector2.UnitY;
    private int cameraTarget = 0; // 0 = planet, 1 = ship

    public StarfieldScene()
    {
        // Initialize background renderer
        background = new BackgroundRenderer();

        // Create planet entity with transform - DOUBLED SIZE
        planet = new Entity(new Vector3(0, 0, 0), new Vector3(4.0f, 4.0f, 4.0f), "Planet");
        planetRenderer = new PlanetRenderer(planet.Transform.Position, 1.0f, 32);

        // Create ship
        ship = new Ship(new Vector3(5, 0, 0), 0.5f);

        // Create camera controller orbiting the planet
        cameraController = new CameraController(planet.Transform);

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
    }

    public void Draw()
    {
        // Get camera first
        var camera = cameraController.GetCamera();

        // Clear background
        Raylib.ClearBackground(Color.Black);

        // 3D mode with camera controller
        Raylib.BeginMode3D(camera);

        // Draw background
        background.Draw(camera);

        // Draw planet with lighting
        planetRenderer.Draw(camera);

        // Draw ship as a simple cube
        DrawShip();

        // Draw grid
        Raylib.DrawGrid(20, 1.0f);

        Raylib.EndMode3D();

        // Draw UI
        DrawUI();
    }

    private void DrawShip()
    {
        // Draw ship as colored cube
        Vector3 shipPos = ship.Transform.Position;
        Vector3 shipScale = ship.Transform.Scale;
        Raylib.DrawCubeWires(shipPos, shipScale.X * 2.0f, shipScale.Y, shipScale.Z * 3.0f, Color.Green);
        Raylib.DrawCube(shipPos, shipScale.X * 0.3f, shipScale.Y * 0.3f, shipScale.Z * 0.3f, Color.Lime);
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

        // Controls
        Raylib.DrawText("Controls:", 10, screenHeight - 120, 12, Color.Gray);
        Raylib.DrawText("WASD - Move/Turn Ship | Left Click+Drag - Orbit Camera | Scroll - Zoom", 10, screenHeight - 100, 12, Color.Gray);
        Raylib.DrawText("T - Toggle Camera Target | R - Reset Camera | ESC - Exit", 10, screenHeight - 80, 12, Color.Gray);
    }

    public void Dispose()
    {
        background.Dispose();
        planetRenderer.Dispose();
        planet.Dispose();
        ship.Dispose();
    }
}
