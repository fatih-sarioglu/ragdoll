using UnityEngine;

public class ProceduralWalk : MonoBehaviour
{
    [Header("Bones")]
    [SerializeField] private Transform upperLegL;
    [SerializeField] private Transform upperLegR;
    [SerializeField] private Transform lowerLegL;
    [SerializeField] private Transform lowerLegR;

    [Header("Axis signs (flip for mirrored bones)")]
    [SerializeField] float kneeSignL = 1f;
    [SerializeField] float kneeSignR = 1f;

    [SerializeField] float hipSignL = 1f;
    [SerializeField] float hipSignR = 1f;

    [Header("Cycle")]
    [SerializeField] float cycleSpeed = 1f;          // cycles per second
    [SerializeField] AnimationCurve upperLegCurve;   // 0..1 time, -1..1 value
    [SerializeField] AnimationCurve lowerLegCurve;   // 0..1 time,  0..1 value
    [SerializeField] float upperLegAngle = 30f;
    [SerializeField] float lowerLegAngle = 45f;

    [Header("Debug")]
    [SerializeField] bool scrubMode;
    [SerializeField, Range(0f, 1f)] float scrubPhase;

    float _phase;
    Quaternion _initUpperL, _initUpperR, _initLowerL, _initLowerR;

    void Awake()
    {
        _initUpperL = upperLegL.localRotation;
        _initUpperR = upperLegR.localRotation;
        _initLowerL = lowerLegL.localRotation;
        _initLowerR = lowerLegR.localRotation;
    }

    void FixedUpdate()
    {
        if (scrubMode) _phase = scrubPhase;
        else _phase = (_phase + cycleSpeed * Time.fixedDeltaTime) % 1f;

        ApplyLeg(upperLegL, lowerLegL, _initUpperL, _initLowerL, _phase, kneeSignL, hipSignL, "L");
        ApplyLeg(upperLegR, lowerLegR, _initUpperR, _initLowerR, (_phase + 0.5f) % 1f, kneeSignR, hipSignR, "R");
    }

    void ApplyLeg(Transform upper, Transform lower, Quaternion initUpper, Quaternion initLower, float phase, float kneeSign, float hipSign, string label)
    {
        float hipAngle = upperLegCurve.Evaluate(phase) * upperLegAngle;
        float kneeAngle = lowerLegCurve.Evaluate(phase) * lowerLegAngle;

        upper.localRotation = initUpper * Quaternion.Euler(0, 0, hipAngle * hipSign);
        lower.localRotation = initLower * Quaternion.Euler(0, 0, kneeAngle * kneeSign);

        if (scrubMode) Debug.Log($"{label} phase={phase:F2} hip={hipAngle:F1} knee={kneeAngle:F1}");
    }
}
