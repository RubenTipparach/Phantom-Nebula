using Microsoft.Xna.Framework;
using System;

namespace PhantomSector.Game.Utils;

/// <summary>
/// Utility class for rounding vector positions
/// Rounds X and Z coordinates to nearest integer, preserving Y
/// </summary>
public static class RoundOffStuff
{
    /// <summary>
    /// Round the X and Z components of a vector to the nearest integer
    /// Y component is preserved as-is
    /// </summary>
    /// <param name="position">The position vector to round</param>
    /// <returns>Rounded position vector</returns>
    public static Vector3 RoundPosition(Vector3 position)
    {
        return new Vector3(
            (float)Math.Round(position.X),
            position.Y,
            (float)Math.Round(position.Z)
        );
    }

    /// <summary>
    /// Round multiple positions to nearest integer
    /// </summary>
    /// <param name="positions">Array of positions to round</param>
    /// <returns>Array of rounded positions</returns>
    public static Vector3[] RoundPositions(Vector3[] positions)
    {
        if (positions == null) return null;

        Vector3[] rounded = new Vector3[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            rounded[i] = RoundPosition(positions[i]);
        }
        return rounded;
    }
}
