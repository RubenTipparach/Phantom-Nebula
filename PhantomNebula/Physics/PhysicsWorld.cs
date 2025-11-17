using System;
using System.Collections.Generic;
using System.Numerics;

namespace PhantomNebula.Physics;

/// <summary>
/// Simplified wrapper for physics colliders (debug visualization)
/// Stores collider information for rendering wireframes
/// Full BepuPhysics integration can be added later when needed
/// </summary>
public class PhysicsWorld : IDisposable
{
    private Dictionary<string, ColliderData> colliders = new();

    /// <summary>
    /// Collider data for storage and visualization
    /// </summary>
    public class ColliderData
    {
        public string Name { get; set; } = "";
        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public ColliderShape Shape { get; set; } = new SphereCollider(1f);
        public ColliderType Type { get; set; }

        public ColliderData() { }

        public ColliderData(string name, Vector3 position, Vector3 scale, ColliderShape shape, ColliderType type)
        {
            Name = name;
            Position = position;
            Scale = scale;
            Shape = shape;
            Type = type;
        }
    }

    public enum ColliderType
    {
        Static,
        Kinematic,
        Dynamic
    }

    /// <summary>
    /// Add a static collider (e.g., planet) at the given position
    /// </summary>
    public void AddStaticCollider(string name, Vector3 position, Vector3 scale, ColliderShape shape)
    {
        colliders[name] = new ColliderData(name, position, scale, shape, ColliderType.Static);
        Console.WriteLine($"[PhysicsWorld] Added static collider '{name}' at {position}");
    }

    /// <summary>
    /// Add a dynamic/kinematic collider (e.g., ship, satellite)
    /// </summary>
    public void AddKinematicCollider(string name, Vector3 position, Vector3 scale, ColliderShape shape)
    {
        colliders[name] = new ColliderData(name, position, scale, shape, ColliderType.Kinematic);
        Console.WriteLine($"[PhysicsWorld] Added kinematic collider '{name}' at {position}");
    }

    /// <summary>
    /// Update a collider's position
    /// </summary>
    public void UpdateColliderPosition(string name, Vector3 position)
    {
        if (colliders.TryGetValue(name, out var collider))
        {
            collider.Position = position;
        }
    }

    /// <summary>
    /// Get collider by name
    /// </summary>
    public ColliderData? GetCollider(string name)
    {
        return colliders.TryGetValue(name, out var collider) ? collider : null;
    }

    /// <summary>
    /// Get all colliders
    /// </summary>
    public Dictionary<string, ColliderData> GetAllColliders() => colliders;

    /// <summary>
    /// Check if two colliders are colliding
    /// </summary>
    public bool IsColliding(string name1, string name2)
    {
        var collider1 = GetCollider(name1);
        var collider2 = GetCollider(name2);

        if (collider1 == null || collider2 == null)
            return false;

        return CheckCollision(collider1, collider2);
    }

    /// <summary>
    /// Check collision between two collider data objects
    /// </summary>
    private bool CheckCollision(ColliderData c1, ColliderData c2)
    {
        // Sphere to Sphere collision
        if (c1.Shape is SphereCollider s1 && c2.Shape is SphereCollider s2)
        {
            float r1 = s1.Radius * c1.Scale.X;
            float r2 = s2.Radius * c2.Scale.X;
            float distance = Vector3.Distance(c1.Position, c2.Position);
            return distance < (r1 + r2);
        }

        // Box to Sphere collision
        if (c1.Shape is BoxCollider b1 && c2.Shape is SphereCollider s3)
        {
            return CheckBoxSphereCollision(b1, c1, s3, c2);
        }

        if (c1.Shape is SphereCollider s4 && c2.Shape is BoxCollider b2)
        {
            return CheckBoxSphereCollision(b2, c2, s4, c1);
        }

        // Box to Box collision
        if (c1.Shape is BoxCollider b3 && c2.Shape is BoxCollider b4)
        {
            return CheckBoxBoxCollision(b3, c1, b4, c2);
        }

        return false;
    }

