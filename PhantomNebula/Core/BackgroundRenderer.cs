using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Core;

/// <summary>
/// Background renderer - renders Voronoi starfield using a full-screen quad
/// Renders procedural 3D Voronoi starfield via shader
/// </summary>
public class BackgroundRenderer
{
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

    public void Draw(Camera3D camera)
    {
        if (!shaderLoaded)
        {
            return;
        }

        // Disable backface culling for sphere rendering
        Rlgl.DisableBackfaceCulling();

        // Draw background sphere at camera position to wrap around it
        // Large scale so it surrounds everything
        DrawModel(model, camera.Position, 1000.0f, Color.White);

        // Re-enable backface culling
        Rlgl.EnableBackfaceCulling();
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
