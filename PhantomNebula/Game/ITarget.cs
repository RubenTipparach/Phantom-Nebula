using System.Numerics;
using PhantomNebula.Utils;

namespace PhantomNebula.Game;

/// <summary>
/// Health interface for entities with health
/// Provides read-only access to health information for UI and targeting systems
/// </summary>
public interface IHealth
{
    /// <summary>
    /// Current health value (0 to startingHealth)
    /// </summary>
    float CurrentHealth { get; }

    /// <summary>
    /// Starting/maximum health value
    /// </summary>
    float StartingHealth { get; }

    /// <summary>
    /// Health as percentage (0 to 1)
    /// </summary>
    float HealthPercent { get; }

    /// <summary>
    /// Check if entity is dead
    /// </summary>
    bool IsDead { get; }
}

/// <summary>
/// Target interface for entities that can be targeted
/// Provides targeting information including position, name, and health
/// </summary>
public interface ITarget : IHealth
{
    /// <summary>
    /// Unique name/identifier for the target
    /// </summary>
    string TargetName { get; }

    /// <summary>
    /// World position of the target
    /// </summary>
    Vector3 Position { get; }

    /// <summary>
    /// Take damage
    /// </summary>
    void TakeDamage(float damageAmount);
}
