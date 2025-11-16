using System;
using System.Numerics;

namespace PhantomNebula.Core;

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
    public Vector2 TargetHeading { get; set; } = Vector2.UnitY;
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
        // Normalize both vectors to ensure unit length
        currentHeadingDir = Vector2.Normalize(currentHeadingDir);
        targetHeadingDir = Vector2.Normalize(targetHeadingDir);

        // Rotate current heading by 90 degrees to get perpendicular (left) vector
        // If heading is (x, y), perpendicular left is (-y, x)
        Vector2 perpendicular = new Vector2(-currentHeadingDir.Y, currentHeadingDir.X);

        // Dot product of target with perpendicular determines rotation direction
        // Positive = target is to the left, rotate left
        // Negative = target is to the right, rotate right
        float dotWithPerpendicular = Vector2.Dot(targetHeadingDir, perpendicular);

        // If already aligned, return current heading
        if (Math.Abs(dotWithPerpendicular) < 0.001f)
        {
            return currentHeadingDir;
        }

        // Determine rotation direction based on which side target is on
        float rotationDirection = dotWithPerpendicular > 0 ? 1f : -1f;

        // Convert turn rate (in turns/rotations) to radians
        float rotationAngle = rotationDirection * turnRate * 2f * PI;

        Console.WriteLine($"[CalculateRotation] Current: ({currentHeadingDir.X:F3}, {currentHeadingDir.Y:F3}), Target: ({targetHeadingDir.X:F3}, {targetHeadingDir.Y:F3}), TurnRate: {turnRate:F6}, RotationDir: {rotationDirection}, RotationAngle: {rotationAngle:F6} rad");

        // Apply rotation using rotation matrix
        // [cos(θ)  -sin(θ)] [x]
        // [sin(θ)   cos(θ)] [y]
        float cosRot = MathF.Cos(rotationAngle);
        float sinRot = MathF.Sin(rotationAngle);

        float newX = currentHeadingDir.X * cosRot - currentHeadingDir.Y * sinRot;
        float newY = currentHeadingDir.X * sinRot + currentHeadingDir.Y * cosRot;

        Console.WriteLine($"[CalculateRotation] Result: ({newX:F3}, {newY:F3})");

        return new Vector2(newX, newY);
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

}
