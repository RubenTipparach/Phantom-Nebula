using System;
using System.Numerics;
using PhantomNebula.Utils;

namespace PhantomNebula.Core;

/// <summary>
/// Ship entity with simplified kinematic movement
/// Inherits from Entity for transform-based positioning
/// </summary>
public class Ship : Entity
{
    // Ship systems
    public ShipSystems Systems { get; private set; } = new();
    public HealthStats Health { get; private set; } = new();

    // Ship renderer
    public ShipRenderer Renderer { get; private set; }


    private float targetSpeed = 0f;
    private float currentSpeed = 0f;
    private const float SpeedSmoothing = 0.08f;
    private const float TurnRate = 0.0002f;

    public Ship(Vector3 initialPosition, float scale = 1.0f) : base(initialPosition, new Vector3(scale, scale, scale), "Ship")
    {
        Health.Init();
        Systems.Position = Transform.Position;
        Systems.Heading = new Vector2(Transform.Forward.X, Transform.Forward.Z);
        Systems.Speed = currentSpeed;

        // Initialize renderer
        Renderer = new ShipRenderer(Transform.Position, scale);
    }

    /// <summary>
    /// Update ship state (input, movement)
    /// </summary>
    public override void Update(float deltaTime)
    {
        // Update heading with smooth rotation
        var offsetHeading = Systems.CalculateRotation(Systems.Heading, Systems.TargetHeading, TurnRate);

        // Update speed with smooth interpolation
        currentSpeed = Systems.CalculateSpeed(currentSpeed, targetSpeed, SpeedSmoothing);

        // Update systems state
        Systems.Position = Transform.Position;
        Systems.Speed = currentSpeed;
        // Note: Don't overwrite Systems.Heading here - it holds the target heading that CalculateRotation uses!

        // Update renderer position and rotation
        Renderer.UpdatePosition(Transform.Position);

        // Convert 2D heading vector to rotation angle (around Y axis)
        float rotationAngle = MathF.Atan2(offsetHeading.X, offsetHeading.Y);
        Renderer.UpdateRotation(rotationAngle);

        // Call base update for children
        base.Update(deltaTime);
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
        return Systems.IsInRange(Transform.Position, targetPosition, range);
    }

    /// <summary>
    /// Check if a target is within the firing arc
    /// </summary>
    public bool IsTargetInArc(Vector3 targetPosition, float arcStart, float arcEnd)
    {
        return Systems.IsInFiringArc(Transform.Position, Systems.Heading, targetPosition, arcStart, arcEnd);
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
