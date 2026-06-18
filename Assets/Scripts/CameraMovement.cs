using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private float sensitivity = 0.1f;

    [SerializeField]
    private float minPitch = -85f;

    [SerializeField]
    private float maxPitch = 85f;

    [SerializeField]
    private Transform cameraTransform;

    private InputAction lookAction;
    private float pitch;

    private void Awake()
    {
        lookAction = InputSystem.actions.FindAction("Player/Look");
    }

    private void OnEnable()
    {
        lookAction.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        lookAction.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>() * sensitivity;

        transform.Rotate(Vector3.up, lookInput.x);

        pitch = Mathf.Clamp(pitch - lookInput.y, minPitch, maxPitch);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
