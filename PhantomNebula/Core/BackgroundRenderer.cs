using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Core;

/// <summary>
/// Background renderer - renders Voronoi starfield using large sphere
/// Shader renders procedural starfield based on view direction
/// </summary>
public class BackgroundRenderer
{
    private Shader backgroundShader;
    private Model backgroundSphere;
    private bool shaderLoaded = false;
    private const float SPHERE_RADIUS = 100.0f;

    public BackgroundRenderer()
    {
        try
        {
            // Load the background shader
            string vertexPath = "Shaders/Starfield.vs";
            string fragmentPath = "Shaders/Starfield.fs";

            backgroundShader = LoadShader(vertexPath, fragmentPath);
            shaderLoaded = true;
            Console.WriteLine("[BackgroundRenderer] Loaded Voronoi starfield shader");

            // Create large sphere for background
            Mesh sphereMesh = GenMeshSphere(SPHERE_RADIUS, 32, 32);
            backgroundSphere = LoadModelFromMesh(sphereMesh);
            Console.WriteLine($"[BackgroundRenderer] Created background sphere with radius {SPHERE_RADIUS}");

            // Apply shader to sphere material
            unsafe
            {
                backgroundSphere.Materials[0].Shader = backgroundShader;
            }

            Console.WriteLine("[BackgroundRenderer] Applied starfield shader to background");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BackgroundRenderer] Failed to initialize: {ex.Message}");
            shaderLoaded = false;
        }
    }

    public void Draw(Camera3D camera)
    {
        if (!shaderLoaded)
        {
            return;
        }

        // Disable backface culling so we can see the sphere from inside
        Rlgl.DisableBackfaceCulling();

        // Draw sphere at camera position so it always surrounds the view
        // This creates the illusion of an infinite starfield background
        DrawModel(backgroundSphere, camera.Position, 1.0f, Color.White);

        // Re-enable backface culling
        Rlgl.EnableBackfaceCulling();
    }

    public void Dispose()
    {
        if (shaderLoaded)
        {
            UnloadShader(backgroundShader);
            UnloadModel(backgroundSphere);
        }
    }
}
