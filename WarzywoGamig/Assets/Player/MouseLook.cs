using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 3f; // Ustawienie ni¿szej czu³oœci
    [SerializeField] private Transform playerBody;

    private float xRotation = 0f;

    private TurretController turretController;

    public static MouseLook Instance;

    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject);

    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        turretController = Object.FindFirstObjectByType<TurretController>();
        if (turretController == null)
        {
            Debug.LogWarning("Brak obiektu TurretController w scenie.");
        }
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

        // Zmniejszanie wp³ywu niestabilnych ruchów myszy
        if (Mathf.Abs(mouseX) < 0.01f) mouseX = 0f;
        if (Mathf.Abs(mouseY) < 0.01f) mouseY = 0f;

        // Obrót kamery w osi X (góra/dó³)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Obrót gracza w osi Y
        playerBody.Rotate(Vector3.up * mouseX);

        if (turretController != null && turretController.isUsingTurret)
        {
            xRotation = Mathf.Clamp(xRotation, turretController.minBarrelAngle, turretController.maxBarrelAngle);
        }
        else
        {
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        }

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
