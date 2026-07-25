using UnityEngine;

[System.Serializable]
public class StepInfo
{
    public AnimationCurve upperLegCurve;
    public AnimationCurve lowerLegCurve;
    public float stepDuration = 0.5f; // seconds per full cycle
    public float upperLegMultiplier = 30f;  // degrees at curve value 1
    public float lowerLegMultiplier = 45f;
}

[CreateAssetMenu(fileName = "RagdollStepData", menuName = "RagDoll/Step Data")]
public class RagdollStepData : ScriptableObject
{
    public StepInfo forwards;
    public StepInfo backwards;
}
