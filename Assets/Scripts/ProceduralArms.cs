using UnityEngine;


public enum ArmState { Free, PunchWindup, PunchHold, PunchOut, PunchRetract }

[System.Serializable]
public class ArmSide
{
    public Transform upper, lower;
    public ConfigurableJoint upperJoint, lowerJoint;
    public float shoulderSign = 1f, elbowSign = 1f;
    public Vector3 windupShoulderEuler;
    public float windupElbowAngle;

    [System.NonSerialized] public Quaternion initUpper, initLower;
    [System.NonSerialized] public float reach;
    [System.NonSerialized] public ArmState state;
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

    ArmSide Side(bool left) => left ? armL : armR;

    void Awake()
    {
        InitSide(armL);
        InitSide(armR);
    }

    void InitSide(ArmSide s)
    {
        s.initUpper = s.upper.localRotation;
        s.initLower = s.lower.localRotation;
        s.state = ArmState.Free;
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
            ApplyWindupPose(s, Mathf.Abs(s.reach) / windupAmount);
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
        }
        else
        {
            s.reach = Mathf.MoveTowards(s.reach, 0f, punchBackSpeed * dt);
            if (s.reach <= 0.01f) s.state = ArmState.Free;

            ApplyArmSprings(s, s.reach);
        }

        ApplyArm(s, punchElbowAngle);
    }

    public void Punch(bool left)
    {
        ArmSide s = Side(left);
        if (s.state != ArmState.Free) return;
        s.state = ArmState.PunchWindup;
    }

    public void ReleasePunch(bool left)
    {
        ArmSide s = Side(left);
        if (s.state == ArmState.PunchHold) s.state = ArmState.PunchOut;
    }

    public void CancelPunch(bool left)
    {
        ArmSide s = Side(left);
        if (s.state == ArmState.PunchOut || s.state == ArmState.PunchRetract)
            s.state = ArmState.Free;
    }

    void ApplyWindupPose(ArmSide s, float t)
    {
        Quaternion shoulderTarget = s.initUpper * Quaternion.Euler(s.windupShoulderEuler);
        Quaternion elbowTarget = s.initLower * Quaternion.Euler(s.windupElbowAngle, 0f, 0f);

        s.upper.localRotation = Quaternion.Slerp(s.initUpper, shoulderTarget, t);
        s.lower.localRotation = Quaternion.Slerp(s.initLower, elbowTarget, t);
    }
}
