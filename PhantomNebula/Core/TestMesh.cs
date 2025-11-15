using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Core;

/// <summary>
/// Test mesh class - generates a simple cube with a red shader
/// </summary>
public class TestMesh
{
    private Model testCube;
    private Shader testShader;
    private bool initialized = false;

    public TestMesh()
    {
        try
        {
            // Load the starfield shader
            testShader = LoadShader("Shaders/Starfield.vs", "Shaders/Starfield.fs");

            if (testShader.Id == 0)
            {
                Console.WriteLine("[TestMesh] ERROR: Shader failed to load");
                return;
            }

            // Create a custom cube mesh using vertices and normals
            Mesh cubeMesh = GenMeshCustom();
            testCube = LoadModelFromMesh(cubeMesh);

            // Apply shader
            unsafe
            {
                testCube.Materials[0].Shader = testShader;
            }

            initialized = true;
            Console.WriteLine("[TestMesh] Test mesh initialized successfully with custom cube and starfield shader");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TestMesh] Failed to initialize: {ex.Message}");
            initialized = false;
        }
    }

    private static Mesh GenMeshCustom()
    {
        // Create a simple cube with 8 vertices and indices
        Mesh mesh = new(8, 12);
        mesh.AllocVertices();
        mesh.AllocTexCoords();
        mesh.AllocNormals();
        mesh.AllocIndices();

        Span<Vector3> vertices = mesh.VerticesAs<Vector3>();
        Span<Vector2> texcoords = mesh.TexCoordsAs<Vector2>();
        Span<Vector3> normals = mesh.NormalsAs<Vector3>();
        Span<ushort> indices = mesh.IndicesAs<ushort>();

        // Define cube vertices
        float size = 0.5f;
        vertices[0] = new(-size, -size, -size);
        vertices[1] = new(size, -size, -size);
        vertices[2] = new(size, size, -size);
        vertices[3] = new(-size, size, -size);
        vertices[4] = new(-size, -size, size);
        vertices[5] = new(size, -size, size);
        vertices[6] = new(size, size, size);
        vertices[7] = new(-size, size, size);

        // Set normals (simple outward-pointing normals)
        for (int i = 0; i < 8; i++)
        {
            normals[i] = Vector3.Normalize(vertices[i]);
        }

        // Set texture coordinates
        texcoords[0] = new(0, 0);
        texcoords[1] = new(1, 0);
        texcoords[2] = new(1, 1);
        texcoords[3] = new(0, 1);
        texcoords[4] = new(0, 0);
        texcoords[5] = new(1, 0);
        texcoords[6] = new(1, 1);
        texcoords[7] = new(0, 1);

        // Define cube indices (6 faces, 2 triangles per face = 12 triangles)
        int idx = 0;

        // Front face
        indices[idx++] = 0; indices[idx++] = 1; indices[idx++] = 2;
        indices[idx++] = 0; indices[idx++] = 2; indices[idx++] = 3;

        // Back face
        indices[idx++] = 4; indices[idx++] = 6; indices[idx++] = 5;
        indices[idx++] = 4; indices[idx++] = 7; indices[idx++] = 6;

        // Left face
        indices[idx++] = 4; indices[idx++] = 3; indices[idx++] = 7;
        indices[idx++] = 4; indices[idx++] = 0; indices[idx++] = 3;

        // Right face
        indices[idx++] = 1; indices[idx++] = 5; indices[idx++] = 6;
        indices[idx++] = 1; indices[idx++] = 6; indices[idx++] = 2;

        // Top face
        indices[idx++] = 3; indices[idx++] = 2; indices[idx++] = 6;
        indices[idx++] = 3; indices[idx++] = 6; indices[idx++] = 7;

        // Bottom face
        indices[idx++] = 4; indices[idx++] = 5; indices[idx++] = 1;
        indices[idx++] = 4; indices[idx++] = 1; indices[idx++] = 0;

        // Upload mesh to GPU
        UploadMesh(ref mesh, false);

        return mesh;
    }

    public void Draw(Vector3 position)
    {
        if (!initialized)
            return;

        DrawModel(testCube, position, 1.0f, Color.White);
    }

    public void Dispose()
    {
        if (initialized)
        {
            UnloadShader(testShader);
            UnloadModel(testCube);
        }
    }
}
