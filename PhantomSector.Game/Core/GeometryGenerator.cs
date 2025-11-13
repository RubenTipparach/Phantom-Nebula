using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace PhantomSector.Game.Core;

public static class GeometryGenerator
{
    /// <summary>
    /// Generates cube vertices and indices with proper normals for lighting
    /// </summary>
    public static void CreateCube(out VertexPositionNormal[] vertices, out short[] indices)
    {
        var vertexList = new List<VertexPositionNormal>();
        var indexList = new List<short>();

        // Front face (Z+)
        AddQuad(vertexList, indexList,
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(0, 0, 1));

        // Back face (Z-)
        AddQuad(vertexList, indexList,
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(0, 0, -1));

        // Left face (X-)
        AddQuad(vertexList, indexList,
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(-1, 0, 0));

        // Right face (X+)
        AddQuad(vertexList, indexList,
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(1, 0, 0));

        // Top face (Y+)
        AddQuad(vertexList, indexList,
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0, 1, 0));

        // Bottom face (Y-)
        AddQuad(vertexList, indexList,
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0, -1, 0));

        vertices = vertexList.ToArray();
        indices = indexList.ToArray();
    }

    /// <summary>
    /// Generates sphere vertices and indices with proper normals
    /// </summary>
    public static void CreateSphere(out VertexPositionNormal[] vertices, out short[] indices, int slices = 16, int stacks = 16, float radius = 0.5f)
    {
        var vertexList = new List<VertexPositionNormal>();
        var indexList = new List<short>();

        // Generate vertices with normals
        for (int stack = 0; stack <= stacks; stack++)
        {
            float phi = MathHelper.Pi * stack / stacks;
            for (int slice = 0; slice <= slices; slice++)
            {
                float theta = MathHelper.TwoPi * slice / slices;

                var pos = new Vector3(
                    radius * (float)(System.Math.Sin(phi) * System.Math.Cos(theta)),
                    radius * (float)System.Math.Cos(phi),
                    radius * (float)(System.Math.Sin(phi) * System.Math.Sin(theta))
                );

                // Normal for a sphere is just the normalized position
                var normal = Vector3.Normalize(pos);

                vertexList.Add(new VertexPositionNormal(pos, normal));
            }
        }

        // Generate indices (counter-clockwise winding when viewed from outside)
        for (int stack = 0; stack < stacks; stack++)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                short first = (short)(stack * (slices + 1) + slice);
                short second = (short)(first + slices + 1);

                // First triangle (counter-clockwise from outside)
                indexList.Add(first);
                indexList.Add(second);
                indexList.Add((short)(first + 1));

                // Second triangle (counter-clockwise from outside)
                indexList.Add(second);
                indexList.Add((short)(second + 1));
                indexList.Add((short)(first + 1));
            }
        }

        vertices = vertexList.ToArray();
        indices = indexList.ToArray();
    }

    /// <summary>
    /// Creates a colored box wireframe for debug visualization
    /// </summary>
    public static void CreateWireframeCube(out VertexPositionColor[] vertices, out short[] indices, Color color)
    {
        vertices = new VertexPositionColor[]
        {
            // Front face vertices
            new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), color),
            new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), color),
            new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), color),
            new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), color),

            // Back face vertices
            new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), color),
            new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), color),
            new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), color),
            new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), color)
        };

        indices = new short[]
        {
            // Front face
            0, 1, 1, 2, 2, 3, 3, 0,
            // Back face
            4, 5, 5, 6, 6, 7, 7, 4,
            // Connecting edges
            0, 4, 1, 5, 2, 6, 3, 7
        };
    }

    private static void AddQuad(List<VertexPositionNormal> vertices, List<short> indices,
        Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal)
    {
        short baseIndex = (short)vertices.Count;

        vertices.Add(new VertexPositionNormal(v0, normal));
        vertices.Add(new VertexPositionNormal(v1, normal));
        vertices.Add(new VertexPositionNormal(v2, normal));
        vertices.Add(new VertexPositionNormal(v3, normal));

        // Counter-clockwise winding order
        indices.Add(baseIndex);
        indices.Add((short)(baseIndex + 2));
        indices.Add((short)(baseIndex + 1));

        indices.Add(baseIndex);
        indices.Add((short)(baseIndex + 3));
        indices.Add((short)(baseIndex + 2));
    }
}
