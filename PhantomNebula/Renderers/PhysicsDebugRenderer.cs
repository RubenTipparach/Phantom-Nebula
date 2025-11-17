using System;
using System.Numerics;
using PhantomNebula.Physics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace PhantomNebula.Renderers;

/// <summary>
/// Renders debug colliders from the physics world
/// Shows all registered colliders as wireframes in different colors
/// </summary>
public class PhysicsDebugRenderer
{
    private PhysicsWorld physicsWorld;

    public PhysicsDebugRenderer(PhantomNebula.Physics.PhysicsWorld physicsWorld)
    {
        this.physicsWorld = physicsWorld;
    }

    /// <summary>
    /// Draw all colliders with custom colors
    /// </summary>
    public void Draw()
    {
        var allColliders = physicsWorld.GetAllColliders();

        foreach (var (name, collider) in allColliders)
        {
            // Choose color based on collider type/name
            Color color = collider.Type switch
            {
                PhysicsWorld.ColliderType.Static => Color.Blue,
                PhysicsWorld.ColliderType.Kinematic => Color.Green,
                PhysicsWorld.ColliderType.Dynamic => Color.Red,
                _ => Color.White
            };

            // Draw the collider shape
            collider.Shape.Draw(collider.Position, collider.Scale, color);
        }
    }
}
