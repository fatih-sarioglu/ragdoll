using UnityEngine;

public class GrabSystem : MonoBehaviour
{
    [Header("Grab")]
    [SerializeField] Transform handL;
    [SerializeField] Transform handR;
    [SerializeField] LayerMask grabMask;
    [SerializeField] float grabRadius = 0.25f;
    [SerializeField] Transform lowerArmL_Ragdoll;
    [SerializeField] Transform lowerArmR_Ragdoll;
    [SerializeField] float grabBreakForce = 5000f;
    [SerializeField] float grabBreakTorque = 5000f;

    private FixedJoint _grabJointL, _grabJointR;

    public bool IsHolding(bool left)
    {
        if (left)
            return _grabJointL != null;
        else
            return _grabJointR != null;
    }

    public void TryGrab(bool left)
    {
        if (left)
        {
            if (_grabJointL != null) return;

            Rigidbody target = FindGrabTarget(handL);
            if (target != null)
            {
                _grabJointL = lowerArmL_Ragdoll.gameObject.AddComponent<FixedJoint>();
                _grabJointL.connectedBody = target;
                _grabJointL.breakForce = grabBreakForce;
                _grabJointL.breakTorque = grabBreakTorque;
            }
        }
        else
        {
            if (_grabJointR != null) return;

            Rigidbody target = FindGrabTarget(handR);
            if (target != null)
            {
                _grabJointR = lowerArmR_Ragdoll.gameObject.AddComponent<FixedJoint>();
                _grabJointR.connectedBody = target;
                _grabJointR.breakForce = grabBreakForce;
                _grabJointR.breakTorque = grabBreakTorque;
            }
        }
    }

    public void Release(bool left)
    {
        if (left) { if (_grabJointL != null) Destroy(_grabJointL); _grabJointL = null; }
        else { if (_grabJointR != null) Destroy(_grabJointR); _grabJointR = null; }
    }

    Rigidbody FindGrabTarget(Transform hand)
    {
        Collider[] hits = Physics.OverlapSphere(hand.position, grabRadius, grabMask);
        Rigidbody best = null;
        float bestDist = float.MaxValue;

        foreach (Collider c in hits)
        {
            if (c.attachedRigidbody == null) continue;
            float d = Vector3.Distance(hand.position, c.transform.position);
            if (d < bestDist) { bestDist = d; best = c.attachedRigidbody; }
        }
        return best;
    }

    void OnDrawGizmos()
    {
        if (handL == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(handL.position, grabRadius);
        Gizmos.DrawWireSphere(handR.position, grabRadius);
    }
}
