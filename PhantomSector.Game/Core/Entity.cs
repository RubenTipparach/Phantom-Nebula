using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PhantomSector.Game.Core;

/// <summary>
/// Basic game object that renders a 3D model
/// </summary>
public class Entity
{
    protected Model myModel;
    protected Matrix[] transforms;
    protected Effect customEffect;
    protected Texture2D emissiveTexture;

    protected Vector3 modelPosition;
    public Vector3 Position
    {
        get { return modelPosition; }
        set { modelPosition = value; }
    }

    protected Matrix modelRotation;
    public Matrix Rotation
    {
        get { return modelRotation; }
        set { modelRotation = value; }
    }

    protected Vector3 color;
    public Vector3 Color
    {
        get { return color; }
        set { color = value; }
    }

    protected bool shaded;
    public bool Shaded
    {
        get { return shaded; }
        set { shaded = value; }
    }

    protected float scale;
    public float Scale
    {
        get { return scale; }
        set { scale = value; }
    }

    public Entity(ContentManager content, string modelPath)
    {
        modelPosition = Vector3.Zero;
        modelRotation = Matrix.Identity;
        color = new Vector3(0.64f, 0.64f, 0.64f);
        shaded = true;
        scale = 1.0f;

        LoadModel(content, modelPath);
    }

    private void LoadModel(ContentManager content, string modelPath)
    {
        try
        {
            // Remove .xnb extension if present
            if (modelPath.EndsWith(".xnb"))
            {
                modelPath = modelPath.Substring(0, modelPath.LastIndexOf(".xnb"));
            }

            myModel = content.Load<Model>(modelPath);
            transforms = new Matrix[myModel.Bones.Count];
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Entity] Failed to load model '{modelPath}': {e.Message}");
            throw;
        }
    }

    public void SetModel(ContentManager content, string modelPath)
    {
        LoadModel(content, modelPath);
    }

    public void SetCustomEffect(Effect effect)
    {
        customEffect = effect;
    }

    public void SetEmissiveTexture(Texture2D texture)
    {
        emissiveTexture = texture;
    }

    public virtual void Draw(Camera camera)
    {
        if (myModel == null) return;

        myModel.CopyAbsoluteBoneTransformsTo(transforms);

        Matrix worldMatrix = Matrix.CreateScale(scale)
            * modelRotation
            * Matrix.CreateTranslation(modelPosition);

        foreach (ModelMesh mesh in myModel.Meshes)
        {
            Matrix meshWorld = transforms[mesh.ParentBone.Index] * worldMatrix;

            // Use custom effect if set, otherwise use BasicEffect
            if (customEffect != null)
            {
                // Set custom shader parameters
                customEffect.Parameters["World"]?.SetValue(meshWorld);
                customEffect.Parameters["View"]?.SetValue(camera.View);
                customEffect.Parameters["Projection"]?.SetValue(camera.Projection);

                // Set light direction (matching planet shader)
                Vector3 lightDir = new Vector3(1, -1, 1);
                lightDir.Normalize();
                customEffect.Parameters["LightDirection"]?.SetValue(lightDir);

                // Set emissive texture if available
                if (emissiveTexture != null)
                {
                    customEffect.Parameters["EmissiveTexture"]?.SetValue(emissiveTexture);
                }

                // Apply custom effect to all mesh parts
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = customEffect;
                }

                mesh.Draw();
            }
            else
            {
                // Use default BasicEffect rendering
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = shaded;

                    if (shaded)
                    {
                        effect.DiffuseColor = color;
                        effect.DirectionalLight0.Enabled = true;
                        effect.DirectionalLight0.DiffuseColor = new Vector3(0.7f, 0.7f, 0.7f);

                        Vector3 lightAngle = new Vector3(1, -1, 1);
                        lightAngle.Normalize();
                        effect.DirectionalLight0.Direction = lightAngle;

                        effect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
                    }
                    else
                    {
                        effect.DiffuseColor = color;
                        effect.AmbientLightColor = new Vector3(1, 1, 1);
                    }

                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    effect.World = meshWorld;
                }

                mesh.Draw();
            }
        }
    }
}
