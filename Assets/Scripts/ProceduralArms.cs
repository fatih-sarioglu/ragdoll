using UnityEngine;

public class ProceduralArms : MonoBehaviour
{
    [Header("Bones")]
    [SerializeField] private Transform upperArmL;
    [SerializeField] private Transform upperArmR;
    [SerializeField] private Transform lowerArmL;
    [SerializeField] private Transform lowerArmR;

    [Header("Joints")]
    [SerializeField] private ConfigurableJoint upperArmJointL;
    [SerializeField] private ConfigurableJoint upperArmJointR;
    [SerializeField] private ConfigurableJoint lowerArmJointL;
    [SerializeField] private ConfigurableJoint lowerArmJointR;

    [Header("Axis signs (flip for mirrored bones)")]
    [SerializeField] float elbowSignL = 1f;
    [SerializeField] float elbowSignR = 1f;
    [SerializeField] float shoulderSignL = 1f;
    [SerializeField] float shoulderSignR = 1f;

    [Header("Reach pose")]
    [SerializeField] float shoulderAngle = 90f;   // degrees at full reach
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

    Quaternion _initUpperL, _initUpperR, _initLowerL, _initLowerR;
    float _reachL, _reachR;

    void Awake()
    {
        _initUpperL = upperArmL.localRotation;
        _initUpperR = upperArmR.localRotation;
        _initLowerL = lowerArmL.localRotation;
        _initLowerR = lowerArmR.localRotation;
    }

    public void TickArm(bool left, float reachTarget, float dt)
    {
        if (left)
        {
            _reachL = Mathf.MoveTowards(_reachL, reachTarget, blendSpeed * dt);
            ApplyArm(upperArmL, lowerArmL, _initUpperL, _initLowerL, upperArmJointL, lowerArmJointL, elbowSignL, shoulderSignL, _reachL);
        }
        else
        {
            _reachR = Mathf.MoveTowards(_reachR, reachTarget, blendSpeed * dt);
            ApplyArm(upperArmR, lowerArmR, _initUpperR, _initLowerR, upperArmJointR, lowerArmJointR, elbowSignR, shoulderSignR, _reachR);
        }
    }

    void ApplyArm(Transform upper, Transform lower, Quaternion initUpper, Quaternion initLower, ConfigurableJoint upperJoint, ConfigurableJoint lowerJoint,
        float elbowSign, float shoulderSign, float reach)
    {
        upper.localRotation = initUpper * Quaternion.Euler(reach * shoulderAngle * shoulderSign, 0, 0);
        lower.localRotation = initLower * Quaternion.Euler(reach * elbowAngle * elbowSign, 0, 0);

        float spring = Mathf.Lerp(limpSpringUpper, reachSpringUpper, reach);
        float damper = Mathf.Lerp(limpDamperUpper, reachDamperUpper, reach);

        SetArmSpring(upperJoint, spring, damper);
        SetArmSpring(lowerJoint, spring, damper);
    }

    private void SetArmSpring(ConfigurableJoint joint, float spring, float damper)
    {
        JointDrive d = joint.slerpDrive;
        d.positionSpring = spring;
        d.positionDamper = damper;
        joint.slerpDrive = d;
    }
}
