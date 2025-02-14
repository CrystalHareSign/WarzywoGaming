using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Transform platform = null;
    private Vector3 lastPlatformPosition;
    private bool isJumping = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        MovePlayer();
    }

    private void LateUpdate()
    {
        if (platform != null && isGrounded)
        {
            // Oblicz różnicę pozycji platformy i zastosuj ją do gracza
            Vector3 platformMovement = platform.position - lastPlatformPosition;
            controller.enabled = false; // Wyłącz CharacterController, aby pozwolić na ręczne przesunięcie
            transform.position += platformMovement;
            controller.enabled = true; // Włącz ponownie CharacterController
        }

        if (platform != null)
        {
            lastPlatformPosition = platform.position;
        }
    }

    private void MovePlayer()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            isJumping = false;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
            platform = null;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Floor") && !isJumping)
        {
            platform = hit.transform;
            lastPlatformPosition = platform.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Floor"))
        {
            platform = null;
        }
    }
}
