using System;
using System.Numerics;

namespace PhantomNebula.Core;

/// <summary>
/// Transform interface for 3D positioning, rotation, and scale
/// Provides orientation vectors (Forward, Right, Up) and local/world positioning
/// </summary>
public interface ITransform
{
    // Position
    Vector3 Position { get; set; }
    Vector3 LocalPosition { get; set; }

    // Rotation (quaternion is ground truth)
    Quaternion Rotation { get; set; }

    // Scale
    Vector3 Scale { get; set; }

    // Orientation vectors (derived from rotation)
    Vector3 Forward { get; }
    Vector3 Right { get; }
    Vector3 Up { get; }

    // Parent transform
    ITransform? Parent { get; set; }

    // Derived properties
    Vector3 WorldPosition { get; }
    Matrix4x4 WorldMatrix { get; }
    Matrix4x4 LocalMatrix { get; }
}

/// <summary>
/// Abstract transform base class implementing ITransform
/// </summary>
public abstract class Transform : ITransform
{
    private Vector3 position = Vector3.Zero;
    private Vector3 localPosition = Vector3.Zero;
    private Quaternion rotation = Quaternion.Identity; // Quaternion is ground truth
    private Vector3 scale = Vector3.One;
    private ITransform? parent;

    public Vector3 Position
    {
        get => position;
        set => position = value;
    }

    public Vector3 LocalPosition
    {
        get => localPosition;
        set => localPosition = value;
    }

    public Quaternion Rotation
    {
        get => rotation;
        set => rotation = value;
    }


    public Quaternion QuatRotation
    {
        get => rotation;
        set => rotation = value;
    }

    public Vector3 Scale
    {
        get => scale;
        set => scale = value;
    }

    public ITransform? Parent
    {
        get => parent;
        set => parent = value;
    }

    public Vector3 WorldPosition
    {
        get
        {
            if (parent != null)
            {
                var parentMatrix = parent.WorldMatrix;
                return Vector3.Transform(localPosition, parentMatrix);
            }
            return position;
        }
    }

    public Vector3 Forward
    {
        get
        {
            var quat = QuatRotation;
            return Vector3.Transform(Vector3.UnitZ, quat);
        }
    }

    public Vector3 Right
    {
        get
        {
            var quat = QuatRotation;
            return Vector3.Transform(Vector3.UnitX, quat);
        }
    }

    public Vector3 Up
    {
        get
        {
            var quat = QuatRotation;
            return Vector3.Transform(Vector3.UnitY, quat);
        }
    }

    public Matrix4x4 LocalMatrix
    {
        get
        {
            var translation = Matrix4x4.CreateTranslation(localPosition);
            var rotationMatrix = Matrix4x4.CreateFromQuaternion(QuatRotation);
            var scaleMatrix = Matrix4x4.CreateScale(scale);

            return scaleMatrix * rotationMatrix * translation;
        }
    }

    public Matrix4x4 WorldMatrix
    {
        get
        {
            var localMatrix = LocalMatrix;
            if (parent != null)
            {
                return localMatrix * parent.WorldMatrix;
            }

            var translation = Matrix4x4.CreateTranslation(position);
            var rotationMatrix = Matrix4x4.CreateFromQuaternion(QuatRotation);
            var scaleMatrix = Matrix4x4.CreateScale(scale);

            return scaleMatrix * rotationMatrix * translation;
        }
    }

    /// <summary>
    /// Convert Euler angles (pitch, yaw, roll) to Quaternion
    /// Uses Raylib's QuaternionFromEuler
    /// </summary>
    private Quaternion QuaternionFromEuler(Vector3 euler)
    {
        return Raylib_cs.Raymath.QuaternionFromEuler(euler.X, euler.Y, euler.Z);
    }

    /// <summary>
    /// Rotate the transform to look at a target position
    /// </summary>
    public void LookAt(Vector3 targetPosition, Vector3? upVector = null)
    {
        upVector ??= Vector3.UnitY;

        var direction = Vector3.Normalize(targetPosition - position);
        var right = Vector3.Normalize(Vector3.Cross(upVector.Value, direction));
        var up = Vector3.Cross(direction, right);

        var lookMatrix = Matrix4x4.Identity;
        lookMatrix.M11 = right.X;
        lookMatrix.M12 = right.Y;
        lookMatrix.M13 = right.Z;

        lookMatrix.M21 = up.X;
        lookMatrix.M22 = up.Y;
        lookMatrix.M23 = up.Z;

        lookMatrix.M31 = direction.X;
        lookMatrix.M32 = direction.Y;
        lookMatrix.M33 = direction.Z;

        var quat = QuaternionFromMatrix(lookMatrix);
        rotation = quat;
    }

    /// <summary>
    /// Extract quaternion from matrix
    /// </summary>
    private Quaternion QuaternionFromMatrix(Matrix4x4 matrix)
    {
        return Quaternion.CreateFromRotationMatrix(matrix);
    }

    /// <summary>
    /// Convert Quaternion to Euler angles
    /// Uses Raylib's QuaternionToEuler
    /// </summary>
    private Vector3 EulerFromQuaternion(Quaternion q)
    {
        return Raylib_cs.Raymath.QuaternionToEuler(q);
    }
}
