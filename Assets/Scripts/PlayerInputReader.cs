using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    PlayerControls _playerControls;
    public Vector2 Move { get; private set; }

    public bool JumpBuffered { get; private set; }

    void Awake() => _playerControls = new PlayerControls();
    void OnEnable() => _playerControls.Gameplay.Enable();
    void OnDisable() => _playerControls.Gameplay.Disable();

    void Update()
    {
        Move = _playerControls.Gameplay.Move.ReadValue<Vector2>();
        if (_playerControls.Gameplay.Jump.WasPressedThisFrame()) JumpBuffered = true; 
    }

    public void ConsumeJump() => JumpBuffered = false;
}
