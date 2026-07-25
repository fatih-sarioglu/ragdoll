using System.Collections.Generic;
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

    [SerializeField] RagdollStepData stepData;
    StepInfo _active;

    [Header("Input")]
    [SerializeField] PlayerInputReader input;

    [Header("Direction")]
    [SerializeField] bool walkBackwards;   // manual switch for now
    StepInfo _pending;

    [Header("Idle")]
    [SerializeField] float restBlendSpeed = 8f;

    [Header("Debug")]
    [SerializeField] bool scrubMode;
    [SerializeField, Range(0f, 1f)] float scrubPhase;

    float _phase;
    Quaternion _initUpperL, _initUpperR, _initLowerL, _initLowerR;

    void Awake()
    {
        if (stepData == null || input == null)
        {
            Debug.LogError("ProceduralWalk: no StepData assigned.", this);
            enabled = false;
            
            return;
        }

        _initUpperL = upperLegL.localRotation;
        _initUpperR = upperLegR.localRotation;
        _initLowerL = lowerLegL.localRotation;
        _initLowerR = lowerLegR.localRotation;

        _active = null;
    }

    void FixedUpdate()
    {
        float moveY = input.Move.y;
        const float deadzone = 0.1f;

        if (Mathf.Abs(moveY) < deadzone)
            _pending = null;                          // null = idle
        else
            _pending = moveY > 0 ? stepData.forwards : stepData.backwards;


        if (_active == null)
        {
            BlendToRest();

            if (_pending != null)
            {
                _active = _pending;
                _phase = 0f;
            }
            return;
        }

        if (scrubMode) _phase = scrubPhase;
        else
        {
            float prev = _phase;
            _phase = (_phase + Time.fixedDeltaTime / _active.stepDuration) % 1f;
            bool wrapped = _phase < prev;   // modulo sent us back past 0 > cycle completed

            if (wrapped && _pending != _active) _active = _pending;
            if (_active == null) return;
        }

        // apply movements
        ApplyLeg(upperLegL, lowerLegL, _initUpperL, _initLowerL, _phase, kneeSignL, hipSignL, "L");
        ApplyLeg(upperLegR, lowerLegR, _initUpperR, _initLowerR, (_phase + 0.5f) % 1f, kneeSignR, hipSignR, "R");
    }

    void ApplyLeg(Transform upper, Transform lower, Quaternion initUpper, Quaternion initLower, float phase, float kneeSign, float hipSign, string label)
    {
        float hipAngle = _active.upperLegCurve.Evaluate(phase) * _active.upperLegMultiplier;
        float kneeAngle = _active.lowerLegCurve.Evaluate(phase) * _active.lowerLegMultiplier;

        upper.localRotation = initUpper * Quaternion.Euler(0, 0, hipAngle * hipSign);
        lower.localRotation = initLower * Quaternion.Euler(0, 0, kneeAngle * kneeSign);

        if (scrubMode) Debug.Log($"{label} phase={phase:F2} hip={hipAngle:F1} knee={kneeAngle:F1}");
    }

    void BlendToRest()
    {
        upperLegL.localRotation = Quaternion.Slerp(upperLegL.localRotation, _initUpperL, restBlendSpeed * Time.fixedDeltaTime);
        upperLegR.localRotation = Quaternion.Slerp(upperLegR.localRotation, _initUpperR, restBlendSpeed * Time.fixedDeltaTime);
        lowerLegL.localRotation = Quaternion.Slerp(lowerLegL.localRotation, _initLowerL, restBlendSpeed * Time.fixedDeltaTime);
        lowerLegR.localRotation = Quaternion.Slerp(lowerLegR.localRotation, _initLowerR, restBlendSpeed * Time.fixedDeltaTime);
    }
}