    /// <summary>
    /// Check collision between a box and sphere
    /// </summary>
    private bool CheckBoxSphereCollision(BoxCollider box, ColliderData boxData, SphereCollider sphere, ColliderData sphereData)
    {
        float bw = box.Width * boxData.Scale.X / 2;
        float bh = box.Height * boxData.Scale.Y / 2;
        float bd = box.Depth * boxData.Scale.Z / 2;

        Vector3 closest = sphereData.Position;
        closest.X = Math.Max(boxData.Position.X - bw, Math.Min(closest.X, boxData.Position.X + bw));
        closest.Y = Math.Max(boxData.Position.Y - bh, Math.Min(closest.Y, boxData.Position.Y + bh));
        closest.Z = Math.Max(boxData.Position.Z - bd, Math.Min(closest.Z, boxData.Position.Z + bd));

        float distance = Vector3.Distance(sphereData.Position, closest);
        float radius = sphere.Radius * sphereData.Scale.X;

        return distance < radius;
    }

    /// <summary>
    /// Check collision between two boxes (AABB)
    /// </summary>
    private bool CheckBoxBoxCollision(BoxCollider b1, ColliderData d1, BoxCollider b2, ColliderData d2)
    {
        float b1w = b1.Width * d1.Scale.X / 2;
        float b1h = b1.Height * d1.Scale.Y / 2;
        float b1d = b1.Depth * d1.Scale.Z / 2;

        float b2w = b2.Width * d2.Scale.X / 2;
        float b2h = b2.Height * d2.Scale.Y / 2;
        float b2d = b2.Depth * d2.Scale.Z / 2;

        return (d1.Position.X - b1w < d2.Position.X + b2w &&
                d1.Position.X + b1w > d2.Position.X - b2w &&
                d1.Position.Y - b1h < d2.Position.Y + b2h &&
                d1.Position.Y + b1h > d2.Position.Y - b2h &&
                d1.Position.Z - b1d < d2.Position.Z + b2d &&
                d1.Position.Z + b1d > d2.Position.Z - b2d);
    }

    /// <summary>
    /// Step the physics simulation (placeholder for future BepuPhysics integration)
    /// </summary>
    public void Update(float deltaTime)
    {
        // TODO: Implement actual physics simulation with BepuPhysics
        // For now, this is a placeholder for collision detection
    }

    public void Dispose()
    {
        colliders.Clear();
    }
}

/// <summary>
/// Base class for collider shapes
/// </summary>
public abstract class ColliderShape
{
    public abstract void Draw(Vector3 position, Vector3 scale, Raylib_cs.Color color);
}

/// <summary>
/// Box collider shape
/// </summary>
public class BoxCollider : ColliderShape
{
    public float Width { get; set; }
    public float Height { get; set; }
    public float Depth { get; set; }

    public BoxCollider(float width, float height, float depth)
    {
        Width = width;
        Height = height;
        Depth = depth;
    }

    public override void Draw(Vector3 position, Vector3 scale, Raylib_cs.Color color)
    {
        float w = Width * scale.X;
        float h = Height * scale.Y;
        float d = Depth * scale.Z;

        float halfW = w / 2;
        float halfH = h / 2;
        float halfD = d / 2;

        Vector3[] corners = new[]
        {
            position + new Vector3(-halfW, -halfH, -halfD),
            position + new Vector3(halfW, -halfH, -halfD),
            position + new Vector3(halfW, halfH, -halfD),
            position + new Vector3(-halfW, halfH, -halfD),
            position + new Vector3(-halfW, -halfH, halfD),
            position + new Vector3(halfW, -halfH, halfD),
            position + new Vector3(halfW, halfH, halfD),
            position + new Vector3(-halfW, halfH, halfD),
        };

        // Bottom face
        Raylib_cs.Raylib.DrawLine3D(corners[0], corners[1], color);
        Raylib_cs.Raylib.DrawLine3D(corners[1], corners[2], color);
        Raylib_cs.Raylib.DrawLine3D(corners[2], corners[3], color);
        Raylib_cs.Raylib.DrawLine3D(corners[3], corners[0], color);

        // Top face
        Raylib_cs.Raylib.DrawLine3D(corners[4], corners[5], color);
        Raylib_cs.Raylib.DrawLine3D(corners[5], corners[6], color);
        Raylib_cs.Raylib.DrawLine3D(corners[6], corners[7], color);
        Raylib_cs.Raylib.DrawLine3D(corners[7], corners[4], color);

        // Vertical edges
        Raylib_cs.Raylib.DrawLine3D(corners[0], corners[4], color);
        Raylib_cs.Raylib.DrawLine3D(corners[1], corners[5], color);
        Raylib_cs.Raylib.DrawLine3D(corners[2], corners[6], color);
        Raylib_cs.Raylib.DrawLine3D(corners[3], corners[7], color);
    }
}

