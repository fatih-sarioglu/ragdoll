using UnityEngine;

/// <summary>
/// The essential helper for active ragdolls. ConfigurableJoint.targetRotation is
/// expressed in the joint's own frame (defined by axis / secondaryAxis), so you can't
/// just assign a bone's localRotation to it. This converts a desired local rotation
/// into joint space, relative to the joint's starting local rotation.
/// </summary>
public static class ConfigurableJointExtensions
{
    /// <param name="joint">The joint to drive.</param>
    /// <param name="targetLocalRotation">Desired localRotation of the joint's transform (e.g. the matching bone on the animated twin).</param>
    /// <param name="startLocalRotation">The joint transform's localRotation cached at startup, BEFORE physics has moved anything.</param>
    public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
    {
        if (joint.configuredInWorldSpace)
        {
            Debug.LogError("SetTargetRotationLocal must not be used on a joint configured in world space.", joint);
            return;
        }

        // Build the joint-space frame from the joint axes.
        Vector3 right = joint.axis;
        Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        Vector3 up = Vector3.Cross(forward, right).normalized;
        Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

        // Transform the delta (start -> target) into joint space.
        Quaternion result = Quaternion.Inverse(worldToJointSpace)
                          * Quaternion.Inverse(targetLocalRotation)
                          * startLocalRotation
                          * worldToJointSpace;

        joint.targetRotation = result;
    }
}