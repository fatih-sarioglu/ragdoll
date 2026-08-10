using UnityEngine;


public enum ArmState { Free, PunchWindup, PunchHold, PunchOut, PunchRetract }

[System.Serializable]
public class ArmSide
{
    public Transform upper, lower;
    public ConfigurableJoint upperJoint, lowerJoint;
    public float shoulderSign = 1f, elbowSign = 1f;

    [System.NonSerialized] public Quaternion initUpper, initLower;
    [System.NonSerialized] public float reach;
    [System.NonSerialized] public ArmState state;
    [System.NonSerialized] public bool isLeft;
    [System.NonSerialized] public float windupStartReach;
}

public class ProceduralArms : MonoBehaviour
{
    [Header("Arms")]
    [SerializeField] ArmSide armL = new ArmSide();
    [SerializeField] ArmSide armR = new ArmSide();

    [Header("Reach pose")]
    [SerializeField] float shoulderAngle = 90f;
    [SerializeField] float elbowAngle = 20f;
    [SerializeField] float blendSpeed = 12f;

    [Header("Reach spring/damper")]
    [SerializeField] float limpSpringUpper = 10f;
    [SerializeField] float limpDamperUpper = 10f;
    [SerializeField] float limpSpringLower = 10f;
    [SerializeField] float limpDamperLower = 10f;
    [SerializeField] float reachSpringUpper = 50f;
    [SerializeField] float reachDamperUpper = 50f;
    [SerializeField] float reachSpringLower = 50f;
    [SerializeField] float reachDamperLower = 50f;
    
    [Header("Punch")]
    [SerializeField] float punchOutSpeed = 10f;
    [SerializeField] float punchBackSpeed = 3f;
    [SerializeField] float punchSpringUpper = 300f;
    [SerializeField] float punchSpringLower = 300f;
    [SerializeField] float punchElbowAngle = 0f;
    [SerializeField] float windupSpringUpper = 150f;
    [SerializeField] float windupSpringLower = 150f;
    [SerializeField] float windupAmount = 0.4f;
    [SerializeField] float windupSpeed = 6f;

    [Header("Windup pose (mirrored automatically per side)")]
    [SerializeField] float windupPullbackAngle = 60f;
    [SerializeField] float windupSideAngle = 35f;
    [SerializeField] float windupElbowBend = 135f;
    [SerializeField] float windupTwist = 0f;

    ArmSide Side(bool left) => left ? armL : armR;

    void Awake()
    {
        InitSide(armL, true);
        InitSide(armR, false);
    }

    void InitSide(ArmSide s, bool left)
    {
        s.initUpper = s.upper.localRotation;
        s.initLower = s.lower.localRotation;
        s.state = ArmState.Free;
        s.isLeft = left;
    }

    public float Reach(bool left) => Side(left).reach;

    public void TickArm(bool left, float reachTarget, float dt)
    {
        ArmSide s = Side(left);
        if (s.state != ArmState.Free) { TickPunch(s, dt); return; }

        s.reach = Mathf.MoveTowards(s.reach, reachTarget, blendSpeed * dt);
        ApplyArm(s, elbowAngle);
        ApplyArmSprings(s, s.reach);
    }

    void ApplyArm(ArmSide s, float elbowAngleOverride)
    {
        s.upper.localRotation = s.initUpper * Quaternion.Euler(s.reach * shoulderAngle * s.shoulderSign, 0, 0);
        s.lower.localRotation = s.initLower * Quaternion.Euler(s.reach * elbowAngleOverride * s.elbowSign, 0, 0);
    }

    void ApplyArmSprings(ArmSide s, float reach)
    {
        float springU = Mathf.Lerp(limpSpringUpper, reachSpringUpper, reach);
        float damperU = Mathf.Lerp(limpDamperUpper, reachDamperUpper, reach);
        float springL = Mathf.Lerp(limpSpringLower, reachSpringLower, reach);
        float damperL = Mathf.Lerp(limpDamperLower, reachDamperLower, reach);

        SetArmSpring(s.upperJoint, springU, damperU);
        SetArmSpring(s.lowerJoint, springL, damperL);
    }

    private void SetArmSpring(ConfigurableJoint joint, float spring, float damper)
    {
        JointDrive d = joint.slerpDrive;
        d.positionSpring = spring;
        d.positionDamper = damper;
        joint.slerpDrive = d;
    }