/// <summary>
/// Sphere collider shape
/// </summary>
public class SphereCollider : ColliderShape
{
    public float Radius { get; set; }

    public SphereCollider(float radius)
    {
        Radius = radius;
    }

    public override void Draw(Vector3 position, Vector3 scale, Raylib_cs.Color color)
    {
        float radius = Radius * scale.X;

        // Draw wireframe sphere using latitude/longitude lines
        const int latitudeSegments = 16;  // Horizontal rings
        const int longitudeSegments = 32; // Vertical segments

        for (int lat = 0; lat < latitudeSegments; lat++)
        {
            float lat1 = (float)(Math.PI * lat / latitudeSegments);
            float lat2 = (float)(Math.PI * (lat + 1) / latitudeSegments);

            for (int lon = 0; lon < longitudeSegments; lon++)
            {
                float lon1 = (float)(2 * Math.PI * lon / longitudeSegments);
                float lon2 = (float)(2 * Math.PI * (lon + 1) / longitudeSegments);

                // Calculate 4 points of the quad
                Vector3 p1 = position + new Vector3(
                    radius * (float)(Math.Sin(lat1) * Math.Cos(lon1)),
                    radius * (float)Math.Cos(lat1),
                    radius * (float)(Math.Sin(lat1) * Math.Sin(lon1))
                );

                Vector3 p2 = position + new Vector3(
                    radius * (float)(Math.Sin(lat1) * Math.Cos(lon2)),
                    radius * (float)Math.Cos(lat1),
                    radius * (float)(Math.Sin(lat1) * Math.Sin(lon2))
                );

                Vector3 p3 = position + new Vector3(
                    radius * (float)(Math.Sin(lat2) * Math.Cos(lon1)),
                    radius * (float)Math.Cos(lat2),
                    radius * (float)(Math.Sin(lat2) * Math.Sin(lon1))
                );

                Vector3 p4 = position + new Vector3(
                    radius * (float)(Math.Sin(lat2) * Math.Cos(lon2)),
                    radius * (float)Math.Cos(lat2),
                    radius * (float)(Math.Sin(lat2) * Math.Sin(lon2))
                );

                // Draw edges of the quad
                Raylib_cs.Raylib.DrawLine3D(p1, p2, color);
                Raylib_cs.Raylib.DrawLine3D(p1, p3, color);
            }
        }
    }
}

/// <summary>
/// Capsule collider shape
/// </summary>
public class CapsuleCollider : ColliderShape
{
    public float HalfLength { get; set; }
    public float Radius { get; set; }

    public CapsuleCollider(float halfLength, float radius)
    {
        HalfLength = halfLength;
        Radius = radius;
    }

    public override void Draw(Vector3 position, Vector3 scale, Raylib_cs.Color color)
    {
        float halfLen = HalfLength * scale.Y;
        float radius = Radius * scale.X;

        Vector3 topCenter = position + Vector3.UnitY * halfLen;
        Vector3 bottomCenter = position - Vector3.UnitY * halfLen;

        Raylib_cs.Raylib.DrawSphere(topCenter, radius, color);
        Raylib_cs.Raylib.DrawSphere(bottomCenter, radius, color);

        const int segments = 8;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)(i * 2 * Math.PI / segments);
            float angle2 = (float)((i + 1) * 2 * Math.PI / segments);

            Vector3 p1 = new Vector3((float)Math.Cos(angle1) * radius, halfLen, (float)Math.Sin(angle1) * radius) + position;
            Vector3 p2 = new Vector3((float)Math.Cos(angle2) * radius, halfLen, (float)Math.Sin(angle2) * radius) + position;
            Vector3 p3 = new Vector3((float)Math.Cos(angle1) * radius, -halfLen, (float)Math.Sin(angle1) * radius) + position;

            Raylib_cs.Raylib.DrawLine3D(p1, p2, color);
            Raylib_cs.Raylib.DrawLine3D(p1, p3, color);
        }
    }
}
