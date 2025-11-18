using Raylib_cs;
using System;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Renderers;

/// <summary>
/// Debug renderer for testing and iterating on shaders.
/// Renders a simple quad at a fixed position with a specified shader and texture.
/// </summary>
public class DebugShaderTestRenderer
{
    private Texture2D texture;
    private Shader shader;
    private Mesh quadMesh;
    private Material material;
    private Vector3 position = Vector3.Zero;
    private float scale = 1.0f;
    private bool isInitialized = false;
    private int ditherPhaseUniformLoc = -1;
    private int alphaUniformLoc = -1;

    /// <summary>
    /// Initializes the debug renderer with a shader and texture.
    /// </summary>
    public DebugShaderTestRenderer(string shaderVsPath, string shaderFsPath, string texturePath)
    {
        try
        {
            // Load texture
            texture = LoadTexture(texturePath);
            if (texture.Id != 0)
            {
                System.Console.WriteLine($"[DebugShaderTestRenderer] Loaded texture: {texturePath}");
            }
            else
            {
                System.Console.WriteLine($"[DebugShaderTestRenderer] Failed to load texture");
                CreateFallbackTexture();
            }

            // Create quad mesh (1x1 plane)
            quadMesh = GenMeshPlane(3.0f, 3.0f, 1, 1);

            // Load shader
            shader = LoadShader(shaderVsPath, shaderFsPath);
            System.Console.WriteLine($"[DebugShaderTestRenderer] Loaded shader: {shaderVsPath} / {shaderFsPath}");

            // Get uniform locations (may not exist for all shaders)
            ditherPhaseUniformLoc = GetShaderLocation(shader, "ditherPhase");
            alphaUniformLoc = GetShaderLocation(shader, "alpha");

            // Create material
            material = LoadMaterialDefault();
            SetMaterialTexture(ref material, MaterialMapIndex.Diffuse, texture);
            material.Shader = shader;

            isInitialized = true;
            System.Console.WriteLine("[DebugShaderTestRenderer] Initialization successful");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[DebugShaderTestRenderer] Initialization failed: {ex.Message}");
            isInitialized = false;
        }
    }

    private void CreateFallbackTexture()
    {
        Image image = GenImageColor(64, 64, new Color(255, 165, 0, 255));
        texture = LoadTextureFromImage(image);
        UnloadImage(image);
    }

    /// <summary>
    /// Sets the position of the debug quad in world space.
    /// </summary>
    public void SetPosition(Vector3 newPosition)
    {
        position = newPosition;
    }

    /// <summary>
    /// Sets the scale of the debug quad.
    /// </summary>
    public void SetScale(float newScale)
    {
        scale = newScale;
    }

    /// <summary>
    /// Gets the shader for setting uniforms.
    /// </summary>
    public Shader GetShader() => shader;

    /// <summary>
    /// Renders the debug quad.
    /// </summary>
    public void Draw()
    {
        if (!isInitialized)
            return;

        // Create transformation matrix using proper matrix operations
        Matrix4x4 transform = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position);

        // Set shader uniforms if they exist
        if (ditherPhaseUniformLoc >= 0)
        {
            SetShaderValue(shader, ditherPhaseUniformLoc, 0.0f, ShaderUniformDataType.Float);
        }
        if (alphaUniformLoc >= 0)
        {
            SetShaderValue(shader, alphaUniformLoc, 1.0f, ShaderUniformDataType.Float);
        }

        // Draw the quad mesh
        DrawMesh(quadMesh, material, Matrix4x4.Transpose(transform));
        // DrawModel(testCube, position, 1.0f, Color.White);
    }

    /// <summary>
    /// Unloads all resources.
    /// </summary>
    public void Unload()
    {
        if (texture.Id != 0)
        {
            UnloadTexture(texture);
        }

        if (shader.Id != 0)
        {
            UnloadShader(shader);
        }
    }
}
