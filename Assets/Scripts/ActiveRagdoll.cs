using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives a ConfigurableJoint ragdoll to follow an animated twin.
///
/// Setup:
///  - animatedRoot  = the HIPS/PELVIS bone of the invisible copy that has the Animator.
///  - physicalRoot  = the HIPS/PELVIS bone of the ragdoll (ConfigurableJoints, no Animator).
/// Bones are matched by name, so both rigs must come from the same FBX.
///
/// Every FixedUpdate, each joint's targetRotation is set to the matching animated
/// bone's local rotation. The Slerp drive spring/damper decide how hard it tries.
/// </summary>
public class ActiveRagdoll : MonoBehaviour
{
    [Header("Rigs (assign the hip/pelvis bone of each)")]
    public Transform animatedRoot;
    public Transform physicalRoot;

    [Header("Joint drive (applied to all joints on Start)")]
    [Tooltip("How hard joints pull toward the animated pose. ~100 = drunk, ~800 = decent tracking, ~5000+ = stiff.")]
    public float positionSpring = 800f;
    [Tooltip("Damping. Roughly spring/20 is a good start; too low = jitter/oscillation.")]
    public float positionDamper = 40f;
    public float maximumForce = Mathf.Infinity;

    [Header("Root matching (keeps the pelvis upright / in place)")]
    public bool matchRootPosition = true;
    public bool matchRootRotation = true;
    public float rootPositionSpring = 2000f;
    public float rootPositionDamper = 100f;
    public float rootRotationSpring = 300f;
    public float rootRotationDamper = 20f;

    [Header("Physics quality")]
    public int solverIterations = 10;
    public int solverVelocityIterations = 10;

    struct DrivenJoint
    {
        public ConfigurableJoint joint;
        public Transform target;              // matching bone on the animated twin
        public Quaternion startLocalRotation; // cached before physics runs
    }

    readonly List<DrivenJoint> _joints = new List<DrivenJoint>();
    Rigidbody _rootBody;

    void Awake()
    {
        // Index the animated twin's bones by name.
        var animatedBones = new Dictionary<string, Transform>();
        foreach (Transform t in animatedRoot.GetComponentsInChildren<Transform>(true))
            animatedBones[t.name] = t;
        animatedBones[animatedRoot.name] = animatedRoot;

        // Hook up every joint on the ragdoll to its animated counterpart.
        foreach (ConfigurableJoint joint in physicalRoot.GetComponentsInChildren<ConfigurableJoint>(true))
        {
            if (!animatedBones.TryGetValue(joint.name, out Transform target))
            {
                Debug.LogWarning($"No animated bone found matching '{joint.name}' — joint will not be driven.", joint);
                continue;
            }

            _joints.Add(new DrivenJoint
            {
                joint = joint,
                target = target,
                startLocalRotation = joint.transform.localRotation
            });
        }

        _rootBody = physicalRoot.GetComponent<Rigidbody>();

        foreach (Rigidbody rb in physicalRoot.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.solverIterations = solverIterations;
            rb.solverVelocityIterations = solverVelocityIterations;
        }
    }

    void Start()
    {
        ApplyDriveSettings();
    }

    /// <summary>Push the inspector spring/damper values into every joint. Call again after tuning at runtime.</summary>
    public void ApplyDriveSettings()
    {
        foreach (DrivenJoint d in _joints)
        {
            JointDrive drive = d.joint.slerpDrive;
            drive.positionSpring = positionSpring;
            drive.positionDamper = positionDamper;
            drive.maximumForce = maximumForce;
            d.joint.slerpDrive = drive;
            d.joint.rotationDriveMode = RotationDriveMode.Slerp;
        }
    }

    void FixedUpdate()
    {
        // Make every joint chase the animated pose.
        foreach (DrivenJoint d in _joints)
            d.joint.SetTargetRotationLocal(d.target.localRotation, d.startLocalRotation);

        if (_rootBody == null)
            return;

#if UNITY_6000_0_OR_NEWER
        Vector3 rootVelocity = _rootBody.linearVelocity;
#else
        Vector3 rootVelocity = _rootBody.velocity;
#endif

        // PD controller keeping the pelvis near the animated pelvis. Without this the
        // ragdoll matches the POSE but nothing holds it up in the world.
        if (matchRootPosition)
        {
            Vector3 force = (animatedRoot.position - _rootBody.position) * rootPositionSpring
                          - rootVelocity * rootPositionDamper;
            _rootBody.AddForce(force);
        }

        if (matchRootRotation)
        {
            Quaternion delta = animatedRoot.rotation * Quaternion.Inverse(_rootBody.rotation);
            delta.ToAngleAxis(out float angleDeg, out Vector3 axis);
            if (angleDeg > 180f) angleDeg -= 360f;

            if (!float.IsNaN(axis.x) && !float.IsInfinity(axis.x) && Mathf.Abs(angleDeg) > 0.01f)
            {
                Vector3 torque = axis.normalized * (angleDeg * Mathf.Deg2Rad * rootRotationSpring)
                               - _rootBody.angularVelocity * rootRotationDamper;
                _rootBody.AddTorque(torque);
            }
        }
    }

    /// <summary>Go limp (e.g. on death / big hit). Pass true to fall, false to recover.</summary>
    public void SetLimp(bool limp)
    {
        float spring = limp ? 0f : positionSpring;
        float damper = limp ? 5f : positionDamper;

        foreach (DrivenJoint d in _joints)
        {
            JointDrive drive = d.joint.slerpDrive;
            drive.positionSpring = spring;
            drive.positionDamper = damper;
            d.joint.slerpDrive = drive;
        }

        bool wasMatching = !limp;
        matchRootPosition = wasMatching && matchRootPosition;
        matchRootRotation = wasMatching && matchRootRotation;
    }
}