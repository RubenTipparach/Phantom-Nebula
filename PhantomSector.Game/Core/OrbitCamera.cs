using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PhantomSector.Game.Core;

/// <summary>
/// Orbit camera that rotates around a target point
/// </summary>
public class OrbitCamera : Camera
{
    public float Distance { get; set; } = 25f;
    public float MinDistance { get; set; } = 5f;
    public float MaxDistance { get; set; } = 100f;

    public float RotationSpeed { get; set; } = 0.01f;
    public float ZoomSpeed { get; set; } = 0.01f;

    private MouseState _previousMouseState;

    public OrbitCamera(GraphicsDevice graphicsDevice) : base(graphicsDevice)
    {
        Target = Vector3.Zero;
    }

    public void SetTarget(Vector3 target)
    {
        Target = target;
    }

    public override void Update(GameTime gameTime)
    {
        // Calculate position based on distance and angles
        Position = new Vector3(
            (float)(System.Math.Cos(yaw) * System.Math.Cos(pitch)) * Distance,
            (float)System.Math.Sin(pitch) * Distance,
            (float)(System.Math.Sin(yaw) * System.Math.Cos(pitch)) * Distance
        ) + Target;

        // Forward points from camera position toward target (inward)
        Forward = Vector3.Normalize(Target - Position);
        Right = Vector3.Normalize(Vector3.Cross(Forward, Up));

        // Don't call base.Update() as it would overwrite Forward with first-person camera logic
    }

    public void HandleInput(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();

        // Camera rotation with right mouse button
        if (mouseState.RightButton == ButtonState.Pressed)
        {
            if (_previousMouseState.RightButton == ButtonState.Pressed)
            {
                float deltaX = (mouseState.X - _previousMouseState.X) * RotationSpeed;
                float deltaY = (mouseState.Y - _previousMouseState.Y) * RotationSpeed;

                yaw -= deltaX;
                pitch -= deltaY;
                pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
            }
        }

        // Zoom with mouse wheel
        if (mouseState.ScrollWheelValue != _previousMouseState.ScrollWheelValue)
        {
            Distance -= (mouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue) * ZoomSpeed;
            Distance = MathHelper.Clamp(Distance, MinDistance, MaxDistance);
        }

        _previousMouseState = mouseState;
    }

    public void SetOrbitAngles(float yawAngle, float pitchAngle)
    {
        yaw = yawAngle;
        pitch = pitchAngle;
    }
}
