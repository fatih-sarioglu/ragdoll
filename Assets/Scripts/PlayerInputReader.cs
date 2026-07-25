using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    PlayerControls _playerControls;
    public Vector2 Move { get; private set; }

    void Awake() => _playerControls = new PlayerControls();
    void OnEnable() => _playerControls.Gameplay.Enable();
    void OnDisable() => _playerControls.Gameplay.Disable();

    void Update() => Move = _playerControls.Gameplay.Move.ReadValue<Vector2>();
}
