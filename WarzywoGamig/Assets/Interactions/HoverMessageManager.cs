using UnityEngine;
using TMPro;

public class HoverMessageManager : MonoBehaviour
{
    public TMP_Text messageText; // Tekst, który bêdzie wyœwietlany po najechaniu kursorem
    private Camera mainCamera;

    public static HoverMessageManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //Debug.Log("HoverMessageManager initialized.");
        }
        else
        {
            //Debug.LogWarning("Another instance of HoverMessageManager found. Destroying this instance.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Ukryj tekst na pocz¹tku
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
        // Pobierz g³ówn¹ kamerê
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Rzutowanie promienia z pozycji kursora
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Sprawdzenie, czy promieñ trafia w collider obiektu
        if (Physics.Raycast(ray, out hit))
        {
            HoverMessage hoverMessage = hit.transform.GetComponent<HoverMessage>();
            if (hoverMessage != null && hit.distance <= hoverMessage.interactionDistance && !hoverMessage.isInteracted && InteractivityManager.Instance.IsInteractable(hit.transform.gameObject))
            {
                // Wyœwietl komunikat po najechaniu kursorem na obiekt
                if (messageText != null)
                {
                    messageText.text = hoverMessage.message;
                    messageText.gameObject.SetActive(true);
                }
            }
            else
            {
                // Ukryj komunikat, jeœli kursor nie jest nad obiektem
                if (messageText != null)
                {
                    messageText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // Ukryj komunikat, jeœli kursor nie jest nad obiektem
            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
            }
        }
    }
}