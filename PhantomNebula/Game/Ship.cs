using System;
using System.Numerics;
using PhantomNebula.Utils;
using PhantomNebula.Core;
using PhantomNebula.Renderers;
using Raylib_cs;

namespace PhantomNebula.Game;

/// <summary>
/// Ship entity with simplified kinematic movement
/// Extends Transform for transform-based positioning
/// </summary>
public class Ship : Core.Transform, IDisposable
{
    // Ship systems
    public ShipSystems Systems { get; private set; } = new();
    public HealthStats Health { get; private set; } = new();

    // Ship renderer
    public ShipRenderer Renderer { get; private set; }

    private float targetSpeed = 0f;
    private float currentSpeed = 0f;
    private const float SpeedSmoothing = 0.08f;
    private const float TurnRate = 0.4f;
    private const float MaxRollAngle = 0.2f; // Maximum roll in radians (~28 degrees)
    private const float RollSpeed = 0.1f; // How fast roll interpolates

    // Track yaw and roll separately
    private Quaternion yawRotation = Quaternion.Identity; // Yaw-only rotation around world Y-axis
    private float currentRoll = 0f; // Current roll angle

    public Ship(Vector3 initialPosition, float scale = 1.0f)
    {
        Position = initialPosition;
        Scale = new Vector3(scale, scale, scale);

        Health.Init();
        Systems.Position = Position;
        Systems.Heading = new Vector2(Forward.X, Forward.Z);
        Systems.Speed = currentSpeed;

        // Initialize renderer
        Renderer = new ShipRenderer();
    }

    /// <summary>
    /// Update ship state (input, movement)
    /// </summary>
    public void Update(float deltaTime)
    {
        // Get current heading from forward vector projected onto XZ plane
        Vector2 currentHeading = Vector2.Normalize(new Vector2(Forward.X, Forward.Z));

        // Calculate angle to target
        float dotProduct = Vector2.Dot(currentHeading, Systems.TargetHeading);
        float angleToTarget = MathF.Acos(Math.Clamp(dotProduct, -1f, 1f));

        // Update heading with smooth rotation
        var rotationAngleY = Systems.CalculateRotation(currentHeading, Systems.TargetHeading, TurnRate * deltaTime);

        // Prevent overshooting - clamp rotation if we're close to target
        const float stopThreshold = 0.01f; // ~0.57 degrees
        if (angleToTarget < stopThreshold)
        {
            rotationAngleY = 0f; // Stop rotating
        }
        else if (MathF.Abs(rotationAngleY) > angleToTarget)
        {
            // About to overshoot - clamp to exact angle remaining
            rotationAngleY = angleToTarget * MathF.Sign(rotationAngleY);
        }

        // Update speed with smooth interpolation
        currentSpeed = Systems.CalculateSpeed(currentSpeed, targetSpeed, SpeedSmoothing);

        // Calculate target roll based on rotation direction
        // Positive rotationAngle = turning right, negative = turning left
        // Roll is opposite: right turn = negative roll (bank right), left turn = positive roll (bank left)
        float targetRoll = 0f;
        if (MathF.Abs(rotationAngleY) > 0.001f)
        {
            // Turning - apply roll in opposite direction of rotation
            targetRoll = -MathF.Sign(rotationAngleY) * MaxRollAngle;
        }

        // Smoothly interpolate current roll towards target roll
        currentRoll += (targetRoll - currentRoll) * RollSpeed;

        // Apply yaw rotation around WORLD Y-axis (always global up)
        Quaternion yawDelta = Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotationAngleY);
        yawRotation = yawDelta * yawRotation;

        // Combine yaw rotation with roll around forward axis
        // First apply yaw, then roll around the resulting forward direction
        Vector3 forwardDir = Vector3.Transform(Vector3.UnitZ, yawRotation);
        Quaternion rollRotation = Quaternion.CreateFromAxisAngle(forwardDir, currentRoll);
        Rotation = rollRotation * yawRotation;

        // Update systems state at end with current transform values (AFTER rotation applied)
        Systems.Position = Position;
        Systems.Speed = currentSpeed;
        Systems.Heading = Vector2.Normalize(new Vector2(Forward.X, Forward.Z));

        // Debug output
        //Console.WriteLine($"[Ship] Rotation: ({Rotation.X:F3}, {Rotation.Y:F3}, {Rotation.Z:F3}) | Forward: ({Forward.X:F3}, {Forward.Y:F3}, {Forward.Z:F3}) | Heading: ({Systems.Heading.X:F3}, {Systems.Heading.Y:F3})");
    }

    /// <summary>
    /// Draw ship using renderer
    /// </summary>
    public void Draw(Camera3D camera, Vector3 lightDirection)
    {
        Renderer.Draw(this, camera, lightDirection);
    }

    /// <summary>
    /// Set target speed for the ship (0-1 normalized)
    /// </summary>
    public void SetTargetSpeed(float speed)
    {
        targetSpeed = float.Clamp(speed, 0f, 1f);
    }

    /// <summary>
    /// Set target heading direction
    /// </summary>
    public void SetHeading(Vector2 direction)
    {
        // Normalize and set as target
        if (direction.Length() > 0.001f)
        {
            Systems.Heading = Vector2.Normalize(direction);
        }
    }

    /// <summary>
    /// Check if a target is within firing range
    /// </summary>
    public bool IsTargetInRange(Vector3 targetPosition, float range)
    {
        return Systems.IsInRange(Position, targetPosition, range);
    }

    /// <summary>
    /// Check if a target is within the firing arc
    /// </summary>
    public bool IsTargetInArc(Vector3 targetPosition, float arcStart, float arcEnd)
    {
        return Systems.IsInFiringArc(Position, Systems.Heading, targetPosition, arcStart, arcEnd);
    }

    /// <summary>
    /// Dispose ship resources
    /// </summary>
    public void Dispose()
    {
        Renderer.Dispose();
    }

    /// <summary>
    /// Deal damage to the ship
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        Health.TakeDamage(damageAmount);
    }

    /// <summary>
    /// Heal the ship
    /// </summary>
    public void Heal(float healAmount)
    {
        Health.Heal(healAmount);
    }

    public void SetTargetHeading(Vector2 heading2D)
    {
        Systems.TargetHeading = heading2D;
    }

    /// <summary>
    /// Check if ship is destroyed
    /// </summary>
    public bool IsDestroyed => Health.IsDead;
}
