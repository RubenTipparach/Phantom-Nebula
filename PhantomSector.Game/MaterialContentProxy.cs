using Microsoft.Xna.Framework;

namespace PhantomSector.Game;

/// <summary>
/// Proxy class to handle MaterialContent from the content pipeline
/// This allows models to load without requiring the full pipeline assembly at runtime
/// </summary>
public class MaterialContentProxy
{
    public Vector3? DiffuseColor { get; set; }
    public Vector3? EmissiveColor { get; set; }
    public Vector3? SpecularColor { get; set; }
    public float? SpecularPower { get; set; }
    public float? Alpha { get; set; }
    public string Texture { get; set; }
}
