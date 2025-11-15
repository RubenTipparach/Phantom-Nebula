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

    // Movement
    private Vector2 heading = Vector2.UnitY;
    private float targetSpeed = 0f;
    private float currentSpeed = 0f;
    private const float SpeedSmoothing = 0.08f;
    private const float TurnRate = 0.02f;

    public Ship(Vector3 initialPosition, float scale = 1.0f) : base(initialPosition, new Vector3(scale, scale, scale), "Ship")
    {
        Health.Init();
        Systems.Position = Transform.Position;
        Systems.Heading = heading;
        Systems.Speed = currentSpeed;
    }

    /// <summary>
    /// Update ship state (input, movement)
    /// </summary>
    public override void Update(float deltaTime)
    {
        // Update heading with smooth rotation
        heading = Systems.CalculateRotation(heading, Systems.Heading, TurnRate);

        // Update speed with smooth interpolation
        currentSpeed = Systems.CalculateSpeed(currentSpeed, targetSpeed, SpeedSmoothing);

        // Calculate movement offset
        Vector2 movement = Systems.CalculateMovement(currentSpeed, heading, Systems.MaxSpeed);

        // Update position
        Transform.Position += new Vector3(movement.X, 0, movement.Y);

        // Update systems state
        Systems.Position = Transform.Position;
        Systems.Speed = currentSpeed;
        Systems.Heading = heading;

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
        return Systems.IsInFiringArc(Transform.Position, heading, targetPosition, arcStart, arcEnd);
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

    /// <summary>
    /// Check if ship is destroyed
    /// </summary>
    public bool IsDestroyed => Health.IsDead;
}
