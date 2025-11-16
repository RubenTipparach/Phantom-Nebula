using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Renderers;

/// <summary>
/// Planet renderer - renders a procedural planet with clouds, atmosphere, and terrain
/// </summary>
public class PlanetRenderer
{
    private Model planetModel;
    private Shader planetShader;
    private bool shaderLoaded = false;
    private Vector3 position;
    private float scale;

    public PlanetRenderer(Vector3 planetPosition, float planetScale, int sphereSegments = 64)
    {
        position = planetPosition;
        scale = planetScale;

        try
        {
            // Load the planet shader
            string vertexPath = "Shaders/Planet.vs";
            string fragmentPath = "Shaders/Planet.fs";

            Console.WriteLine($"[PlanetRenderer] Loading shader from: {vertexPath} and {fragmentPath}");
            planetShader = LoadShader(vertexPath, fragmentPath);

            if (planetShader.Id == 0)
            {
                Console.WriteLine("[PlanetRenderer] ERROR: Shader failed to load (ID is 0)");
                shaderLoaded = false;
                return;
            }

            shaderLoaded = true;
            Console.WriteLine("[PlanetRenderer] Loaded planet shader successfully");

            // Create a high-resolution sphere for planet rendering
            Mesh sphereMesh = GenMeshSphere(scale, sphereSegments, sphereSegments);
            planetModel = LoadModelFromMesh(sphereMesh);

            // Apply shader to model material
            unsafe
            {
                planetModel.Materials[0].Shader = planetShader;
            }

            Console.WriteLine("[PlanetRenderer] Created planet sphere with shader applied");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlanetRenderer] Failed to initialize: {ex.Message}");
            Console.WriteLine($"[PlanetRenderer] Stack trace: {ex.StackTrace}");
            shaderLoaded = false;
        }
    }

    public void Draw(Camera3D camera, Vector3 lightDirection)
    {
        if (!shaderLoaded)
        {
            return;
        }

        // Get time once
        float timeValue = (float)GetTime();

        // Set shader uniforms before drawing
        int cameraPosLoc = GetShaderLocation(planetShader, "cameraPos");
        if (cameraPosLoc != -1)
            SetShaderValue(planetShader, cameraPosLoc, camera.Position, ShaderUniformDataType.Vec3);

        int timeLoc = GetShaderLocation(planetShader, "time");
        if (timeLoc != -1)
            SetShaderValue(planetShader, timeLoc, timeValue, ShaderUniformDataType.Float);

        int lightDirLoc = GetShaderLocation(planetShader, "lightDir");
        if (lightDirLoc != -1)
            SetShaderValue(planetShader, lightDirLoc, lightDirection, ShaderUniformDataType.Vec3);

        // Draw the planet with shader
        DrawModel(planetModel, position, 1.0f, Color.White);
    }

    public void Dispose()
    {
        if (shaderLoaded)
        {
            UnloadShader(planetShader);
            UnloadModel(planetModel);
        }
    }
}
