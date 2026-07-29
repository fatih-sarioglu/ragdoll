using UnityEngine;

public class ProceduralJump : MonoBehaviour
{
    [Header("Feet")]
    [SerializeField] private Transform footL;
    [SerializeField] private Transform footR;

    [Header("Layers")]
    [SerializeField] private LayerMask groundMask;

    [Header("Input")]
    [SerializeField] PlayerInputReader input;

    [Header("Physics")]
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private float jumpVelocity = 1f;


    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (input.JumpBuffered)
        {
            foreach (Rigidbody rb in ragdollRoot.GetComponentsInChildren<Rigidbody>())
                rb.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);

            input.ConsumeJump();
        }
    }

    void Update()
    {
        Debug.DrawRay(footL.position + Vector3.up * 0.1f, Vector3.down * 0.25f, IsGrounded ? Color.green : Color.red);
        Debug.DrawRay(footR.position + Vector3.up * 0.1f, Vector3.down * 0.25f, IsGrounded ? Color.green : Color.red);
    }

    bool IsGrounded => FootGrounded(footL) || FootGrounded(footR);

    private bool FootGrounded(Transform foot)
    {
        return Physics.Raycast(foot.position + Vector3.up * 0.1f, Vector3.down, 0.25f, groundMask);
    }
}
