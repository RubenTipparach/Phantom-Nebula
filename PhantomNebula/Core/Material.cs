using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Core;

/// <summary>
/// Abstract base class for materials
/// Materials encapsulate shaders, textures, and shader parameters
/// </summary>
public abstract class Material : IDisposable
{
    protected Shader shader;
    protected bool shaderLoaded = false;

    /// <summary>
    /// Apply this material to a Raylib model's material slot
    /// </summary>
    public abstract void ApplyToModel(ref Raylib_cs.Material modelMaterial);

    /// <summary>
    /// Update shader uniforms (called before rendering)
    /// </summary>
    public abstract void UpdateUniforms(Camera3D camera, Vector3 lightDirection);

    /// <summary>
    /// Load the shader for this material
    /// </summary>
    protected abstract void LoadShader();

    public virtual void Dispose()
    {
        if (shaderLoaded)
        {
            UnloadShader(shader);
        }
    }
}
