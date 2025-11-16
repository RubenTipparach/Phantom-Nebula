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
    private CameraController cameraController;
    private GifRecorder gifRecorder;
    private RenderTexture2D sceneTexture;

    // Directional light - loaded from config
    private Vector3 lightDirection;

    // Input state
    private float shipTargetSpeed = 0f;
    private Vector2 shipTargetHeading = Vector2.UnitY;
    private int cameraTarget = 0; // 0 = planet, 1 = ship

    // Mouse raycast
    private Vector3? mouseWorldPosition = null;

    public StarfieldScene()
    {
        // Load configuration
        var config = GameConfig.Instance;

        // Set light direction from config
        lightDirection = Vector3.Normalize(new Vector3(
            config.LightDirectionX,
            config.LightDirectionY,
            config.LightDirectionZ
        ));

        // Initialize background renderer with starfield shader
        background = new BackgroundRenderer();

        // Initialize test mesh (red cube)
        testMesh = new TestMesh();

        // Create planet entity with transform from config
        planet = new Entity(
            new Vector3(config.PlanetPositionX, config.PlanetPositionY, config.PlanetPositionZ),
            new Vector3(config.PlanetScale, config.PlanetScale, config.PlanetScale),
            "Planet"
        );
        planetRenderer = new PlanetRenderer(planet.Transform.Position, 50.0f, 32);

        // Create ship from config (includes renderer)
        ship = new Ship(
            new Vector3(config.ShipPositionX, config.ShipPositionY, config.ShipPositionZ),
            config.ShipScale
        );

        // Create camera controller orbiting the ship (not the planet)
        cameraController = new CameraController(ship.Transform);

        // Initialize GIF recorder
        gifRecorder = new GifRecorder();

        // Create render texture for GIF capture (1280x720)
        sceneTexture = Raylib.LoadRenderTexture(1280, 720);

        Console.WriteLine("[StarfieldScene] Initialized with Voronoi starfield shader, 1 planet, 1 ship");
        Console.WriteLine($"[StarfieldScene] Light Direction: {lightDirection}");
    }

    public void Update(float deltaTime)
    {
        HandleInput();
        UpdateMouseRaycast();

        // Update entities
        planet.Update(deltaTime);
        ship.Update(deltaTime);

        // Update camera controller
        cameraController.Update(deltaTime);
    }

    private void UpdateMouseRaycast()
    {
        // Get mouse screen position
        Vector2 mousePosition = GetMousePosition();

        // Get camera
        Camera3D camera = cameraController.GetCamera();

        // Create ray from mouse screen position
        Ray mouseRay = GetMouseRay(mousePosition, camera);

        // Define plane at origin with Vector3.UnitY as normal
        Vector3 planeNormal = Vector3.UnitY;
        Vector3 planePoint = Vector3.Zero;

        // Calculate ray-plane intersection
        // Ray equation: P = origin + t * direction
        // Plane equation: dot(P - planePoint, planeNormal) = 0
        // Solving for t: t = dot(planePoint - origin, planeNormal) / dot(direction, planeNormal)

        float denominator = Vector3.Dot(mouseRay.Direction, planeNormal);

        // Check if ray is parallel to plane
        if (Math.Abs(denominator) > 0.0001f)
        {
            float t = Vector3.Dot(planePoint - mouseRay.Position, planeNormal) / denominator;

            // Only use intersection if it's in front of the ray
            if (t >= 0)
            {
                mouseWorldPosition = mouseRay.Position + mouseRay.Direction * t;
            }
            else
            {
                mouseWorldPosition = null;
            }
        }
        else
        {
            mouseWorldPosition = null;
        }
    }

    private void HandleInput()
    {
        // Arrow keys to rotate light direction for testing sun alignment (only in debug mode)
        if (GameConfig.Instance.EnableLightDirectionControls)
        {
            float rotationSpeed = 0.02f;

            if (Raylib.IsKeyDown(KeyboardKey.Left))
        {
            // Rotate around Y axis (left)
            float angle = rotationSpeed;
            float x = lightDirection.X * MathF.Cos(angle) - lightDirection.Z * MathF.Sin(angle);
            float z = lightDirection.X * MathF.Sin(angle) + lightDirection.Z * MathF.Cos(angle);
            lightDirection = Vector3.Normalize(new Vector3(x, lightDirection.Y, z));
        }
        if (Raylib.IsKeyDown(KeyboardKey.Right))
        {
            // Rotate around Y axis (right)
            float angle = -rotationSpeed;
            float x = lightDirection.X * MathF.Cos(angle) - lightDirection.Z * MathF.Sin(angle);
            float z = lightDirection.X * MathF.Sin(angle) + lightDirection.Z * MathF.Cos(angle);
            lightDirection = Vector3.Normalize(new Vector3(x, lightDirection.Y, z));
        }
        if (Raylib.IsKeyDown(KeyboardKey.Up))
        {
            // Rotate around X axis (up)
            float angle = rotationSpeed;
            float y = lightDirection.Y * MathF.Cos(angle) - lightDirection.Z * MathF.Sin(angle);
            float z = lightDirection.Y * MathF.Sin(angle) + lightDirection.Z * MathF.Cos(angle);
            lightDirection = Vector3.Normalize(new Vector3(lightDirection.X, y, z));
        }
        if (Raylib.IsKeyDown(KeyboardKey.Down))
        {
            // Rotate around X axis (down)
            float angle = -rotationSpeed;
            float y = lightDirection.Y * MathF.Cos(angle) - lightDirection.Z * MathF.Sin(angle);
            float z = lightDirection.Y * MathF.Sin(angle) + lightDirection.Z * MathF.Cos(angle);
            lightDirection = Vector3.Normalize(new Vector3(lightDirection.X, y, z));
        }
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

            // Set custom projection matrix with far plane
            cameraController.SetProjectionMatrix(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

            // 3D mode with camera controller
            Raylib.BeginMode3D(camera);

            // Draw background starfield with sun
            background.Draw(camera, lightDirection);

            // Draw planet with procedural shader
            planetRenderer.Draw(camera, lightDirection);

            // Draw test mesh (red cube) - offset from planet
            // testMesh.Draw(new Vector3(3.0f, 0, 0));

            // Draw ship with model and shader
            ship.Renderer.Draw(camera, lightDirection);

            // Draw red cross at mouse raycast intersection
            if (mouseWorldPosition.HasValue)
            {
                DrawRedCross(mouseWorldPosition.Value);
            }

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

    private void DrawRedCross(Vector3 position)
    {
        // Draw a red cross at the given position
        float crossSize = 1.0f;
        Color crossColor = Color.Red;

        // Draw horizontal line
        DrawLine3D(
            position - new Vector3(crossSize, 0, 0),
            position + new Vector3(crossSize, 0, 0),
            crossColor
        );

        // Draw vertical line (along Z axis since Y is up)
        DrawLine3D(
            position - new Vector3(0, 0, crossSize),
            position + new Vector3(0, 0, crossSize),
            crossColor
        );
    }

    private void DrawUI()
    {
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        // Title
        Raylib.DrawText("PHANTOM NEBULA", 10, 10, 20, Color.White);

        // // Ship stats
        // Vector3 shipPos = ship.Transform.Position;
        // Raylib.DrawText($"Ship Position: {shipPos.X:F2}, {shipPos.Y:F2}, {shipPos.Z:F2}", 10, 40, 12, Color.White);
        // Raylib.DrawText($"Ship Speed: {ship.Systems.Speed:F2}", 10, 60, 12, Color.White);
        // Raylib.DrawText($"Ship Health: {ship.Health.Percent:P0}", 10, 80, 12, Color.White);

        // Planet stats
        // Raylib.DrawText($"Planet Position: {planet.Transform.Position.X:F2}, {planet.Transform.Position.Y:F2}, {planet.Transform.Position.Z:F2}", 10, 100, 12, Color.White);

        // Camera info
        string targetName = cameraTarget == 0 ? "Planet" : "Ship";
        Raylib.DrawText($"Camera Target: {targetName}", 10, 140, 12, Color.Yellow);
        Raylib.DrawText($"Orbit Distance: {cameraController.OrbitDistance:F2}", 10, 160, 12, Color.Yellow);

        // Light direction info
        Raylib.DrawText($"Light Dir: ({lightDirection.X:F2}, {lightDirection.Y:F2}, {lightDirection.Z:F2})", 10, 180, 12, new Color(0, 255, 255, 255));
        Raylib.DrawText("Arrow Keys: Rotate Light", 10, 200, 12, Color.Gray);

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
        // Raylib.DrawText("Controls:", 10, screenHeight - 140, 12, Color.Gray);
        // Raylib.DrawText("WASD - Move/Turn Ship | Left Click+Drag - Orbit Camera | Scroll - Zoom", 10, screenHeight - 120, 12, Color.Gray);
        // Raylib.DrawText("T - Toggle Camera Target | R - Reset Camera | Ctrl+8 - Record | Ctrl+9 - Stop", 10, screenHeight - 100, 12, Color.Gray);
        // Raylib.DrawText("ESC - Exit", 10, screenHeight - 80, 12, Color.Gray);
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
