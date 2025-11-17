using System;
using System.Numerics;
using PhantomNebula.Core;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Renderers;

/// <summary>
/// Ship renderer - loads and renders the shippy model with PBR-like lighting
/// Uses directional light source consistent with planet lighting
/// Requires a Transform to draw
/// </summary>
public class ShipRenderer
{
    private Model shipModel;
    private Texture2D shipTexture;
    private Texture2D shipEmissiveTexture;
    private Shader shipShader;
    private bool shaderLoaded = false;

    public ShipRenderer(string modelPath = "Resources/Models/shippy1.obj",
                        string albedoPath = "Resources/Models/shippy.png",
                        string emissivePath = "Resources/Models/shippy_em.png")
    {

        try
        {
            // Load albedo texture
            shipTexture = LoadTexture(albedoPath);

            // Load emissive texture or create black texture if not provided
            if (!string.IsNullOrEmpty(emissivePath) && System.IO.File.Exists(emissivePath))
            {
                shipEmissiveTexture = LoadTexture(emissivePath);
                Console.WriteLine($"[ShipRenderer] Loaded emissive texture from {emissivePath}");
            }
            else
            {
                // Create 1x1 black pixel texture for no glow
                Image blackImage = GenImageColor(1, 1, new Color(0, 0, 0, 255));
                shipEmissiveTexture = LoadTextureFromImage(blackImage);
                UnloadImage(blackImage);
                Console.WriteLine("[ShipRenderer] Using 1x1 black texture for emissive");
            }

            // Load the ship model (already has smooth normals from OBJ)
            shipModel = LoadModel(modelPath);

            Console.WriteLine("[ShipRenderer] Loaded shippy model successfully");

            // Try to load ship shader
            string vertexPath = "Shaders/Ship.vs";
            string fragmentPath = "Shaders/Ship.fs";

            unsafe
            {
                fixed (byte* vPath = System.Text.Encoding.UTF8.GetBytes(vertexPath + "\0"))
                fixed (byte* fPath = System.Text.Encoding.UTF8.GetBytes(fragmentPath + "\0"))
                {
                    if (FileExists((sbyte*)vPath) && FileExists((sbyte*)fPath))
                    {
                        Console.WriteLine($"[ShipRenderer] Loading shader from: {vertexPath} and {fragmentPath}");
                        shipShader = LoadShader((sbyte*)vPath, (sbyte*)fPath);

                        if (shipShader.Id != 0)
                        {
                            shaderLoaded = true;
                            // Apply shader and textures to all materials
                            for (int i = 0; i < shipModel.MaterialCount; i++)
                            {
                                shipModel.Materials[i].Shader = shipShader;
                                // Set texture slots for the shader
                                // texture0 = albedo/diffuse (Maps[0]), texture1 = emissive (Maps[1])
                                shipModel.Materials[i].Maps[0].Texture = shipTexture;
                                shipModel.Materials[i].Maps[1].Texture = shipEmissiveTexture;
                            }
                            Console.WriteLine("[ShipRenderer] Loaded ship shader successfully");
                        }
                        else
                        {
                            Console.WriteLine("[ShipRenderer] ERROR: Ship shader failed to load (ID is 0)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[ShipRenderer] Ship shader files not found, using default material");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShipRenderer] Failed to initialize: {ex.Message}");
            Console.WriteLine($"[ShipRenderer] Stack trace: {ex.StackTrace}");
        }
    }

    public void Draw(Core.Transform transform, Camera3D camera, Vector3 lightDirection)
    {
        if (shipModel.MaterialCount == 0)
            return;

        // Set shader uniforms if shader is loaded
        if (shaderLoaded)
        {
            // Get time once
            float timeValue = (float)GetTime();

            // Set shader uniforms before drawing
            int cameraPosLoc = GetShaderLocation(shipShader, "cameraPos");
            if (cameraPosLoc != -1)
                SetShaderValue(shipShader, cameraPosLoc, camera.Position, ShaderUniformDataType.Vec3);

            int lightDirLoc = GetShaderLocation(shipShader, "lightDir");
            if (lightDirLoc != -1)
                SetShaderValue(shipShader, lightDirLoc, lightDirection, ShaderUniformDataType.Vec3);

            int timeLoc = GetShaderLocation(shipShader, "time");
            if (timeLoc != -1)
                SetShaderValue(shipShader, timeLoc, timeValue, ShaderUniformDataType.Float);
        }

        // Draw the ship model using Transform values
        // Convert quaternion rotation to matrix
        Quaternion quat = transform.QuatRotation;
        shipModel.Transform = Raylib_cs.Raymath.QuaternionToMatrix(quat);

        DrawModel(shipModel, transform.Position, transform.Scale.X, Color.White);
    }

    public void Dispose()
    {
        UnloadModel(shipModel);
        UnloadTexture(shipTexture);
        UnloadTexture(shipEmissiveTexture);
        if (shaderLoaded)
        {
            UnloadShader(shipShader);
        }
    }
}
