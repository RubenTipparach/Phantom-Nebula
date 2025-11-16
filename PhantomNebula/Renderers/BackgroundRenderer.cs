using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Renderers;

/// <summary>
/// Background renderer - renders Voronoi starfield using a full-screen quad
/// Renders procedural 3D Voronoi starfield via shader
/// </summary>
public class BackgroundRenderer
{
    // OpenGL constants for depth function
    private const int GL_LESS = 0x0201;
    private const int GL_LEQUAL = 0x0203;
    private const int GL_EQUAL = 0x0202;

    // P/Invoke for OpenGL depth function
    [DllImport("opengl32.dll", EntryPoint = "glDepthFunc")]
    private static extern void glDepthFunc(int func);

    private Shader backgroundShader;
    private Model model;
    private bool shaderLoaded = false;

    public BackgroundRenderer()
    {
        try
        {
            // Load the background shader
            string vertexPath = "Shaders/Starfield.vs";
            string fragmentPath = "Shaders/Starfield.fs";

            Console.WriteLine($"[BackgroundRenderer] Loading shader from: {vertexPath} and {fragmentPath}");
            backgroundShader = LoadShader(vertexPath, fragmentPath);

            if (backgroundShader.Id == 0)
            {
                Console.WriteLine("[BackgroundRenderer] ERROR: Shader failed to load (ID is 0)");
                shaderLoaded = false;
                return;
            }

            shaderLoaded = true;
            Console.WriteLine("[BackgroundRenderer] Loaded Voronoi starfield shader successfully");

            // Create background sphere using built-in function
            Mesh sphereMesh = GenMeshSphere(1.0f, 32, 32);
            model = LoadModelFromMesh(sphereMesh);
            Console.WriteLine("[BackgroundRenderer] Created background sphere");

            // Apply shader to quad material
            unsafe
            {
                model.Materials[0].Shader = backgroundShader;
            }

            Console.WriteLine("[BackgroundRenderer] Applied starfield shader to background");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BackgroundRenderer] Failed to initialize: {ex.Message}");
            Console.WriteLine($"[BackgroundRenderer] Stack trace: {ex.StackTrace}");
            shaderLoaded = false;
        }
    }

    private static Mesh CreateInvertedBackgroundSphere()
    {
        // Create a base sphere
        Mesh baseMesh = GenMeshSphere(1.0f, 32, 32);

        // Create a new mesh with inverted geometry
        Mesh invertedMesh = new(baseMesh.VertexCount, baseMesh.TriangleCount);
        invertedMesh.AllocVertices();
        invertedMesh.AllocNormals();
        invertedMesh.AllocTexCoords();
        invertedMesh.AllocIndices();

        // Copy vertices
        Span<Vector3> baseVertices = baseMesh.VerticesAs<Vector3>();
        Span<Vector3> vertices = invertedMesh.VerticesAs<Vector3>();
        baseVertices.CopyTo(vertices);

        // Invert normals
        Span<Vector3> baseNormals = baseMesh.NormalsAs<Vector3>();
        Span<Vector3> normals = invertedMesh.NormalsAs<Vector3>();
        for (int i = 0; i < baseMesh.VertexCount; i++)
        {
            normals[i] = -baseNormals[i];
        }

        // Copy texture coordinates
        Span<Vector2> baseTexCoords = baseMesh.TexCoordsAs<Vector2>();
        Span<Vector2> texCoords = invertedMesh.TexCoordsAs<Vector2>();
        baseTexCoords.CopyTo(texCoords);

        // Copy and reverse indices (invert winding order)
        Span<ushort> baseIndices = baseMesh.IndicesAs<ushort>();
        Span<ushort> indices = invertedMesh.IndicesAs<ushort>();
        for (int i = 0; i < baseMesh.TriangleCount * 3; i += 3)
        {
            indices[i] = baseIndices[i];
            indices[i + 1] = baseIndices[i + 2];
            indices[i + 2] = baseIndices[i + 1];
        }

        // Upload mesh to GPU
        UploadMesh(ref invertedMesh, false);

        return invertedMesh;
    }

    public void Draw(Camera3D camera, Vector3 lightDirection)
    {
        if (!shaderLoaded)
        {
            return;
        }

        // Set shader uniforms
        float timeValue = (float)GetTime();

        int cameraPosLoc = GetShaderLocation(backgroundShader, "cameraPosition");
        if (cameraPosLoc != -1)
            SetShaderValue(backgroundShader, cameraPosLoc, camera.Position, ShaderUniformDataType.Vec3);

        int lightDirLoc = GetShaderLocation(backgroundShader, "lightDirection");
        if (lightDirLoc != -1)
            SetShaderValue(backgroundShader, lightDirLoc, lightDirection, ShaderUniformDataType.Vec3);

        int timeLoc = GetShaderLocation(backgroundShader, "time");
        if (timeLoc != -1)
            SetShaderValue(backgroundShader, timeLoc, timeValue, ShaderUniformDataType.Float);

        // We are inside the sphere, we need to disable backface culling!
        Rlgl.DisableBackfaceCulling();
        Rlgl.DisableDepthMask();

        // Draw background sphere at camera position to wrap around it
        DrawModel(model, camera.Position, 100.0f, Color.White);

        Rlgl.EnableBackfaceCulling();
        Rlgl.EnableDepthMask();
    }

    public void Dispose()
    {
        if (shaderLoaded)
        {
            UnloadShader(backgroundShader);
            UnloadModel(model);
        }
    }
}
