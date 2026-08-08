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

    private bool _heldL, _heldR, _grabModeL, _grabModeR;

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


        // arms, grabbing & punching
        // left
        bool pressedL = input.GrabL;
        bool grabMode = input.Modifier;


        if (pressedL && !_heldL)            // press edge
        {
            _heldL = true;
            _grabModeL = grabMode;         // latch the mode at press time
            if (!_grabModeL) arms.Punch(true);
        }
        else if (!pressedL && _heldL)       // release edge
        {
            _heldL = false;
            if (!_grabModeL) arms.ReleasePunch(true);
            else grab.Release(true);
        }

        float targetL = (_heldL && _grabModeL) ? 1f : 0f;
        arms.TickArm(true, targetL, Time.fixedDeltaTime);
        if (_heldL && _grabModeL && arms.Reach(true) > 0.6f) grab.TryGrab(true);

        // right
        bool pressedR = input.GrabR;
        if (pressedR && !_heldR)            // press edge
        {
            _heldR = true;
            _grabModeR = grabMode;         // latch the mode at press time
            if (!_grabModeR) arms.Punch(false);
        }
        else if (!pressedR && _heldR)       // release edge
        {
            _heldR = false;
            if (!_grabModeR) arms.ReleasePunch(false);
            else grab.Release(false);
        }

        float targetR = (_heldR && _grabModeR) ? 1f : 0f;
        arms.TickArm(false, targetR, Time.fixedDeltaTime);
        if (_heldR && _grabModeR && arms.Reach(false) > 0.6f) grab.TryGrab(false);




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
