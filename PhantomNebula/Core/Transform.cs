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

    // Rotation (in radians)
    Vector3 Rotation { get; set; }
    Quaternion QuatRotation { get; }

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
/// Default implementation of ITransform
/// </summary>
public class Transform : ITransform
{
    private Vector3 position = Vector3.Zero;
    private Vector3 localPosition = Vector3.Zero;
    private Vector3 rotation = Vector3.Zero; // Euler angles in radians (pitch, yaw, roll)
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

    public Vector3 Rotation
    {
        get => rotation;
        set => rotation = value;
    }

    public Quaternion QuatRotation
    {
        get => QuaternionFromEuler(rotation);
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
    /// Order: Z (roll), X (pitch), Y (yaw)
    /// </summary>
    private Quaternion QuaternionFromEuler(Vector3 euler)
    {
        float cy = (float)Math.Cos(euler.Y * 0.5f);
        float sy = (float)Math.Sin(euler.Y * 0.5f);
        float cp = (float)Math.Cos(euler.X * 0.5f);
        float sp = (float)Math.Sin(euler.X * 0.5f);
        float cr = (float)Math.Cos(euler.Z * 0.5f);
        float sr = (float)Math.Sin(euler.Z * 0.5f);

        return new Quaternion(
            sp * cy * cr - cp * sy * sr,
            cp * sy * cr + sp * cy * sr,
            cp * cy * sr - sp * sy * cr,
            cp * cy * cr + sp * sy * sr
        );
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
        rotation = EulerFromQuaternion(quat);
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
    /// </summary>
    private Vector3 EulerFromQuaternion(Quaternion q)
    {
        var sqx = q.X * q.X;
        var sqy = q.Y * q.Y;
        var sqz = q.Z * q.Z;
        var sqw = q.W * q.W;

        var pitch = (float)Math.Atan2(2f * (q.W * q.X + q.Y * q.Z), 1f - 2f * (sqx + sqy));
        var yaw = (float)Math.Asin(2f * (q.W * q.Y - q.Z * q.X));
        var roll = (float)Math.Atan2(2f * (q.W * q.Z + q.X * q.Y), 1f - 2f * (sqy + sqz));

        return new Vector3(pitch, yaw, roll);
    }
}
