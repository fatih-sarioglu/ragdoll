using UnityEngine;

public class RagdollLocomotion : MonoBehaviour
{
    [Header("Modules")]
    [SerializeField] ProceduralWalk walk;
    [SerializeField] ProceduralJump jump;

    [Header("Input")]
    [SerializeField] PlayerInputReader input;

    enum LocoState { Idle, Walking, Airborne }
    LocoState _state = LocoState.Idle;

    [Header("Walk")]
    [SerializeField] RagdollStepData stepData;

    StepInfo _active;
    StepInfo _pending;

    [Header("Jump")]
    [SerializeField] Rigidbody hipsRb;

    [SerializeField] float minAirTime;
    private float _airTime;

    [SerializeField] float tuckHip, tuckKnee;
    [SerializeField] float tuckBlendSpeed;



    void FixedUpdate()
    {
        jump.Tick();

        float moveY = input.Move.y;
        const float deadzone = 0.1f;
        if (Mathf.Abs(moveY) < deadzone)
            _pending = null;                          // null = idle
        else
            _pending = moveY > 0 ? stepData.forwards : stepData.backwards;


        bool jumpPressed = input.JumpBuffered;
        input.ConsumeJump();

        if (_state != LocoState.Airborne && jumpPressed && jump.IsGrounded)
        {
            jump.Launch();
            _state = LocoState.Airborne;
            _airTime = 0f;
        }
        else if (!jump.IsGrounded)
        {
            _state = LocoState.Airborne;
        }


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
