using Raylib_cs;
using System;
using System.Numerics;
using PhantomNebula.Game;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Renderers;

/// <summary>
/// Renders billboard explosions with scaling and color fade effects using a custom shader.
/// </summary>
public class ExplosionRenderer
{
    private Texture2D explosionTexture;
    private Shader explosionShader;
    private int ditherPhaseUniformLoc;
    private int alphaUniformLoc;
    private bool hasShader = false;
    private Mesh billboardMesh;
    private Material billboardMaterial;

    /// <summary>
    /// Initializes the explosion renderer with the explosion texture and shader.
    /// </summary>
    public ExplosionRenderer(string explosionTexturePath)
    {
        try
        {
            // Load explosion texture
            explosionTexture = LoadTexture(explosionTexturePath);
            if (explosionTexture.Id != 0)
            {
                System.Console.WriteLine($"[ExplosionRenderer] Loaded explosion texture: {explosionTexturePath}");
            }
            else
            {
                System.Console.WriteLine($"[ExplosionRenderer] Failed to load explosion texture");
                CreateFallbackTexture();
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[ExplosionRenderer] Exception loading explosion texture: {ex.Message}");
            CreateFallbackTexture();
        }

        // Create billboard mesh (a simple quad)
        billboardMesh = GenMeshPlane(1.0f, 1.0f, 5, 5);

        // Try to load explosion shader
        try
        {
            explosionShader = LoadShader("Shaders/Explosion.vs", "Shaders/Explosion.fs");
            ditherPhaseUniformLoc = GetShaderLocation(explosionShader, "ditherPhase");
            alphaUniformLoc = GetShaderLocation(explosionShader, "alpha");
            hasShader = true;
            System.Console.WriteLine("[ExplosionRenderer] Loaded explosion shader successfully");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[ExplosionRenderer] Failed to load explosion shader: {ex.Message}");
            hasShader = false;
        }

        // Create material for billboard
        billboardMaterial = LoadMaterialDefault();
        SetMaterialTexture(ref billboardMaterial, MaterialMapIndex.Diffuse, explosionTexture);
        if (hasShader)
        {
            billboardMaterial.Shader = explosionShader;
        }
    }

    private void CreateFallbackTexture()
    {
        // Create a simple fallback texture (orange/yellow gradient)
        Image image = GenImageColor(64, 64, new Color(255, 165, 0, 255));
        explosionTexture = LoadTextureFromImage(image);
        UnloadImage(image);
    }

    /// <summary>
    /// Renders a single explosion billboard.
    /// The billboard faces the camera and displays the explosion texture with scaling and alpha.
    /// </summary>
    public void RenderExplosion(Explosion explosion, Camera3D camera)
    {
        if (!explosion.IsAlive)
            return;

        // Create transformation matrix with scale and position
        float size = explosion.CurrentScale;
        Matrix4x4 transform = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(explosion.Position);

        // Get the color for this frame
        Color explosionColor = GetExplosionColor(explosion);

        // Set shader uniforms if using shader
        if (hasShader)
        {
            SetShaderValue(explosionShader, ditherPhaseUniformLoc, explosion.DitherPhase, ShaderUniformDataType.Float);
            SetShaderValue(explosionShader, alphaUniformLoc, explosion.CurrentAlpha, ShaderUniformDataType.Float);
        }

        // Tint the material with the explosion color
        unsafe
        {
            billboardMaterial.Maps[(int)MaterialMapIndex.Diffuse].Color = explosionColor;
        }

        // Draw the billboard mesh
        DrawMesh(billboardMesh, billboardMaterial, Matrix4x4.Transpose(transform));
    }

    /// <summary>
    /// Gets the color for the explosion based on its lifetime.
    /// Transitions from bright yellow/orange to dark red/black.
    /// </summary>
    private Color GetExplosionColor(Explosion explosion)
    {
        float normalizedLifetime = explosion.Lifetime / explosion.MaxLifetime;

        // Color transition: yellow -> orange -> red -> dark red -> black
        Color color;
        if (normalizedLifetime < 0.2f)
        {
            // Yellow to orange (0-20%)
            float t = normalizedLifetime / 0.2f;
            byte r = (byte)LerpValue(255f, 255f, t);
            byte g = (byte)LerpValue(255f, 165f, t);
            byte b = 0;
            color = new Color((int)r, (int)g, (int)b, 255);
        }
        else if (normalizedLifetime < 0.4f)
        {
            // Orange to red (20-40%)
            float t = (normalizedLifetime - 0.2f) / 0.2f;
            byte r = (byte)LerpValue(255f, 255f, t);
            byte g = (byte)LerpValue(165f, 50f, t);
            byte b = 0;
            color = new Color((int)r, (int)g, (int)b, 255);
        }
        else if (normalizedLifetime < 0.7f)
        {
            // Red to dark (40-70%)
            float t = (normalizedLifetime - 0.4f) / 0.3f;
            byte r = (byte)LerpValue(255f, 150f, t);
            byte g = (byte)LerpValue(50f, 20f, t);
            byte b = 0;
            color = new Color((int)r, (int)g, (int)b, 255);
        }
        else
        {
            // Dark to black with fade (70-100%)
            float t = (normalizedLifetime - 0.7f) / 0.3f;
            byte r = (byte)LerpValue(150f, 50f, t);
            byte g = (byte)LerpValue(20f, 10f, t);
            byte b = 0;
            color = new Color((int)r, (int)g, (int)b, 255);
        }

        return color;
    }

    private float LerpValue(float a, float b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return a + (b - a) * t;
    }

    /// <summary>
    /// Unloads the explosion texture, shader, mesh, and material resources.
    /// </summary>
    public void Unload()
    {
        if (explosionTexture.Id != 0)
        {
            UnloadTexture(explosionTexture);
        }

        if (hasShader)
        {
            UnloadShader(explosionShader);
        }

        // Note: UnloadMesh and UnloadMaterial might be called internally when the objects are GC'd
        // We'll let the Raylib cleanup handle this since Mesh is a struct
    }
}
