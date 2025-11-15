using System;
using System.Numerics;
using System.Collections.Generic;

namespace PhantomNebula.Core;

/// <summary>
/// Base entity class with transform
/// All game objects inherit from this for consistent positioning and orientation
/// </summary>
public class Entity
{
    public string Name { get; set; } = "Entity";
    public ITransform Transform { get; private set; }
    public bool Active { get; set; } = true;

    private List<Entity> children = new();
    private Entity? parent;

    public Entity(string name = "Entity")
    {
        Name = name;
        Transform = new Transform();
    }

    public Entity(Vector3 position, string name = "Entity")
    {
        Name = name;
        Transform = new Transform { Position = position };
    }

    public Entity(Vector3 position, Vector3 scale, string name = "Entity")
    {
        Name = name;
        Transform = new Transform { Position = position, Scale = scale };
    }

    /// <summary>
    /// Get or set parent entity
    /// </summary>
    public Entity? Parent
    {
        get => parent;
        set
        {
            if (parent != null)
            {
                parent.children.Remove(this);
            }

            parent = value;
            if (parent != null)
            {
                parent.children.Add(this);
                Transform.Parent = parent.Transform;
            }
            else
            {
                Transform.Parent = null;
            }
        }
    }

    /// <summary>
    /// Get all children
    /// </summary>
    public IReadOnlyList<Entity> Children => children.AsReadOnly();

    /// <summary>
    /// Add a child entity
    /// </summary>
    public void AddChild(Entity child)
    {
        child.Parent = this;
    }

    /// <summary>
    /// Remove a child entity
    /// </summary>
    public void RemoveChild(Entity child)
    {
        if (children.Remove(child))
        {
            child.Parent = null;
        }
    }

    /// <summary>
    /// Update entity (override in derived classes)
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        // Update children
        foreach (var child in children)
        {
            if (child.Active)
            {
                child.Update(deltaTime);
            }
        }
    }

    /// <summary>
    /// Draw entity (override in derived classes)
    /// </summary>
    public virtual void Draw()
    {
        // Draw children
        foreach (var child in children)
        {
            if (child.Active)
            {
                child.Draw();
            }
        }
    }

    /// <summary>
    /// Dispose entity
    /// </summary>
    public virtual void Dispose()
    {
        foreach (var child in children)
        {
            child.Dispose();
        }
        children.Clear();
    }
}
