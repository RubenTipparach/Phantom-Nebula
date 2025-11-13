using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Forms.NET.Controls;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace PhantomSector.Editor;

public class GameViewControl : MonoGame.Forms.NET.Controls.MonoGameControl
{
    private VertexPositionColor[] cubeVertices = Array.Empty<VertexPositionColor>();
    private short[] cubeIndices = Array.Empty<short>();
    private BasicEffect? effect;
    private Matrix world;
    private Matrix view;
    private Matrix projection;

    private Quaternion currentRotation = Quaternion.Identity;
    private Quaternion targetRotation = Quaternion.Identity;

    private float interpolationProgress = 1f;
    private const float InterpolationSpeed = 5f; // Higher = faster interpolation

    private float _rotationX = 0f;
    private float _rotationY = 0f;
    private float _rotationZ = 0f;

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    [System.ComponentModel.Browsable(false)]
    public float RotationX
    {
        get => _rotationX;
        set
        {
            if (interpolationProgress >= 1f)
            {
                currentRotation = targetRotation;
            }
            _rotationX = value;
            targetRotation = Quaternion.CreateFromYawPitchRoll(
                MathHelper.ToRadians(_rotationY),
                MathHelper.ToRadians(_rotationX),
                MathHelper.ToRadians(_rotationZ)
            );
            interpolationProgress = 0f;
        }
    }

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    [System.ComponentModel.Browsable(false)]
    public float RotationY
    {
        get => _rotationY;
        set
        {
            if (interpolationProgress >= 1f)
            {
                currentRotation = targetRotation;
            }
            _rotationY = value;
            targetRotation = Quaternion.CreateFromYawPitchRoll(
                MathHelper.ToRadians(_rotationY),
                MathHelper.ToRadians(_rotationX),
                MathHelper.ToRadians(_rotationZ)
            );
            interpolationProgress = 0f;
        }
    }

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    [System.ComponentModel.Browsable(false)]
    public float RotationZ
    {
        get => _rotationZ;
        set
        {
            if (interpolationProgress >= 1f)
            {
                currentRotation = targetRotation;
            }
            _rotationZ = value;
            targetRotation = Quaternion.CreateFromYawPitchRoll(
                MathHelper.ToRadians(_rotationY),
                MathHelper.ToRadians(_rotationX),
                MathHelper.ToRadians(_rotationZ)
            );
            interpolationProgress = 0f;
        }
    }

    protected override void Initialize()
    {
        // Initialize camera matrices
        view = Matrix.CreateLookAt(
            new Vector3(0, 0, 5),
            Vector3.Zero,
            Vector3.Up
        );

        projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45),
            Editor.GraphicsDevice.Viewport.AspectRatio,
            0.1f,
            100f
        );

        // Initialize basic effect
        effect = new BasicEffect(Editor.GraphicsDevice)
        {
            VertexColorEnabled = true,
            View = view,
            Projection = projection
        };

        // Create vertex-colored cube
        CreateCube();
    }

    private void CreateCube()
    {
        // Define the 8 vertices of a cube with different colors
        cubeVertices = new VertexPositionColor[8];

        // Front face vertices
        cubeVertices[0] = new VertexPositionColor(new Vector3(-1, -1, -1), XnaColor.Red);     // Bottom-left
        cubeVertices[1] = new VertexPositionColor(new Vector3(1, -1, -1), XnaColor.Green);    // Bottom-right
        cubeVertices[2] = new VertexPositionColor(new Vector3(-1, 1, -1), XnaColor.Blue);     // Top-left
        cubeVertices[3] = new VertexPositionColor(new Vector3(1, 1, -1), XnaColor.Yellow);    // Top-right

        // Back face vertices
        cubeVertices[4] = new VertexPositionColor(new Vector3(-1, -1, 1), XnaColor.Magenta);  // Bottom-left
        cubeVertices[5] = new VertexPositionColor(new Vector3(1, -1, 1), XnaColor.Cyan);      // Bottom-right
        cubeVertices[6] = new VertexPositionColor(new Vector3(-1, 1, 1), XnaColor.Orange);    // Top-left
        cubeVertices[7] = new VertexPositionColor(new Vector3(1, 1, 1), XnaColor.White);      // Top-right

        // Define the 36 indices for 12 triangles (6 faces * 2 triangles per face)
        // Reversed winding order (clockwise instead of counter-clockwise)
        cubeIndices = new short[]
        {
            // Front face
            0, 1, 2,
            1, 3, 2,

            // Back face
            5, 4, 7,
            4, 6, 7,

            // Left face
            4, 0, 6,
            0, 2, 6,

            // Right face
            1, 5, 3,
            5, 7, 3,

            // Top face
            2, 3, 6,
            3, 7, 6,

            // Bottom face
            4, 5, 0,
            5, 1, 0
        };
    }

    protected override void Update(GameTime gameTime)
    {
        // Interpolate rotation using Slerp if needed
        if (interpolationProgress < 1f)
        {
            interpolationProgress += (float)gameTime.ElapsedGameTime.TotalSeconds * InterpolationSpeed;
            if (interpolationProgress > 1f)
            {
                interpolationProgress = 1f;
            }

            // Use Slerp for smooth quaternion interpolation
            Quaternion interpolatedRotation = Quaternion.Slerp(currentRotation, targetRotation, interpolationProgress);
            world = Matrix.CreateFromQuaternion(interpolatedRotation);
        }
        else
        {
            // No interpolation needed, use target rotation directly
            world = Matrix.CreateFromQuaternion(targetRotation);
        }

        if (effect != null)
        {
            effect.World = world;
        }
    }

    protected override void Draw()
    {
        Editor.GraphicsDevice.Clear(XnaColor.CornflowerBlue);

        if (effect != null)
        {
            // Draw the cube
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Editor.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    cubeVertices,
                    0,
                    cubeVertices.Length,
                    cubeIndices,
                    0,
                    cubeIndices.Length / 3
                );
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            effect?.Dispose();
        }
        base.Dispose(disposing);
    }
}
