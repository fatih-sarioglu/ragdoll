using UnityEngine;

public class RootController : MonoBehaviour
{
    [SerializeField] private float positionSpring = 100f;
    [SerializeField] private float positionDamper = 10f;
    [SerializeField] private float rotationSpring = 100f;
    [SerializeField] private float rotationDamper = 10f;

    Rigidbody _rb;
    Vector3 _targetPosition;
    Quaternion _targetRotation; 

    void Awake() {
        _rb = GetComponent<Rigidbody>();
        _targetPosition = transform.position;
        _targetRotation = transform.rotation;
    }

    
    void FixedUpdate() {
        Vector3 gap = _targetPosition - _rb.position;
        Vector3 force = gap * positionSpring - _rb.linearVelocity * positionDamper;
        _rb.AddForce(force);

        Quaternion deltaRot = _targetRotation * Quaternion.Inverse(_rb.rotation);
        deltaRot.ToAngleAxis(out float angleDeg, out Vector3 axis);
        if (angleDeg > 180f) angleDeg -= 360f;

        if (!float.IsInfinity(axis.x) && Mathf.Abs(angleDeg) > 0.01f) {
            Vector3 torque = axis.normalized * (angleDeg * Mathf.Deg2Rad * rotationSpring) - _rb.angularVelocity * rotationDamper;
            _rb.AddTorque(torque);
        }
    }
}
