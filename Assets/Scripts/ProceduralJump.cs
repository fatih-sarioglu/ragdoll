using System.Collections.Generic;
using UnityEngine;

public class ProceduralJump : MonoBehaviour
{
    [Header("Feet")]
    [SerializeField] private Transform footL;
    [SerializeField] private Transform footR;

    [Header("Layers")]
    [SerializeField] private LayerMask groundMask;

    [Header("Physics")]
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private float jumpVelocity = 1f;
    [SerializeField] private float rayOffset = 0.1f;
    [SerializeField] private float rayLength = 0.25f;


    bool _groundedL, _groundedR;
    public bool IsGrounded => _groundedL || _groundedR;

    private Rigidbody[] _ragdollBones;

    void Awake()
    {
        _ragdollBones = ragdollRoot.GetComponentsInChildren<Rigidbody>();
    }

    public void Tick()
    {
        _groundedL = FootGrounded(footL);
        _groundedR = FootGrounded(footR);
    }

    public void Launch()
    {
        foreach (Rigidbody rb in _ragdollBones)
            rb.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
    }

    void Update()
    {
        Debug.DrawRay(footL.position + Vector3.up * rayOffset, Vector3.down * rayLength, _groundedL ? Color.green : Color.red);
        Debug.DrawRay(footR.position + Vector3.up * rayOffset, Vector3.down * rayLength, _groundedR ? Color.green : Color.red);
    }


    private bool FootGrounded(Transform foot)
    {
        return Physics.Raycast(foot.position + Vector3.up * rayOffset, Vector3.down, rayLength, groundMask);
    }
}
