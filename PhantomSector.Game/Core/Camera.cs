using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhantomSector.Game.Core;

/// <summary>
/// Abstract base class for 3D cameras
/// </summary>
public abstract class Camera
{
    protected float yaw, pitch;
    private bool rotationSetExternally = false;
    protected bool IsRotationLocked => rotationSetExternally;

    public Vector3 Position { get; set; }
    public Vector3 Target { get; protected set; }
    public Vector3 Up { get; protected set; } = Vector3.Up;
    public Vector3 Forward { get; protected set; }
    public Vector3 Right { get; protected set; }

    public Matrix View => Matrix.CreateLookAt(Position, Target, Up);
    public Matrix Projection { get; protected set; }

    public Vector2 NearFarPlane = new Vector2(1, 10000f);

    public Camera(GraphicsDevice graphicsDevice)
    {
        float aspectRatio = graphicsDevice.Viewport.AspectRatio;
        Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, NearFarPlane.X, NearFarPlane.Y);
    }

    public virtual void Update(GameTime gameTime)
    {
        if (!rotationSetExternally)
        {
            Forward = Vector3.Normalize(Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(yaw, pitch, 0)));
            Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.Up));
        }

        rotationSetExternally = false;
    }

    /// <summary>
    /// Gets the camera's world rotation as a quaternion.
    /// </summary>
    public virtual Quaternion GetRotation()
    {
        return Quaternion.CreateFromRotationMatrix(Matrix.Invert(View));
    }

    /// <summary>
    /// Sets the camera's world rotation from a quaternion.
    /// </summary>
    public virtual void SetRotation(Quaternion rotation)
    {
        Forward = Vector3.Normalize(Vector3.Transform(Vector3.Forward, rotation));
        Right = Vector3.Normalize(Vector3.Transform(Vector3.Right, rotation));
        Target = Position + Forward;

        yaw = (float)Math.Atan2(-Forward.X, -Forward.Z);
        pitch = (float)Math.Asin(Forward.Y);

        rotationSetExternally = true;
    }

    public void UpdateProjection(GraphicsDevice graphicsDevice)
    {
        float aspectRatio = graphicsDevice.Viewport.AspectRatio;
        Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, NearFarPlane.X, NearFarPlane.Y);
    }
}
