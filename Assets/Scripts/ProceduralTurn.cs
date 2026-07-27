using UnityEngine;

public class ProceduralTurn : MonoBehaviour
{
    [SerializeField] Transform hipsAnimated;
    [SerializeField] PlayerInputReader input;
    [SerializeField] float turnSpeed = 120f;

    float _yaw;
    Quaternion _initHips;

    void Awake() => _initHips = hipsAnimated.localRotation;

    void FixedUpdate()
    {
        float turn = input.Move.x;
        if (Mathf.Abs(turn) < 0.1f) turn = 0f;

        _yaw += turn * turnSpeed * Time.fixedDeltaTime;
        hipsAnimated.localRotation = Quaternion.Euler(0f, 0f, _yaw) * _initHips;
    }
}