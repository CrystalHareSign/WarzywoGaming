using UnityEngine;

public class PlayerMovement : MonoBehaviour
{   
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    public CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        // Sprawd�, czy gracz jest na ziemi
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Resetuj pr�dko�� spadania
        }

        // Pobierz wej�cia z klawiatury
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Oblicz wektor ruchu
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Przemie�� gracza
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Skok
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Zastosuj grawitacj�
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}