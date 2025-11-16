using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Core;

/// <summary>
/// Ship renderer - loads and renders the shippy model with PBR-like lighting
/// Uses directional light source consistent with planet lighting
/// </summary>
public class ShipRenderer
{
    private Model shipModel;
    private Texture2D shipTexture;
    private Texture2D shipEmissiveTexture;
    private Shader shipShader;
    private bool shaderLoaded = false;
    private Vector3 position;
    private float scale;

    public ShipRenderer(Vector3 shipPosition, float shipScale = 1.0f)
    {
        position = shipPosition;
        scale = shipScale;

        try
        {
            // Load ship textures
            shipTexture = LoadTexture("Resources/Models/shippy.png");
            shipEmissiveTexture = LoadTexture("Resources/Models/shippy_em.png");

            // Load the ship model
            shipModel = LoadModel("Resources/Models/shippy1.obj");

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

    public void UpdatePosition(Vector3 newPosition)
    {
        position = newPosition;
    }

    public void Draw(Camera3D camera, Vector3 lightDirection)
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

        // Draw the ship model
        DrawModel(shipModel, position, scale, Color.White);
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
