using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Renderers;

/// <summary>
/// Sphere renderer using Gouraud shading
/// </summary>
public class SphereRenderer
{
    private Model sphereModel;
    private Shader sphereShader;
    private Texture2D sphereTexture;
    private Texture2D sphereEmissiveTexture;
    private bool shaderLoaded = false;
    private Vector3 position;
    private float radius;

    public SphereRenderer(Vector3 position, float radius = 1.0f, string? texturePath = null)
    {
        this.position = position;
        this.radius = radius;

        try
        {
            // Generate sphere mesh
            Mesh sphereMesh = GenMeshSphere(radius, 32, 32);
            sphereModel = LoadModelFromMesh(sphereMesh);

            Console.WriteLine("[SphereRenderer] Generated sphere mesh successfully");

            // Load texture if provided
            if (!string.IsNullOrEmpty(texturePath))
            {
                sphereTexture = LoadTexture(texturePath);
                Console.WriteLine($"[SphereRenderer] Loaded texture: {texturePath}");
            }
            else
            {
                // Create a simple white texture as default
                Image albedoImage = GenImageColor(1, 1, Color.White);
                sphereTexture = LoadTextureFromImage(albedoImage);
                UnloadImage(albedoImage);
                Console.WriteLine("[SphereRenderer] Using default white texture");
            }

            // Create black emissive texture (no emission)
            Image emissiveImage = GenImageColor(1, 1, Color.Black);
            sphereEmissiveTexture = LoadTextureFromImage(emissiveImage);
            UnloadImage(emissiveImage);

            // Try to load Gouraud shader (reuse ship shader)
            string vertexPath = "Shaders/Ship.vs";
            string fragmentPath = "Shaders/Ship.fs";

            unsafe
            {
                fixed (byte* vPath = System.Text.Encoding.UTF8.GetBytes(vertexPath + "\0"))
                fixed (byte* fPath = System.Text.Encoding.UTF8.GetBytes(fragmentPath + "\0"))
                {
                    if (FileExists((sbyte*)vPath) && FileExists((sbyte*)fPath))
                    {
                        Console.WriteLine($"[SphereRenderer] Loading Gouraud shader from: {vertexPath} and {fragmentPath}");
                        sphereShader = LoadShader((sbyte*)vPath, (sbyte*)fPath);

                        if (sphereShader.Id != 0)
                        {
                            shaderLoaded = true;
                            // Apply shader and textures to material
                            sphereModel.Materials[0].Shader = sphereShader;
                            // Set texture slots for the shader
                            // texture0 = albedo/diffuse (Maps[0]), texture1 = emissive (Maps[1])
                            sphereModel.Materials[0].Maps[0].Texture = sphereTexture;
                            sphereModel.Materials[0].Maps[1].Texture = sphereEmissiveTexture;
                            Console.WriteLine("[SphereRenderer] Loaded Gouraud shader successfully");
                        }
                        else
                        {
                            Console.WriteLine("[SphereRenderer] ERROR: Shader failed to load (ID is 0)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[SphereRenderer] Shader files not found, using default material");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SphereRenderer] Failed to initialize: {ex.Message}");
            Console.WriteLine($"[SphereRenderer] Stack trace: {ex.StackTrace}");
        }
    }

    public void Draw(Camera3D camera, Vector3 lightDirection)
    {
        if (sphereModel.MaterialCount == 0)
            return;

        // Set shader uniforms if shader is loaded
        if (shaderLoaded)
        {
            int cameraPosLoc = GetShaderLocation(sphereShader, "cameraPos");
            if (cameraPosLoc != -1)
                SetShaderValue(sphereShader, cameraPosLoc, camera.Position, ShaderUniformDataType.Vec3);

            int lightDirLoc = GetShaderLocation(sphereShader, "lightDir");
            if (lightDirLoc != -1)
                SetShaderValue(sphereShader, lightDirLoc, lightDirection, ShaderUniformDataType.Vec3);
        }

        // Draw the sphere
        DrawModel(sphereModel, position, 1.0f, Color.White);
    }

    public void SetPosition(Vector3 newPosition)
    {
        position = newPosition;
    }

    public void Dispose()
    {
        UnloadTexture(sphereTexture);
        UnloadTexture(sphereEmissiveTexture);
        UnloadModel(sphereModel);
        if (shaderLoaded)
        {
            UnloadShader(sphereShader);
        }
    }
}
