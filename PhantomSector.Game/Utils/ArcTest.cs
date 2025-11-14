using Microsoft.Xna.Framework;
using System;

namespace PhantomSector.Game.Utils;

public static class ArcTest
{
    // Helper method to create rotation quaternion from euler angles (Unity compatibility)
    private static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
    {
        return Quaternion.CreateFromYawPitchRoll(
            MathHelper.ToRadians(yaw),
            MathHelper.ToRadians(pitch),
            MathHelper.ToRadians(roll));
    }

    public static bool TargetArcTest(Vector3 shipPosition, Vector3 shipForward, Vector3 targetPosition, float startDegree, float stopDegree)
    {
        Vector3 leftArcNormalized = Vector3.Transform(shipForward, CreateFromYawPitchRoll(startDegree, 0, 0));
        Vector3 RightArcNormalized = Vector3.Transform(shipForward, CreateFromYawPitchRoll(stopDegree, 0, 0));
        Vector3 targetNormalized = Vector3.Normalize(targetPosition - shipPosition);

        var offset = (stopDegree - startDegree);

        if (offset == 360f) return true;

        var testResult = TargetArcTest(leftArcNormalized, RightArcNormalized, targetNormalized, offset, Vector3.Zero);

        return testResult;
    }

    public static bool TargetArcTest(Vector3 myPosition, Vector3 myForward,
        Vector3 targetPosition, float startDegree, float stopDegree, bool debug = false)
    {
        Vector3 leftArcNormalized = Vector3.Transform(myForward, CreateFromYawPitchRoll(startDegree, 0, 0));
        Vector3 RightArcNormalized = Vector3.Transform(myForward, CreateFromYawPitchRoll(stopDegree, 0, 0));
        Vector3 targetNormalized = Vector3.Normalize(targetPosition - myPosition);

        // Debug drawing would need to be implemented with your game's rendering system
        // if (debug) { /* Draw debug lines */ }

        var offset = (stopDegree - startDegree);

        if (offset == 360f) return true;

        var testResult = TargetArcTest(leftArcNormalized, RightArcNormalized, targetNormalized, offset, Vector3.Zero);

        return testResult;
    }

    public static bool TargetArcTest(Vector3 leftVector, Vector3 rightVector, Vector3 targetVector, float actualAngle, Vector3 offset)
    {
        float angle = actualAngle;
        float halfAngle = angle / 2f;

        Vector3 midWayVector = Vector3.Transform(leftVector, CreateFromYawPitchRoll(-halfAngle, 0, 0));
        float minRangeUnit = Vector3.Dot(midWayVector, rightVector);
        float targetRangeUnit = Vector3.Dot(midWayVector, targetVector);

        return (targetRangeUnit >= minRangeUnit);
    }


    public static bool TargetArcTestVertical(Vector3 leftVector, Vector3 rightVector, Vector3 targetVector, float actualAngle, Vector3 offset)
    {
        float angle = actualAngle;
        float halfAngle = angle / 2f;

        Vector3 midWayVector = Vector3.Transform(rightVector, CreateFromYawPitchRoll(0, halfAngle, 0));
        float minRangeUnit = Vector3.Dot(midWayVector, rightVector);
        float targetRangeUnit = Vector3.Dot(midWayVector, targetVector);

        return (targetRangeUnit >= minRangeUnit);
    }

    // Removed Transform-based overload - use the Vector3/Quaternion version directly

    // this is a pure utility function.
    public static bool TargetArcTest3D(
        Vector3 turretPosition,
        Quaternion turretRotation,
        Vector3 targetPosition,

        float horizontalStartDegree,
        float horizontalStopDegree,

        float verticalStartDegree,
        float verticalStopDegree,

        bool debug = false)
    {
        var myPosition = turretPosition;
        var orientation = turretRotation;

        (var upVector, var rightVector) = orientation.GetUpAndRightVectors();

        //horizontal
        Vector3 leftArcNormalized = Vector3.Transform(Vector3.Forward, CreateFromYawPitchRoll(horizontalStartDegree, 0, 0));
        Vector3 RightArcNormalized = Vector3.Transform(Vector3.Forward, CreateFromYawPitchRoll(horizontalStopDegree, 0, 0));

        // vertical
        Vector3 bottomArcNormalized = Vector3.Transform(Vector3.Forward, CreateFromYawPitchRoll(0, verticalStartDegree, 0));
        Vector3 topArcNormalized = Vector3.Transform(Vector3.Forward, CreateFromYawPitchRoll(0, verticalStopDegree, 0));

        Vector3 targetNormalized = Vector3.Normalize(targetPosition - myPosition);

        Vector3 flattenedXZTarget = Vector3.Normalize(Vector3.Transform(ProjectOnPlane(targetNormalized, upVector), Quaternion.Inverse(orientation))); // horizontal
        Vector3 flattenedYZTarget = Vector3.Normalize(Vector3.Transform(ProjectOnPlane(targetNormalized, rightVector), Quaternion.Inverse(orientation))); // vertical

        var hOffset = (horizontalStartDegree - horizontalStopDegree);
        var yOffset = (verticalStartDegree - verticalStopDegree);

        bool testResultXZ = hOffset >= 360f || TargetArcTest(leftArcNormalized, RightArcNormalized, flattenedXZTarget, hOffset, Vector3.Zero); // horizontal
        bool testResultYZ = yOffset >= 360f || TargetArcTestVertical(bottomArcNormalized, topArcNormalized, flattenedYZTarget, yOffset, Vector3.Left * 4);// vertical

        // Debug drawing would need to be implemented with your game's rendering system
        // if (debug) { /* Draw debug lines */ }

        return testResultXZ && testResultYZ;
    }

    // Helper method to project a vector onto a plane (Unity compatibility)
    private static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
    {
        float distance = Vector3.Dot(vector, planeNormal);
        return vector - planeNormal * distance;
    }
}


public static class QuaternionExtensions
{
    // Extension method for Quaternion
    public static (Vector3 up, Vector3 right) GetUpAndRightVectors(this Quaternion quaternion)
    {
        Vector3 upVector = Vector3.Transform(Vector3.Up, quaternion);
        Vector3 rightVector = Vector3.Transform(Vector3.Right, quaternion);

        return (upVector, rightVector);
    }
}