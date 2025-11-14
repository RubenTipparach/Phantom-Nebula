using Microsoft.Xna.Framework;
using System;

namespace PhantomSector.Game.Utils;

/// <summary>
/// Vector utility functions for collision detection and physics
/// </summary>
public static class VectorStuff
{
    /// <summary>
    /// Check if there's a collision below (floor collision)
    /// Uses raycasting approach in 2D
    /// </summary>
    /// <param name="rayPositionStart">Starting position of the ray</param>
    /// <param name="maxDistance">Maximum distance to check</param>
    /// <param name="obstacles">List of obstacle positions to check against</param>
    /// <returns>Tuple of (hasCollision, collisionHeight)</returns>
    public static (bool, float) CheckFloorCollision(Vector3 rayPositionStart, float maxDistance, Vector3[] obstacles)
    {
        float floorHeight = rayPositionStart.Y;
        bool result = false;

        // Check if there's an obstacle below
        foreach (var obstacle in obstacles)
        {
            float distance = Vector3.Distance(rayPositionStart, obstacle);

            // Check if obstacle is below and within max distance
            if (obstacle.Y < rayPositionStart.Y && distance <= maxDistance)
            {
                if (obstacle.Y > floorHeight)
                {
                    floorHeight = obstacle.Y;
                    result = true;
                }
            }
        }

        return (result, floorHeight);
    }

    /// <summary>
    /// Check if there's a wall collision in a given direction
    /// </summary>
    /// <param name="rayDirection">Direction to check</param>
    /// <param name="rayPositionStart">Starting position</param>
    /// <param name="stepIncrement">Distance to check</param>
    /// <param name="obstacles">List of obstacles to check against</param>
    /// <returns>True if collision detected</returns>
    public static bool CheckWallCollision(Vector3 rayDirection, Vector3 rayPositionStart, float stepIncrement, Vector3[] obstacles)
    {
        if (rayDirection.Length() < 0.001f) return false;

        Vector3 normalizedDirection = Vector3.Normalize(rayDirection);
        Vector3 rayEnd = rayPositionStart + normalizedDirection * stepIncrement;

        // Check if any obstacle is in the path
        foreach (var obstacle in obstacles)
        {
            float distanceToObstacle = Vector3.Distance(rayPositionStart, obstacle);

            // If obstacle is within step distance, collision detected
            if (distanceToObstacle <= stepIncrement)
            {
                // Check if obstacle is in the direction we're moving
                Vector3 toObstacle = obstacle - rayPositionStart;
                float dot = Vector3.Dot(Vector3.Normalize(toObstacle), normalizedDirection);

                if (dot > 0) // Obstacle is in front
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Check line-of-sight between two points
    /// </summary>
    /// <param name="start">Start position</param>
    /// <param name="end">End position</param>
    /// <param name="obstacles">List of obstacles</param>
    /// <param name="obstacleSize">Radius of obstacles</param>
    /// <returns>True if there's a clear line of sight</returns>
    public static bool CheckLineOfSight(Vector3 start, Vector3 end, Vector3[] obstacles, float obstacleSize = 1f)
    {
        Vector3 direction = end - start;
        float distance = direction.Length();

        if (distance < 0.001f) return true;

        Vector3 normalizedDirection = Vector3.Normalize(direction);

        foreach (var obstacle in obstacles)
        {
            // Project obstacle onto the line
            Vector3 toObstacle = obstacle - start;
            float projectionLength = Vector3.Dot(toObstacle, normalizedDirection);

            // Check if projection is within line segment
            if (projectionLength > 0 && projectionLength < distance)
            {
                // Find closest point on line to obstacle
                Vector3 closestPoint = start + normalizedDirection * projectionLength;
                float distanceToLine = Vector3.Distance(closestPoint, obstacle);

                // If too close, line of sight is blocked
                if (distanceToLine < obstacleSize)
                {
                    return false;
                }
            }
        }

        return true;
    }
}

/// <summary>
/// Axis utility functions
/// </summary>
public static class AxisUtils
{
    /// <summary>
    /// Round a vector to its dominant axis
    /// Returns the vector snapped to the axis it points most toward
    /// </summary>
    /// <param name="vector">The vector to round</param>
    /// <returns>Axis-rounded vector</returns>
    public static Vector3 AxisRound(Vector3 vector)
    {
        int largestIndex = 0;
        float largestValue = Math.Abs(vector.X);

        // Find dominant axis
        if (Math.Abs(vector.Y) > largestValue)
        {
            largestIndex = 1;
            largestValue = Math.Abs(vector.Y);
        }

        if (Math.Abs(vector.Z) > largestValue)
        {
            largestIndex = 2;
        }

        // Create vector pointing along dominant axis
        Vector3 result = Vector3.Zero;
        if (largestIndex == 0)
        {
            result.X = vector.X > 0 ? 1 : -1;
        }
        else if (largestIndex == 1)
        {
            result.Y = vector.Y > 0 ? 1 : -1;
        }
        else
        {
            result.Z = vector.Z > 0 ? 1 : -1;
        }

        return result;
    }

    /// <summary>
    /// Snap vector to nearest cardinal direction
    /// </summary>
    /// <param name="vector">The vector to snap</param>
    /// <returns>Snapped vector (unit length)</returns>
    public static Vector3 SnapToCardinal(Vector3 vector)
    {
        return AxisRound(vector);
    }

    /// <summary>
    /// Get the axis index of the dominant component
    /// 0 = X, 1 = Y, 2 = Z
    /// </summary>
    public static int GetDominantAxis(Vector3 vector)
    {
        float absX = Math.Abs(vector.X);
        float absY = Math.Abs(vector.Y);
        float absZ = Math.Abs(vector.Z);

        if (absX >= absY && absX >= absZ) return 0;
        if (absY >= absZ) return 1;
        return 2;
    }
}
