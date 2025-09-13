using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Params")]
    public float sensitivity = 2f;
    public float minY = -80f;
    public float maxY = 80f;

    [Header("References")]
    public InputActionReference look;
    public Transform cameraTransform;
    public Transform playerBody;

    private float rotationX;

    void Awake()
    {
        look.action.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 lookInput = look.action.ReadValue<Vector2>() * sensitivity;

        playerBody.Rotate(Vector3.up * lookInput.x);

        rotationX -= lookInput.y;
        rotationX = Mathf.Clamp(rotationX, minY, maxY);

        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}