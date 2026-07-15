using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utilities for active ragdoll setup. Place this file in an "Editor" folder.
///
/// 1) "Convert CharacterJoints To ConfigurableJoints" — select your ragdoll root
///    (the one the Ragdoll Wizard produced) and run this. It swaps every
///    CharacterJoint for a ConfigurableJoint, carrying over anchors, axes and
///    the twist/swing limits, and sets up a Slerp drive ready to be driven.
///
/// 2) "Strip Physics From Selected" — run this on the ANIMATED TWIN copy to
///    remove all joints, rigidbodies and colliders so only the Animator drives it.
/// </summary>
public static class ActiveRagdollEditorTools
{
    [MenuItem("Tools/Active Ragdoll/Convert CharacterJoints To ConfigurableJoints")]
    static void ConvertSelected()
    {
        GameObject root = Selection.activeGameObject;
        if (root == null)
        {
            Debug.LogWarning("Select the ragdoll root first.");
            return;
        }

        CharacterJoint[] joints = root.GetComponentsInChildren<CharacterJoint>(true);
        if (joints.Length == 0)
        {
            Debug.LogWarning("No CharacterJoints found under " + root.name);
            return;
        }

        foreach (CharacterJoint cj in joints)
            Convert(cj);

        Debug.Log($"Converted {joints.Length} CharacterJoints to ConfigurableJoints under {root.name}.");
    }

    static void Convert(CharacterJoint cj)
    {
        GameObject go = cj.gameObject;

        // Capture everything we need before destroying the CharacterJoint.
        Rigidbody connected      = cj.connectedBody;
        Vector3 anchor           = cj.anchor;
        Vector3 axis             = cj.axis;
        Vector3 swingAxis        = cj.swingAxis;
        bool autoConnectedAnchor = cj.autoConfigureConnectedAnchor;
        Vector3 connectedAnchor  = cj.connectedAnchor;
        SoftJointLimit lowTwist  = cj.lowTwistLimit;
        SoftJointLimit highTwist = cj.highTwistLimit;
        SoftJointLimit swing1    = cj.swing1Limit;
        SoftJointLimit swing2    = cj.swing2Limit;
        bool enableCollision     = cj.enableCollision;
        bool enableProjection    = cj.enableProjection;

        Undo.DestroyObjectImmediate(cj);

        ConfigurableJoint joint = Undo.AddComponent<ConfigurableJoint>(go);

        joint.connectedBody = connected;
        joint.anchor = anchor;
        joint.axis = axis;
        joint.secondaryAxis = swingAxis;
        joint.autoConfigureConnectedAnchor = autoConnectedAnchor;
        if (!autoConnectedAnchor)
            joint.connectedAnchor = connectedAnchor;

        // Bones shouldn't translate relative to their parent, only rotate.
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        // CharacterJoint twist/swing maps directly onto ConfigurableJoint angular limits.
        joint.lowAngularXLimit  = lowTwist;
        joint.highAngularXLimit = highTwist;
        joint.angularYLimit     = swing1;
        joint.angularZLimit     = swing2;

        joint.enableCollision = enableCollision;
        joint.projectionMode = enableProjection
            ? JointProjectionMode.PositionAndRotation
            : JointProjectionMode.None;

        // Slerp drive = one spring/damper driving the whole rotation toward targetRotation.
        // These defaults are a sane starting point; the ActiveRagdoll driver can override them.
        joint.rotationDriveMode = RotationDriveMode.Slerp;
        JointDrive drive = joint.slerpDrive;
        drive.positionSpring = 800f;
        drive.positionDamper = 40f;
        drive.maximumForce = Mathf.Infinity;
        joint.slerpDrive = drive;
    }

    [MenuItem("Tools/Active Ragdoll/Strip Physics From Selected (for animated twin)")]
    static void StripPhysics()
    {
        GameObject root = Selection.activeGameObject;
        if (root == null)
        {
            Debug.LogWarning("Select the animated twin root first.");
            return;
        }

        // Order matters: joints depend on rigidbodies, so remove joints first.
        foreach (Joint j in root.GetComponentsInChildren<Joint>(true))
            Undo.DestroyObjectImmediate(j);
        foreach (Rigidbody rb in root.GetComponentsInChildren<Rigidbody>(true))
            Undo.DestroyObjectImmediate(rb);
        foreach (Collider c in root.GetComponentsInChildren<Collider>(true))
            Undo.DestroyObjectImmediate(c);

        Debug.Log("Stripped joints, rigidbodies and colliders from " + root.name);
    }
}