using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace PhantomSector.Game.Utils;

/// <summary>
/// Waypoint direction enumeration
/// </summary>
public enum DirectionWaypoint
{
    CCW = 1,  // Counter-clockwise
    CW = 2    // Clockwise
}

/// <summary>
/// Waypoint visualizer utility
/// Handles waypoint path management and visualization
/// </summary>
public class WaypointVisualizer
{
    public List<Vector3> Waypoints { get; set; } = new();
    public DirectionWaypoint Direction { get; set; } = DirectionWaypoint.CW;

    /// <summary>
    /// Add a waypoint to the path
    /// </summary>
    public void AddWaypoint(Vector3 position)
    {
        Waypoints.Add(position);
    }

    /// <summary>
    /// Remove a waypoint from the path
    /// </summary>
    public void RemoveWaypoint(int index)
    {
        if (index >= 0 && index < Waypoints.Count)
        {
            Waypoints.RemoveAt(index);
        }
    }

    /// <summary>
    /// Clear all waypoints
    /// </summary>
    public void ClearWaypoints()
    {
        Waypoints.Clear();
    }

    /// <summary>
    /// Get total path length
    /// </summary>
    public float GetPathLength()
    {
        if (Waypoints.Count < 2) return 0f;

        float length = 0f;
        for (int i = 0; i < Waypoints.Count - 1; i++)
        {
            length += Vector3.Distance(Waypoints[i], Waypoints[i + 1]);
        }
        return length;
    }

    /// <summary>
    /// Get the next waypoint in the path
    /// </summary>
    public Vector3 GetNextWaypoint(int currentIndex)
    {
        if (Waypoints.Count == 0) return Vector3.Zero;

        int nextIndex = Direction == DirectionWaypoint.CW
            ? (currentIndex + 1) % Waypoints.Count
            : (currentIndex - 1 + Waypoints.Count) % Waypoints.Count;

        return Waypoints[nextIndex];
    }
}
