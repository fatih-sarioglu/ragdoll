using UnityEngine;

public class ProceduralArms : MonoBehaviour
{
    [Header("Bones")]
    [SerializeField] private Transform upperArmL;
    [SerializeField] private Transform upperArmR;
    [SerializeField] private Transform lowerArmL;
    [SerializeField] private Transform lowerArmR;

    [Header("Axis signs (flip for mirrored bones)")]
    [SerializeField] float elbowSignL = 1f;
    [SerializeField] float elbowSignR = 1f;

    [SerializeField] float shoulderSignL = 1f;
    [SerializeField] float shoulderSignR = 1f;

    Quaternion _initUpperL, _initUpperR, _initLowerL, _initLowerR;

    void Awake()
    {
        _initUpperL = upperArmL.localRotation;
        _initUpperR = upperArmR.localRotation;
        _initLowerL = lowerArmL.localRotation;
        _initLowerR = lowerArmR.localRotation;
    }


}
