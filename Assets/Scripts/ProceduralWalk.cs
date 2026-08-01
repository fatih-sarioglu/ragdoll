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

    [Header("Idle")]
    [SerializeField] float restBlendSpeed = 8f;

    [Header("Debug")]
    [SerializeField] bool scrubMode;
    [SerializeField, Range(0f, 1f)] float scrubPhase;

    public bool Wrapped { get; private set; }

    float _phase;
    Quaternion _initUpperL, _initUpperR, _initLowerL, _initLowerR;

    void Awake()
    {
        _initUpperL = upperLegL.localRotation;
        _initUpperR = upperLegR.localRotation;
        _initLowerL = lowerLegL.localRotation;
        _initLowerR = lowerLegR.localRotation;
    }

    public void TickCycle(StepInfo gait, float dt)
    {
        if (scrubMode)
        {
            _phase = scrubPhase;
            Wrapped = false;
        }
        else
        {
            float prev = _phase;
            _phase = (_phase + dt / gait.stepDuration) % 1f;
            Wrapped = _phase < prev;   // modulo sent us back past 0 > cycle completed
        }

        // apply movements
        ApplyLeg(gait, upperLegL, lowerLegL, _initUpperL, _initLowerL, _phase, kneeSignL, hipSignL, "L");
        ApplyLeg(gait, upperLegR, lowerLegR, _initUpperR, _initLowerR, (_phase + 0.5f) % 1f, kneeSignR, hipSignR, "R");

    }

    public void ResetPhase(float phase) { _phase = phase; }

    void ApplyLeg(StepInfo gait, Transform upper, Transform lower, Quaternion initUpper, Quaternion initLower, float phase, float kneeSign, float hipSign, string label)
    {
        float hipAngle = gait.upperLegCurve.Evaluate(phase) * gait.upperLegMultiplier;
        float kneeAngle = gait.lowerLegCurve.Evaluate(phase) * gait.lowerLegMultiplier;

        upper.localRotation = initUpper * Quaternion.Euler(0, 0, hipAngle * hipSign);
        lower.localRotation = initLower * Quaternion.Euler(0, 0, kneeAngle * kneeSign);

        if (scrubMode) Debug.Log($"{label} phase={phase:F2} hip={hipAngle:F1} knee={kneeAngle:F1}");
    }

    public void BlendToPose(float hipAngle, float kneeAngle, float speed)
    {
        float t = speed * Time.fixedDeltaTime;

        Quaternion targetUpperL = _initUpperL * Quaternion.Euler(0, 0, hipAngle * hipSignL);
        Quaternion targetUpperR = _initUpperR * Quaternion.Euler(0, 0, hipAngle * hipSignR);
        Quaternion targetLowerL = _initLowerL * Quaternion.Euler(0, 0, hipAngle * kneeSignL);
        Quaternion targetLowerR = _initLowerR * Quaternion.Euler(0, 0, hipAngle * kneeSignR);

        upperLegL.localRotation = Quaternion.Slerp(upperLegL.localRotation, targetUpperL, t);
        upperLegR.localRotation = Quaternion.Slerp(upperLegR.localRotation, targetUpperR, t);
        lowerLegL.localRotation = Quaternion.Slerp(lowerLegL.localRotation, targetLowerL, t);
        lowerLegR.localRotation = Quaternion.Slerp(lowerLegR.localRotation, targetLowerR, t);
    }

    public void BlendToRest()
    {
        upperLegL.localRotation = Quaternion.Slerp(upperLegL.localRotation, _initUpperL, restBlendSpeed * Time.fixedDeltaTime);
        upperLegR.localRotation = Quaternion.Slerp(upperLegR.localRotation, _initUpperR, restBlendSpeed * Time.fixedDeltaTime);
        lowerLegL.localRotation = Quaternion.Slerp(lowerLegL.localRotation, _initLowerL, restBlendSpeed * Time.fixedDeltaTime);
        lowerLegR.localRotation = Quaternion.Slerp(lowerLegR.localRotation, _initLowerR, restBlendSpeed * Time.fixedDeltaTime);
    }
}
