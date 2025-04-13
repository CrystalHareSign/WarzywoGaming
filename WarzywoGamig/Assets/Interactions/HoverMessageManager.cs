using UnityEngine;
using TMPro;

public class HoverMessageManager : MonoBehaviour
{
    public TMP_Text messageText; // Tekst, który bêdzie wyœwietlany po najechaniu kursorem
    public TMP_Text keyText; // Tekst z przyciskiem (np. "E")
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

        if (keyText != null)
        {
            keyText.gameObject.SetActive(false); // Upewnij siê, ¿e jest zakomentowane!
        }

        mainCamera = Camera.main;
    }


    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            HoverMessage hoverMessage = hit.transform.GetComponent<HoverMessage>();
            if (hoverMessage != null && hit.distance <= hoverMessage.interactionDistance && !hoverMessage.isInteracted && InteractivityManager.Instance.IsInteractable(hit.transform.gameObject))
            {
                if (messageText != null && keyText != null)
                {
                    messageText.text = hoverMessage.message;
                    keyText.text = hoverMessage.keyText; // Nie zapomnij u¿yæ hoverMessage.keyText

                    messageText.fontSize = hoverMessage.fontSize;
                    keyText.fontSize = hoverMessage.fontSize;

                    messageText.gameObject.SetActive(true);
                    keyText.gameObject.SetActive(true); // W³¹czamy keyText, jak messageText
                }
            }
            else
            {
                // Ukrywamy oba teksty
                if (messageText != null && keyText != null)
                {
                    messageText.gameObject.SetActive(false);
                    keyText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (messageText != null && keyText != null)
            {
                messageText.gameObject.SetActive(false);
                keyText.gameObject.SetActive(false);
            }
        }
    }

}