    void TickPunch(ArmSide s, float dt)
    {
        if (s.state == ArmState.PunchWindup)
        {
            s.reach = Mathf.MoveTowards(s.reach, -windupAmount, windupSpeed * dt);
            if (s.reach <= -windupAmount + 0.01f) s.state = ArmState.PunchHold;

            SetArmSpring(s.upperJoint, windupSpringUpper, windupSpringUpper * 0.08f);
            SetArmSpring(s.lowerJoint, windupSpringLower, windupSpringLower * 0.08f);
            ApplyWindupPose(s, Mathf.InverseLerp(s.windupStartReach, -windupAmount, s.reach));
            return;
        }
        else if (s.state == ArmState.PunchHold)
        {
            SetArmSpring(s.upperJoint, windupSpringUpper, windupSpringUpper * 0.08f);
            SetArmSpring(s.lowerJoint, windupSpringLower, windupSpringLower * 0.08f);
            ApplyWindupPose(s, 1f);
            return;
        }
        else if (s.state == ArmState.PunchOut)
        {
            s.reach = Mathf.MoveTowards(s.reach, 1f, punchOutSpeed * dt);
            if (s.reach >= 0.99f) s.state = ArmState.PunchRetract;

            SetArmSpring(s.upperJoint, punchSpringUpper, punchSpringUpper * 0.08f);
            SetArmSpring(s.lowerJoint, punchSpringLower, punchSpringLower * 0.08f);

            // Blend straight from the cocked pose to the extended pose. Going
            // through ApplyArm here would retrace the idle pose at reach 0.
            ApplyPunchPose(s, Mathf.InverseLerp(-windupAmount, 1f, s.reach));
        }
        else
        {
            s.reach = Mathf.MoveTowards(s.reach, 0f, punchBackSpeed * dt);
            if (s.reach <= 0.01f) s.state = ArmState.Free;

            ApplyArmSprings(s, s.reach);
            ApplyArm(s, punchElbowAngle);
        }
    }

    public void Punch(bool left)
    {
        ArmSide s = Side(left);
        if (s.state != ArmState.Free) return;
        s.windupStartReach = s.reach;
        s.state = ArmState.PunchWindup;
    }

    public void ReleasePunch(bool left)
    {
        ArmSide s = Side(left);
        if (s.state == ArmState.PunchHold || s.state == ArmState.PunchWindup)
            s.state = ArmState.PunchOut;
    }

    public void CancelPunch(bool left)
    {
        ArmSide s = Side(left);
        if (s.state == ArmState.PunchOut || s.state == ArmState.PunchRetract)
            s.state = ArmState.Free;
    }

    // The cocked pose: upper arm swung back and out, elbow bent.
    // Stacked eulers are avoided for the shoulder because composing pullback and
    // side rotations that way adds a parasitic roll around the bone axis, which
    // visibly twists the whole forearm once the elbow is bent. Instead the euler
    // only picks the *direction* the bone should point, and FromToRotation
    // produces the minimal (twist-free) swing toward it. windupTwist then rolls
    // the arm deliberately to aim the elbow fold.
    void WindupTargets(ArmSide s, out Quaternion upperTarget, out Quaternion lowerTarget)
    {
        // Local +X swings the arm forward on both sides (same convention as the
        // reach pose), so pullback is -X. The lateral swing is mirrored between
        // sides: -Z is outward for the left arm, +Z for the right.
        float sideSign = s.isLeft ? -1f : 1f;
        Vector3 dir = Quaternion.Euler(
            -windupPullbackAngle * s.shoulderSign,
            0f,
            windupSideAngle * sideSign) * Vector3.up;

        Quaternion swing = Quaternion.FromToRotation(Vector3.up, dir);
        Quaternion roll = Quaternion.AngleAxis(windupTwist * sideSign, Vector3.up);

        upperTarget = s.initUpper * swing * roll;
        lowerTarget = s.initLower * Quaternion.Euler(windupElbowBend * s.elbowSign, 0f, 0f);
    }

    void ApplyWindupPose(ArmSide s, float t)
    {
        WindupTargets(s, out Quaternion upperTarget, out Quaternion lowerTarget);
        s.upper.localRotation = Quaternion.Slerp(s.initUpper, upperTarget, t);
        s.lower.localRotation = Quaternion.Slerp(s.initLower, lowerTarget, t);
    }

    // Punch trajectory: cocked pose -> fully extended reach pose, directly.
    void ApplyPunchPose(ArmSide s, float t)
    {
        WindupTargets(s, out Quaternion windupUpper, out Quaternion windupLower);
        Quaternion punchUpper = s.initUpper * Quaternion.Euler(shoulderAngle * s.shoulderSign, 0f, 0f);
        Quaternion punchLower = s.initLower * Quaternion.Euler(punchElbowAngle * s.elbowSign, 0f, 0f);

        s.upper.localRotation = Quaternion.Slerp(windupUpper, punchUpper, t);
        s.lower.localRotation = Quaternion.Slerp(windupLower, punchLower, t);
    }
}
