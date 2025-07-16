using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 5f; // public, by InventoryUI mógł spowalniać
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float backwardSpeed = 3f; // <--- NOWE POLE: prędkość do tyłu/po skosie
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Sound Settings")]
    [SerializeField] private string[] indoorWalkingSounds;
    [SerializeField] private string[] outdoorWalkingSounds;
    [SerializeField] private float walkSoundDelay = 0.5f;
    [SerializeField] private float sprintSoundDelay = 0.3f;

    private CharacterController controller;
    private Vector3 velocity;
    public bool isGrounded;
    private Transform platform = null;
    private Vector3 lastPlatformPosition;
    private bool isJumping = false;
    private bool sprintingWhileAirborne = false;

    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();
    private float lastSoundTime = 0f;

    private AudioChanger audioChanger;
    private PlayerStats playerStats;

    // Nowe pola do obsługi staminaExhausted
    private bool shiftHeldLastFrame = false;

    // POLA DLA SLOWDOWN PODCZAS UŻYWANIA ITEMU
    [HideInInspector] public bool isSlowedByItemUse = false;

    // Flaga blokująca sprint przez UI (np. podczas używania itema)
    [HideInInspector] public bool isSprintBlockedByUI = false;

    public bool isSprinting = false; // Dodatkowa flaga, jeśli chcesz obsłużyć stop sprinting z UI

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));
        audioChanger = Object.FindAnyObjectByType<AudioChanger>();
        playerStats = PlayerStats.Instance;
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
            sprintingWhileAirborne = false;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        if (move.magnitude > 1f)
            move = move.normalized;

        // Kierunek względem transform.forward
        float moveForwardDot = Vector3.Dot(move, transform.forward);
        // Dot < 0 oznacza ruch z komponentem do tyłu (czyli także po skosie do tyłu)
        bool isMovingBackwardDiagonal = moveForwardDot < 0f && move.magnitude > 0.1f;

        bool isSprintKeyPressed = Input.GetKey(sprintKey);
        bool isTryingToMove = move.magnitude > 0.1f;

        // BLOKADA SPRINTU PRZEZ UI
        bool sprintBlocked = isSprintBlockedByUI;

        // BLOKADA SPRINTU JEŚLI GRACZ TRZYMA LOOT
        bool isHoldingLoot = false;
        if (Inventory.Instance != null && Inventory.Instance.lootParent != null)
        {
            isHoldingLoot = Inventory.Instance.lootParent.childCount > 0;
        }
        if (isHoldingLoot)
        {
            isSprintKeyPressed = false;
            sprintBlocked = true;
        }

        // BLOKADA SPRINTU PRZY RUCHU DO TYŁU/PO SKOSIE DO TYŁU
        if (isMovingBackwardDiagonal)
        {
            isSprintKeyPressed = false;
            sprintBlocked = true;
        }

        // Obsługa staminaExhausted: sprint dostępny tylko, gdy stamina nie jest wyczerpana I NIE jest zablokowany przez UI
        bool canSprint = playerStats != null && playerStats.currentStamina > 0f && !playerStats.staminaExhausted && !sprintBlocked;

        if (isGrounded)
        {
            sprintingWhileAirborne = isSprintKeyPressed && isTryingToMove && canSprint;
        }

        isSprinting = ((isGrounded && isSprintKeyPressed && isTryingToMove && canSprint) ||
                       (!isGrounded && sprintingWhileAirborne && canSprint));

        float currentSpeed = isMovingBackwardDiagonal ? backwardSpeed : (isSprinting ? sprintSpeed : moveSpeed);
        float currentSoundDelay = isSprinting ? sprintSoundDelay : walkSoundDelay;

        if (isSprinting)
        {
            playerStats.UseStamina(playerStats.staminaUsage * Time.deltaTime);
        }

        // Jeśli stamina się skończyła, ustaw staminaExhausted
        if (playerStats != null && playerStats.currentStamina <= 0f)
        {
            playerStats.staminaExhausted = true;
            isSprinting = false;
            currentSpeed = moveSpeed;
            currentSoundDelay = walkSoundDelay;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (move.magnitude > 0f && isGrounded && Time.time - lastSoundTime >= currentSoundDelay)
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

        // Odblokowanie sprintu po zregenerowaniu staminy:
        if (playerStats != null && playerStats.staminaExhausted)
        {
            // stamina się odnowiła i shift NIE jest wciśnięty
            if (!isSprintKeyPressed && playerStats.currentStamina > 0.1f)
            {
                playerStats.staminaExhausted = false;
            }
        }

        shiftHeldLastFrame = isSprintKeyPressed;
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
            if (audioChanger.isPlayerInside)
            {
                PlayRandomSounds(indoorWalkingSounds);
            }
            else
            {
                PlayRandomSounds(outdoorWalkingSounds);
            }
        }
    }

    private void PlayRandomSounds(string[] sounds)
    {
        if (sounds.Length == 0) return;

        int randomIndex = Random.Range(0, sounds.Length);
        string randomSound = sounds[randomIndex];

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;
            playSoundOnObject.PlaySound(randomSound, 0.5f, false);
        }
    }

    // Dodaj tę funkcję, by InventoryUI mogło przerwać sprint
    public void StopSprinting()
    {
        isSprinting = false;
        // Dodatkowe działania jeśli masz inną logikę sprintu
    }
}