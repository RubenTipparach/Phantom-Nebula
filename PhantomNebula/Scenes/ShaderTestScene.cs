using System;
using System.Numerics;
using Raylib_cs;
using PhantomNebula.Renderers;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Scenes;

/// <summary>
/// Test scene for shader development
/// Contains test mesh and sphere renderer for testing Gouraud shading
/// </summary>
public class ShaderTestScene
{
    private TestMesh testMesh;
    private SphereRenderer sphere;
    private Camera3D camera;
    private Vector3 lightDirection;

    public ShaderTestScene()
    {
        // Initialize test mesh (red cube)
        testMesh = new TestMesh();

        // Create sphere at origin
        sphere = new SphereRenderer(new Vector3(3.0f, 0, 0), 1.0f);

        // Setup camera
        camera = new Camera3D
        {
            Position = new Vector3(0, 5, 10),
            Target = new Vector3(0, 0, 0),
            Up = Vector3.UnitY,
            FovY = 45.0f,
            Projection = CameraProjection.Perspective
        };

        // Setup light direction
        lightDirection = Vector3.Normalize(new Vector3(1.0f, -1.0f, 1.0f));

        Console.WriteLine("[ShaderTestScene] Initialized shader test scene");
    }

    public void Update(float deltaTime)
    {
        // Basic camera controls
        UpdateCamera(ref camera, CameraMode.Free);
    }

    public void Draw()
    {
        ClearBackground(Color.Black);

        BeginMode3D(camera);

        // Draw test mesh (red cube) at origin
        testMesh.Draw(Vector3.Zero);

        // Draw sphere with Gouraud shading
        sphere.Draw(camera, lightDirection);

        // Draw grid for reference
        DrawGrid(10, 1.0f);

        EndMode3D();

        // Draw UI
        DrawText("SHADER TEST SCENE", 10, 10, 20, Color.White);
        DrawText("WASD + Mouse - Camera | ESC - Exit", 10, 40, 12, Color.Gray);
    }

    public void Dispose()
    {
        testMesh.Dispose();
        sphere.Dispose();
    }
}
