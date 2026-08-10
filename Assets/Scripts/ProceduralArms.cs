using UnityEngine;


[System.Serializable]
public class ArmSide
{
    public Transform upper, lower;
    public ConfigurableJoint upperJoint, lowerJoint;
    public float shoulderSign = 1f, elbowSign = 1f;

    [System.NonSerialized] public Quaternion initUpper, initLower;
    [System.NonSerialized] public float reach;
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
    }

    public float Reach(bool left) => Side(left).reach;

    public void TickArm(bool left, float reachTarget, float dt)
    {
        ArmSide s = Side(left);

        s.reach = Mathf.MoveTowards(s.reach, reachTarget, blendSpeed * dt);
        ApplyArm(s);
        ApplyArmSprings(s, s.reach);
    }

    void ApplyArm(ArmSide s)
    {
        s.upper.localRotation = s.initUpper * Quaternion.Euler(s.reach * shoulderAngle * s.shoulderSign, 0, 0);
        s.lower.localRotation = s.initLower * Quaternion.Euler(s.reach * elbowAngle * s.elbowSign, 0, 0);
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
}
