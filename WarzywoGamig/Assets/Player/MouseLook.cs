using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 3f; // Ustawienie ni�szej czu�o�ci
    [SerializeField] private Transform playerBody;

    private float xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        // Pobierz ruch myszy, ale bez skalowania przez Time.deltaTime
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // Zmniejszanie wp�ywu niestabilnych ruch�w myszy
        if (Mathf.Abs(mouseX) < 0.01f) mouseX = 0f;
        if (Mathf.Abs(mouseY) < 0.01f) mouseY = 0f;

        // Obr�t kamery w osi X (g�ra/d�)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Obr�t gracza w osi Y
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
