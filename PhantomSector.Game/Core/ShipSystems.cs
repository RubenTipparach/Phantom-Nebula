using Microsoft.Xna.Framework;
using System;

namespace PhantomSector.Game.Core;

/// <summary>
/// Ship Systems Module
/// Encapsulates firing arc validation, rotation control, and speed management
/// Mirrors the exact implementations from the Lua game systems
/// </summary>
public class ShipSystems
{
    private const float PI = 3.14159265359f;

    // ============================================
    // SHIP PROPERTIES
    // ============================================

    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector2 Heading { get; set; } = Vector2.UnitY;  // Default heading towards +Z
    public float Speed { get; set; } = 0f;
    public float MaxSpeed { get; set; } = 10f;

    // ============================================
    // FIRING ARC SYSTEM
    // ============================================

    /// <summary>
    /// Check if a target is within weapon range
    /// </summary>
    /// <param name="shipPos">Ship position in 3D space</param>
    /// <param name="targetPos">Target position in 3D space</param>
    /// <param name="range">Maximum firing distance</param>
    /// <returns>True if target is in range</returns>
    public bool IsInRange(Vector3 shipPos, Vector3 targetPos, float range)
    {
        float dx = targetPos.X - shipPos.X;
        float dz = targetPos.Z - shipPos.Z;
        float distanceSq = dx * dx + dz * dz;
        float rangeSq = range * range;

        return distanceSq <= rangeSq;
    }

    /// <summary>
    /// Check if a target is within weapon firing arc
    /// Uses simplified 2D dot product test in X,Z space
    /// </summary>
    /// <param name="shipPos">Ship position {x, y, z}</param>
    /// <param name="shipHeading">Ship heading as direction vector {x, z} (unit vector)</param>
    /// <param name="targetPos">Target position {x, y, z}</param>
    /// <param name="arcStart">Left edge of arc in degrees (negative = left)</param>
    /// <param name="arcEnd">Right edge of arc in degrees (positive = right)</param>
    /// <returns>True if target is within firing arc</returns>
    public bool IsInFiringArc(Vector3 shipPos, Vector2 shipHeading, Vector3 targetPos, float arcStart, float arcEnd)
    {
        // Ship forward direction (normalized)
        float shipForwardX = shipHeading.X;
        float shipForwardZ = shipHeading.Y;

        // Vector from ship to target
        float toTargetX = targetPos.X - shipPos.X;
        float toTargetZ = targetPos.Z - shipPos.Z;

        // Normalize to_target
        float toTargetLen = (float)Math.Sqrt(toTargetX * toTargetX + toTargetZ * toTargetZ);
        if (toTargetLen < 0.001f)
        {
            return false;  // Target is at ship position
        }

        toTargetX /= toTargetLen;
        toTargetZ /= toTargetLen;

        // Dot product with ship forward = cosine of angle between them
        float dot = shipForwardX * toTargetX + shipForwardZ * toTargetZ;

        // Calculate actual angle from dot product (in degrees, 0 = aligned with forward)
        float angle = (float)Math.Acos(Math.Max(-1f, Math.Min(1f, dot))) * 180f / PI;

        // Cross product to determine left vs right
        // (ship_forward × to_target) in 2D: forward.x * target.z - forward.z * target.x
        float cross = shipForwardX * toTargetZ - shipForwardZ * toTargetX;

        // If cross < 0, target is to the right (negative angle)
        if (cross < 0)
        {
            angle = -angle;
        }

        // Check if angle is within arc
        return angle >= arcStart && angle <= arcEnd;
    }

    // ============================================
    // ROTATION SYSTEM
    // ============================================

    /// <summary>
    /// Calculate rotation direction and magnitude to target heading
    /// Rotates at constant turn_rate in shortest direction, returns new heading direction vector
    /// </summary>
    /// <param name="currentHeadingDir">Current direction vector (unit vector)</param>
    /// <param name="targetHeadingDir">Target direction vector (unit vector)</param>
    /// <param name="turnRate">Turn rate in turns (e.g., 0.01 for 1% rotation per frame)</param>
    /// <returns>New heading direction after rotation step</returns>
    public Vector2 CalculateRotation(Vector2 currentHeadingDir, Vector2 targetHeadingDir, float turnRate)
    {
        float currentAngle = Atan2(currentHeadingDir.X, currentHeadingDir.Y);
        float targetAngle = Atan2(targetHeadingDir.X, targetHeadingDir.Y);

        // Calculate shortest angular difference (wraps around 0/1 boundary)
        float angleDiff = targetAngle - currentAngle;
        if (angleDiff > 0.5f)
        {
            angleDiff -= 1f;
        }
        else if (angleDiff < -0.5f)
        {
            angleDiff += 1f;
        }

        // Only rotate if not already at target (tolerance: 0.001 turns)
        if (Math.Abs(angleDiff) > 0.001f)
        {
            // Determine direction: positive or negative
            float rotationAmount = angleDiff > 0 ? turnRate : -turnRate;

            // Apply rotation to current angle
            float newAngle = currentAngle + rotationAmount;

            // Convert back to direction vector
            float newX = (float)Math.Cos(newAngle * 2f * PI);
            float newZ = (float)Math.Sin(newAngle * 2f * PI);

            // Normalize to ensure unit vector (handle floating point drift)
            float len = (float)Math.Sqrt(newX * newX + newZ * newZ);
            if (len > 0.0001f)
            {
                newX /= len;
                newZ /= len;
            }

            return new Vector2(newX, newZ);
        }

        return currentHeadingDir;
    }

    // ============================================
    // SPEED SYSTEM
    // ============================================

    /// <summary>
    /// Calculate new ship speed using smooth interpolation (lerp)
    /// </summary>
    /// <param name="currentSpeed">Current ship speed</param>
    /// <param name="targetSpeed">Target ship speed</param>
    /// <param name="speedSmoothing">Smoothing factor (e.g., 0.08 for smooth interpolation)</param>
    /// <returns>New ship speed after interpolation</returns>
    public float CalculateSpeed(float currentSpeed, float targetSpeed, float speedSmoothing)
    {
        // Smooth ship speed (lerp towards target)
        return currentSpeed + (targetSpeed - currentSpeed) * speedSmoothing;
    }

    /// <summary>
    /// Calculate movement offset based on speed and direction
    /// Move ship in direction of heading based on speed
    /// </summary>
    /// <param name="shipSpeed">Current ship speed (0-1 normalized)</param>
    /// <param name="shipHeadingDir">Ship heading direction (unit vector)</param>
    /// <param name="maxSpeed">Maximum speed constant</param>
    /// <returns>Movement offset for this frame</returns>
    public Vector2 CalculateMovement(float shipSpeed, Vector2 shipHeadingDir, float maxSpeed)
    {
        float moveSpeed = shipSpeed * maxSpeed * 0.1f;  // Scale for reasonable movement

        return new Vector2(
            shipHeadingDir.X * moveSpeed,
            shipHeadingDir.Y * moveSpeed
        );
    }

    // ============================================
    // HELPER FUNCTIONS
    // ============================================

    /// <summary>
    /// Atan2 implementation that returns angle in turns (0-1 range)
    /// 0 = +Z axis, 0.25 = +X axis, 0.5 = -Z axis, 0.75 = -X axis
    /// </summary>
    private float Atan2(float x, float z)
    {
        float angle = (float)Math.Atan2(x, z) / (2f * PI);
        if (angle < 0)
        {
            angle += 1f;
        }
        return angle;
    }
}
