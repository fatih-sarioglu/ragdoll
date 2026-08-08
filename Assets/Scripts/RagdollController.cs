using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [Header("Modules")]
    [SerializeField] ProceduralWalk walk;
    [SerializeField] ProceduralJump jump;
    [SerializeField] ProceduralTurn turn;
    [SerializeField] ProceduralArms arms;
    [SerializeField] GrabSystem grab;

    [Header("Input")]
    [SerializeField] PlayerInputReader input;

    [Header("Camera")]
    [SerializeField] Transform cameraTransform;

    enum LocoState { Idle, Walking, Airborne }
    LocoState _state = LocoState.Idle;

    [Header("Walk")]
    [SerializeField] RagdollStepData stepData;

    StepInfo _active;
    StepInfo _pending;

    [Header("Jump")]
    [SerializeField] Rigidbody hipsRb;

    [SerializeField] float coyoteTime;
    private float _ungroundedTime = 0.15f;
    [SerializeField] float minAirTime = 0.25f;
    private float _airTime;

    [SerializeField] float tuckHip, tuckKnee;
    [SerializeField] float tuckBlendSpeed;

    private bool _wasGrabbingL, _wasGrabbingR;

    void FixedUpdate()
    {
        // walk/turn
        Vector3 camF = cameraTransform.forward;
        camF.y = 0f;
        camF.Normalize();
        
        Vector3 camR = cameraTransform.right;
        camR.y = 0f;
        camR.Normalize();

        Vector3 moveDir = camF * input.Move.y + camR * input.Move.x;

        const float deadzone = 0.1f;
        bool wantsMove = moveDir.sqrMagnitude > deadzone * deadzone;

        if (wantsMove)
        {
            float targetYaw = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            turn.SetTargetYaw(targetYaw);
            _pending = stepData.forwards;
        }
        else
        {
            _pending = null;
        }
        turn.Tick(Time.fixedDeltaTime);

        // jump/airborne
        jump.Tick();
        bool jumpPressed = input.JumpBuffered;
        input.ConsumeJump();

        if (_state != LocoState.Airborne && jumpPressed && jump.IsGrounded)
        {
            jump.Launch();
            _state = LocoState.Airborne;
            _airTime = 0f;
        }


        if (jump.IsGrounded) _ungroundedTime = 0f;
        else _ungroundedTime += Time.fixedDeltaTime;
        if (_state != LocoState.Airborne && _ungroundedTime > coyoteTime)
        { 
            _state = LocoState.Airborne;
            _airTime = 0f;
        }

        // arms
        arms.TickArm(true, input.GrabL ? 1f : 0f, Time.fixedDeltaTime);
        arms.TickArm(false, input.GrabR ? 1f : 0f, Time.fixedDeltaTime);


        // grabbing
        bool wantsGrabL = input.GrabL;
        if (wantsGrabL && arms.Reach(true) > 0.6f) grab.TryGrab(true);
        else if (!wantsGrabL) grab.Release(true);

        bool wantsGrabR = input.GrabR;
        if (wantsGrabR && arms.Reach(false) > 0.6f) grab.TryGrab(false);
        else if (!wantsGrabR) grab.Release(false);


        switch (_state)
            {
                case LocoState.Idle:
                    {
                        walk.BlendToRest();
                        if (_pending != null)
                        {
                            _active = _pending;
                            walk.ResetPhase(0f);
                            _state = LocoState.Walking;
                        }
                        break;
                    }
                case LocoState.Walking:
                    {
                        walk.TickCycle(_active, Time.fixedDeltaTime);
                        if (walk.Wrapped)
                        {
                            if (_pending == null)
                                _state = LocoState.Idle;
                            else if (_pending != _active)
                                _active = _pending;
                        }
                        break;
                    }
                case LocoState.Airborne:
                    {
                        _airTime += Time.fixedDeltaTime;
                        walk.BlendToPose(tuckHip, tuckKnee, tuckBlendSpeed);
                        if (_airTime > minAirTime && jump.IsGrounded && hipsRb.linearVelocity.y < 0f)
                            _state = LocoState.Idle;
                        break;
                    }
            }
    }
}
