using UnityEngine;

public class ProceduralTurn : MonoBehaviour
{
    [SerializeField] Transform hipsAnimated;
    [SerializeField] float turnSpeed = 120f;

    Quaternion _initHips;

    void Awake() => _initHips = hipsAnimated.localRotation;

    private float _yaw, _targetYaw;

    public void SetTargetYaw(float yaw)
    {
        _targetYaw = yaw;
    }

    public void Tick(float dt)
    {
        _yaw = Mathf.MoveTowardsAngle(_yaw, _targetYaw, turnSpeed * dt);
        hipsAnimated.localRotation = Quaternion.Euler(0f, 0f, _yaw) * _initHips;
    }
}