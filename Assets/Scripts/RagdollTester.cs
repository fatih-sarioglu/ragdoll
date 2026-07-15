using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ragdoll test harness — new Input System version.
/// Attach to the Main Camera, assign 'target' (character hips/root).
/// Requires the Input System package (com.unity.inputsystem).
///
/// Controls:
///   Right mouse drag ... orbit around the character
///   Scroll wheel ....... zoom in/out
///   Left click ......... fire a sphere at wherever the cursor points
///   1 / 2 / 3 .......... light / medium / heavy projectile
///   T .................. toggle limp (optional, needs an ActiveRagdoll reference)
///   Space (hold) ....... slow motion
/// </summary>
public class RagdollTester : MonoBehaviour
{
    [Header("Target")]
    public Transform target;              // character hips/root to orbit around
    // public ActiveRagdoll activeRagdoll;   // optional — leave empty until you write yours

    [Header("Orbit camera")]
    public float distance = 5f;
    public float minDistance = 1.5f;
    public float maxDistance = 15f;
    [Tooltip("Mouse delta is in pixels/frame in the new Input System, so this is much smaller than old-Input speeds.")]
    public float orbitSpeed = 0.2f;
    public float zoomSpeed = 0.002f;

    [Header("Projectile")]
    public float launchSpeed = 20f;
    public float projectileLifetime = 5f;

    float _yaw = 0f;
    float _pitch = 15f;
    int _sizeIndex = 1; // 0 light, 1 medium, 2 heavy
    bool _limp;

    static readonly float[] Masses = { 0.5f, 3f, 15f };
    static readonly float[] Radii  = { 0.1f, 0.2f, 0.35f };
    static readonly Color[] Colors = { Color.yellow, new Color(1f, 0.5f, 0f), Color.red };

    void Update()
    {
        Mouse mouse = Mouse.current;
        Keyboard kb = Keyboard.current;
        if (target == null || mouse == null || kb == null) return;

        // --- Orbit ---
        if (mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            _yaw += delta.x * orbitSpeed;
            _pitch -= delta.y * orbitSpeed;
            _pitch = Mathf.Clamp(_pitch, -20f, 80f);
        }

        // Scroll is in pixels (~120 per notch), hence the tiny zoomSpeed.
        float scroll = mouse.scroll.ReadValue().y;
        distance = Mathf.Clamp(distance - scroll * zoomSpeed * distance, minDistance, maxDistance);

        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 focus = target.position + Vector3.up * 0.5f;
        transform.position = focus + rot * new Vector3(0f, 0f, -distance);
        transform.LookAt(focus);

        // --- Projectile size selection ---
        if (kb.digit1Key.wasPressedThisFrame) _sizeIndex = 0;
        if (kb.digit2Key.wasPressedThisFrame) _sizeIndex = 1;
        if (kb.digit3Key.wasPressedThisFrame) _sizeIndex = 2;

        // --- Fire ---
        if (mouse.leftButton.wasPressedThisFrame)
            Fire(mouse.position.ReadValue());

        // // --- Limp toggle ---
        // if (kb.tKey.wasPressedThisFrame && activeRagdoll != null)
        // {
        //     _limp = !_limp;
        //     activeRagdoll.SetLimp(_limp);
        // }

        // --- Slow motion ---
        if (kb.spaceKey.wasPressedThisFrame)
        {
            Time.timeScale = 0.2f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // keep physics steps proportional
        }
        if (kb.spaceKey.wasReleasedThisFrame)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    void Fire(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "TestProjectile";
        float radius = Radii[_sizeIndex];
        ball.transform.localScale = Vector3.one * (radius * 2f);
        ball.transform.position = ray.origin + ray.direction * 0.5f;

        ball.GetComponent<Renderer>().material.color = Colors[_sizeIndex];

        Rigidbody rb = ball.AddComponent<Rigidbody>();
        rb.mass = Masses[_sizeIndex];
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // fast + small = tunneling risk

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = ray.direction * launchSpeed;
#else
        rb.velocity = ray.direction * launchSpeed;
#endif

        Destroy(ball, projectileLifetime);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 500, 120),
            $"RMB drag: orbit | Scroll: zoom | LMB: fire\n" +
            $"1/2/3: projectile size (current: {new[] { "light", "medium", "heavy" }[_sizeIndex]})\n" +
            $"T: toggle limp ({(_limp ? "LIMP" : "active")}) | Space: slow-mo");
    }
}