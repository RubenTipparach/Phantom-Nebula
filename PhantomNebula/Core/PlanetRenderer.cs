using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Core;

/// <summary>
/// Shader-based planet renderer using lighting shader with texture
/// Renders a sphere with Phong lighting applied through raylib-cs pattern
/// </summary>
public class PlanetRenderer
{
    private Vector3 position;
    private float radius;
    private Model planetModel;
    private Shader lightingShader;
    private Texture2D planetTexture;
    private bool shaderLoaded = false;
    private Light[] lights = new Light[4];

    public unsafe PlanetRenderer(Vector3 position, float radius = 1.0f, int subdivisions = 32)
    {
        this.position = position;
        this.radius = radius;

        try
        {
            // Load the lighting shader
            string vertexPath = "Shaders/lighting.vs";
            string fragmentPath = "Shaders/lighting.fs";

            lightingShader = LoadShader(vertexPath, fragmentPath);
            shaderLoaded = true;
            Console.WriteLine("[PlanetRenderer] Loaded lighting shader from files");

            // Set up shader locations for lighting
            lightingShader.Locs[(int)ShaderLocationIndex.VectorView] = GetShaderLocation(lightingShader, "viewPos");

            // Set ambient lighting uniform
            int ambientLoc = GetShaderLocation(lightingShader, "ambient");
            float[] ambient = new[] { 0.2f, 0.2f, 0.2f, 1.0f };
            SetShaderValue(lightingShader, ambientLoc, ambient, ShaderUniformDataType.Vec4);

            // Create sphere mesh and wrap in model
            Mesh sphereMesh = GenMeshSphere(radius, subdivisions, subdivisions);
            planetModel = LoadModelFromMesh(sphereMesh);
            Console.WriteLine($"[PlanetRenderer] Created planet sphere with radius {radius}");

            // Load texture from file
            planetTexture = LoadTexture("Resources/planet.png");
            Console.WriteLine("[PlanetRenderer] Loaded planet texture from file");

            // Apply shader and texture to the planet's material
            unsafe
            {
                planetModel.Materials[0].Shader = lightingShader;
                planetModel.Materials[0].Maps[(int)MaterialMapIndex.Albedo].Texture = planetTexture;

                // Set diffuse color
                SetShaderValue(lightingShader,
                    GetShaderLocation(lightingShader, "colDiffuse"),
                    new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    ShaderUniformDataType.Vec4);
            }

            // Create four point lights around the planet
            lights[0] = Rlights.CreateLight(
                0,
                LightType.Point,
                new Vector3(3, 2, 2),
                Vector3.Zero,
                Color.Yellow,
                lightingShader
            );
            lights[1] = Rlights.CreateLight(
                1,
                LightType.Point,
                new Vector3(-3, 2, 2),
                Vector3.Zero,
                Color.Red,
                lightingShader
            );
            lights[2] = Rlights.CreateLight(
                2,
                LightType.Point,
                new Vector3(0, 3, -3),
                Vector3.Zero,
                Color.Green,
                lightingShader
            );
            lights[3] = Rlights.CreateLight(
                3,
                LightType.Point,
                new Vector3(0, -2, 3),
                Vector3.Zero,
                Color.Blue,
                lightingShader
            );

            Console.WriteLine("[PlanetRenderer] Applied lighting shader and created lights");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlanetRenderer] Failed to load shader: {ex.Message}");
            shaderLoaded = false;
        }
    }

    public unsafe void Draw(Camera3D camera)
    {
        if (!shaderLoaded)
        {
            // Fallback to simple sphere rendering
            DrawSphere(position, radius, new Color(100, 120, 200, 255));
            return;
        }

        // Update light values in shader
        for (int i = 0; i < 4; i++)
        {
            Rlights.UpdateLightValues(lightingShader, lights[i]);
        }

        // Update viewPos uniform with camera position
        SetShaderValue(
            lightingShader,
            lightingShader.Locs[(int)ShaderLocationIndex.VectorView],
            camera.Position,
            ShaderUniformDataType.Vec3
        );

        // Draw planet with shader applied through material
        DrawModel(planetModel, position, 1.0f, Color.White);
    }

    public void Dispose()
    {
        if (shaderLoaded)
        {
            UnloadShader(lightingShader);
            UnloadModel(planetModel);
            UnloadTexture(planetTexture);
        }
    }
}
