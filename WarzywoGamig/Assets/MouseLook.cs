using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform playerBody;

    private float xRotation = 0f;

    private void Start()
    {
        // Ukryj i zablokuj kursor na œrodku ekranu
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

        // Obróæ kamerê w osi X (góra/dó³)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Ogranicz obrót kamery, aby unikn¹æ obrotu o 360 stopni

        // Zastosuj obrót kamery
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Obróæ gracza w osi Y (lewo/prawo)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}