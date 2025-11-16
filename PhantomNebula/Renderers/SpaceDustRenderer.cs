using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Renderers;

/// <summary>
/// Space dust renderer - renders a transparent plane with procedural dust particles
/// The plane follows the camera and uses planet position for parallax
/// </summary>
public class SpaceDustRenderer
{
    private Model planeModel;
    private Shader dustShader;
    private bool shaderLoaded = false;
    private float planeSize = 1000.0f;
    private float startTime;

    public SpaceDustRenderer()
    {
        try
        {
            // Generate a large plane mesh
            Mesh planeMesh = GenMeshPlane(planeSize, planeSize, 1, 1);
            planeModel = LoadModelFromMesh(planeMesh);

            // Load space dust shader
            string vertexPath = "Shaders/SpaceDust.vs";
            string fragmentPath = "Shaders/SpaceDust.fs";

            unsafe
            {
                fixed (byte* vPath = System.Text.Encoding.UTF8.GetBytes(vertexPath + "\0"))
                fixed (byte* fPath = System.Text.Encoding.UTF8.GetBytes(fragmentPath + "\0"))
                {
                    if (FileExists((sbyte*)vPath) && FileExists((sbyte*)fPath))
                    {
                        Console.WriteLine($"[SpaceDustRenderer] Loading space dust shader from: {vertexPath} and {fragmentPath}");
                        dustShader = LoadShader((sbyte*)vPath, (sbyte*)fPath);

                        if (dustShader.Id != 0)
                        {
                            shaderLoaded = true;
                            planeModel.Materials[0].Shader = dustShader;
                            Console.WriteLine("[SpaceDustRenderer] Loaded space dust shader successfully");
                        }
                        else
                        {
                            Console.WriteLine("[SpaceDustRenderer] ERROR: Shader failed to load (ID is 0)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[SpaceDustRenderer] Shader files not found");
                    }
                }
            }

            startTime = (float)GetTime();
            Console.WriteLine("[SpaceDustRenderer] Initialized space dust renderer");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SpaceDustRenderer] Failed to initialize: {ex.Message}");
            Console.WriteLine($"[SpaceDustRenderer] Stack trace: {ex.StackTrace}");
        }
    }

    public void Draw(Camera3D camera, Vector3 planetPosition, Vector3 shipPosition)
    {
        if (planeModel.MaterialCount == 0 || !shaderLoaded)
            return;

        // Set shader uniforms
        int cameraPosLoc = GetShaderLocation(dustShader, "cameraPos");
        if (cameraPosLoc != -1)
            SetShaderValue(dustShader, cameraPosLoc, camera.Position, ShaderUniformDataType.Vec3);

        int planetPosLoc = GetShaderLocation(dustShader, "planetPos");
        if (planetPosLoc != -1)
            SetShaderValue(dustShader, planetPosLoc, planetPosition, ShaderUniformDataType.Vec3);

        int shipPosLoc = GetShaderLocation(dustShader, "shipPos");
        if (shipPosLoc != -1)
            SetShaderValue(dustShader, shipPosLoc, shipPosition, ShaderUniformDataType.Vec3);

        float currentTime = (float)GetTime() - startTime;
        int timeLoc = GetShaderLocation(dustShader, "time");
        if (timeLoc != -1)
            SetShaderValue(dustShader, timeLoc, currentTime, ShaderUniformDataType.Float);

        // Position plane at Y=0, following ship in XZ
        Vector3 planePosition = new Vector3(shipPosition.X, 0.0f, shipPosition.Z);

        // Disable backface culling for double-sided rendering
        Rlgl.DisableBackfaceCulling();

        // Disable depth writing so transparent parts don't block objects behind
        Rlgl.DisableDepthMask();

        // Draw the plane
        DrawModel(planeModel, planePosition, 1.0f, Color.White);

        // Re-enable depth writing and backface culling
        Rlgl.EnableDepthMask();
        Rlgl.EnableBackfaceCulling();
    }

    public void Dispose()
    {
        UnloadModel(planeModel);
        if (shaderLoaded)
        {
            UnloadShader(dustShader);
        }
    }
}
