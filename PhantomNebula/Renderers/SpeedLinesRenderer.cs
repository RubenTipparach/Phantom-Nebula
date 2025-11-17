using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Renderers;

/// <summary>
/// Renders 3D speed lines around the ship to convey speed and direction
/// </summary>
public class SpeedLinesRenderer
{
    private struct SpeedLine
    {
        public Vector3 WorldPosition; // Fixed world position where line spawned
        public Vector3 RelativeDirection; // Fixed world position where line spawned

        public float Length;
        public float Lifetime; // Time this line has been alive
        public bool Active; // Whether this line should be drawn
    }

    private SpeedLine[] speedLines;
    private Random random;

    // Configuration
    private const int LineCount = 200;
    private const float SpawnRadius = 15.0f; // Radius around ship where lines spawn
    private const float LineLength = 1.0f;
    private const float MinLineLength = 0.1f; // Minimum line length
    private const float MinSpeedThreshold = 0.1f; // Don't spawn lines below this speed
    private const float MaxLifetime = 10.0f; // Lines expire after 10 seconds
    private const float MaxDistance = 15.0f; // Deactivate lines that fall too far behind ship
    private const float SpawnRate = 0.02f; // Probability of spawning per frame (balanced with lifetime)

    public SpeedLinesRenderer()
    {
        random = new Random();
        speedLines = new SpeedLine[LineCount];

        // Initialize speed lines as inactive
        for (int i = 0; i < LineCount; i++)
        {
            speedLines[i] = new SpeedLine { Active = false };
        }
    }

    private SpeedLine CreateRandomLine(Vector3 shipPosition, Vector3 shipForward, float shipSpeed)
    {
        // Generate random point in a sphere using spherical coordinates
        float theta = (float)(random.NextDouble() * Math.PI * 2); // Azimuthal angle (0 to 2π)
        float phi = (float)(Math.Acos(2.0 * random.NextDouble() - 1.0)); // Polar angle (0 to π) - uniform distribution
        float radius = (float)(Math.Pow(random.NextDouble(), 1.0/3.0) * SpawnRadius); // Cube root for uniform volume distribution

        // Convert spherical to Cartesian coordinates
        float x = radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
        float y = radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);
        float z = radius * (float)Math.Cos(phi);

        Vector3 sphereOffset = new Vector3(x, y, z);

        // Add forward bias based on ship speed - spawn lines ahead of ship
        Vector3 forwardBias = Vector3.Normalize(shipForward) * (shipSpeed * SpawnRadius);

        // Calculate world position (biased toward front of ship based on velocity)
        Vector3 worldPos = shipPosition + sphereOffset + forwardBias;

        // Calculate length with minimum threshold
        float lineLength = Math.Max(MinLineLength, LineLength * shipSpeed);

        // Random initial lifetime offset to stagger despawning
        float initialLifetime = (float)(random.NextDouble() * MaxLifetime);

        return new SpeedLine
        {
            WorldPosition = worldPos, // Fixed world position where line spawned
            RelativeDirection = shipForward,
            Length = lineLength, // Length depends on speed at spawn time with minimum
            Lifetime = -initialLifetime, // Start with negative lifetime for staggered spawning
            Active = true
        };
    }

    public void Update(Vector3 shipPosition, Vector3 shipForward, float shipSpeed, float deltaTime)
    {
        for (int i = 0; i < LineCount; i++)
        {
            // Update lifetime for active lines
            if (speedLines[i].Active)
            {
                var line = speedLines[i];
                line.Lifetime += deltaTime;

                // Check distance from ship
                float distance = Vector3.Distance(line.WorldPosition, shipPosition);

                // Deactivate if expired or too far away
                if (line.Lifetime >= MaxLifetime || distance > MaxDistance)
                {
                    line.Active = false;
                    speedLines[i] = line;
                    continue;
                }

                speedLines[i] = line;
            }
            // Spawn new lines if ship is moving fast enough and slot is inactive
            else if (shipSpeed >= MinSpeedThreshold)
            {
                // Spawn new lines at a balanced rate with despawn (lifetime-based)
                if (random.NextDouble() < SpawnRate)
                {
                    speedLines[i] = CreateRandomLine(shipPosition, shipForward, shipSpeed);
                }
            }
        }
    }

    public void Draw(Vector3 shipForward)
    {
        for (int i = 0; i < LineCount; i++)
        {
            SpeedLine line = speedLines[i];

            // Skip inactive lines
            if (!line.Active)
                continue;

            // Line start is fixed world position where it spawned
            Vector3 lineStart = line.WorldPosition;

            // Line end updates each frame based on current ship's forward direction
            Vector3 lineEnd = lineStart - shipForward * line.Length;
            // Vector3 lineEnd = lineStart - line.RelativeDirection * line.Length;

            // Fade based on lifetime
            float alpha = 1.0f;

            // Fade in during negative lifetime (first appearance)
            if (line.Lifetime < 0.0f)
            {
                alpha = 0.0f; // Don't show lines with negative lifetime
            }
            // Fade out in last 2 seconds
            else if (line.Lifetime > MaxLifetime - 2.0f)
            {
                alpha = (MaxLifetime - line.Lifetime) / 2.0f;
            }

            // Draw the line
            Color lineColor = new Color(
                200,
                220,
                255,
                (int)(alpha * 200) // Brighter alpha for better visibility
            );

            DrawLine3D(lineStart, lineEnd, lineColor);
        }
    }
}
