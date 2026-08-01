using UnityEngine;

public class CameraController : MonoBehaviour
{
    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}
