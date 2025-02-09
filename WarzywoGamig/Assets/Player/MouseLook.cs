using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform playerBody;

    private float xRotation = 0f;

    private void Start()
    {
        // Ukryj i zablokuj kursor na �rodku ekranu
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        // Pobierz ruch myszy
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Obr�� kamer� w osi X (g�ra/d�)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Ogranicz obr�t kamery, aby unikn�� obrotu o 360 stopni

        // Zastosuj obr�t kamery
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Obr�� gracza w osi Y (lewo/prawo)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}