using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Sound Settings")]
    [SerializeField] private string[] indoorWalkingSounds; // Dźwięki wewnątrz
    [SerializeField] private string[] outdoorWalkingSounds; // Dźwięki na zewnątrz
    [SerializeField] private float walkSoundDelay = 0.5f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Transform platform = null;
    private Vector3 lastPlatformPosition;
    private bool isJumping = false;

    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();
    private float lastSoundTime = 0f;

    private AudioChanger audioChanger; // Referencja do skryptu AudioChanger

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playSoundObjects.AddRange(Object.FindObjectsOfType<PlaySoundOnObject>());
        audioChanger = FindObjectOfType<AudioChanger>(); // Pobranie referencji do AudioChanger

        // Możesz zainicjować dźwięki wewnętrzne i zewnętrzne według potrzeby
    }

    private void Update()
    {
        MovePlayer();
    }

    private void LateUpdate()
    {
        if (platform != null && isGrounded)
        {
            Vector3 platformMovement = platform.position - lastPlatformPosition;
            controller.enabled = false;
            transform.position += platformMovement;
            controller.enabled = true;
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

        if (move.magnitude > 0f && isGrounded && Time.time - lastSoundTime >= walkSoundDelay)
        {
            PlayRandomWalkSounds();
            lastSoundTime = Time.time;
        }

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

    private void PlayRandomWalkSounds()
    {
        if (audioChanger != null)
        {
            if (audioChanger.isPlayerInside) // Debug: Sprawdzanie, czy gracz jest wewnątrz
            {
                Debug.Log("Gracz jest wewnątrz, odtwarzanie dźwięków wewnętrznych.");
                PlayRandomSounds(indoorWalkingSounds);
            }
            else
            {
                Debug.Log("Gracz jest na zewnątrz, odtwarzanie dźwięków zewnętrznych.");
                PlayRandomSounds(outdoorWalkingSounds);
            }
        }
    }

    private void PlayRandomSounds(string[] sounds)
    {
        if (sounds.Length == 0) return;

        int randomIndex = Random.Range(0, sounds.Length);
        string randomSound = sounds[randomIndex];
        Debug.Log("Odtwarzanie dźwięku: " + randomSound); // Debug: Sprawdzanie, który dźwięk jest odtwarzany

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound(randomSound, 1f, false);
        }
    }
}
