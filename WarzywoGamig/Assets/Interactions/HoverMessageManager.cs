using TMPro;
using UnityEngine;
using System.Collections;

public class HoverMessageManager : MonoBehaviour
{
    public TMP_Text messageText; // Tekst wyœwietlany po najechaniu kursorem
    public TMP_Text keyText; // Tekst z przyciskiem (np. "E")
    public TMP_Text messageTextInfo; // Tekst popup do informacji (fadeout)

    private Camera mainCamera;
    public static HoverMessageManager Instance;

    public LayerMask interactableLayer; // Warstwa interaktywnych przedmiotów

    private Coroutine infoFadeCoroutine = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Ukryj tekst na pocz¹tku
        if (messageText != null) messageText.gameObject.SetActive(false);
        if (keyText != null) keyText.gameObject.SetActive(false);
        if (messageTextInfo != null)
        {
            messageTextInfo.gameObject.SetActive(false);
            SetTextAlpha(messageTextInfo, 1f);
        }
        mainCamera = Camera.main;
    }

    void Update()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f, interactableLayer))
        {
            HoverMessage hoverMessage = hit.collider.GetComponent<HoverMessage>();

            // Debug - trafienie w collider
            Debug.Log($"[Prompt] hit: {hit.collider.name}, hoverMsg: {hoverMessage}, interactDist: {(hoverMessage ? hoverMessage.interactionDistance : -1)}, hitDist: {hit.distance}");

            if (hoverMessage != null && hit.distance <= hoverMessage.interactionDistance &&
                !hoverMessage.isInteracted &&
                InteractivityManager.Instance.IsInteractable(hit.collider.gameObject))
            {
                // Debug - warunki promptu spe³nione
                Debug.Log($"[Prompt] SHOW: {hit.collider.name}, hitDist: {hit.distance}, activeSelf: {messageText?.gameObject.activeSelf}, alpha: {messageText?.color.a}");

                // Pokazuj prompt...
                if (messageText != null && keyText != null)
                {
                    messageText.text = hoverMessage.message;
                    keyText.text = hoverMessage.keyText;

                    messageText.fontSize = hoverMessage.messageFontSize;
                    keyText.fontSize = hoverMessage.keyFontSize;

                    messageText.gameObject.SetActive(true);
                    keyText.gameObject.SetActive(true);

                    // Debug - po aktywowaniu
                    Debug.Log($"[Prompt] SetActive(true) called. messageText.activeSelf: {messageText.gameObject.activeSelf}, keyText.activeSelf: {keyText.gameObject.activeSelf}");
                }
            }
            else
            {
                // Debug - warunki promptu NIE spe³nione
                Debug.Log($"[Prompt] HIDE: {hit.collider.name}");

                if (messageText != null && keyText != null)
                {
                    messageText.gameObject.SetActive(false);
                    keyText.gameObject.SetActive(false);

                    // Debug - po dezaktywowaniu
                    Debug.Log($"[Prompt] SetActive(false) called. messageText.activeSelf: {messageText.gameObject.activeSelf}, keyText.activeSelf: {keyText.gameObject.activeSelf}");
                }
            }
        }
        else
        {
            // Debug - raycast nie trafi³ w nic interaktywnego
            Debug.Log("[Prompt] Raycast miss, hiding prompt.");

            if (messageText != null && keyText != null)
            {
                messageText.gameObject.SetActive(false);
                keyText.gameObject.SetActive(false);

                // Debug - po dezaktywowaniu (brak trafienia)
                Debug.Log($"[Prompt] SetActive(false) (miss). messageText.activeSelf: {messageText.gameObject.activeSelf}, keyText.activeSelf: {keyText.gameObject.activeSelf}");
            }
        }
    }

    /// <summary>
    /// Wywo³aj popup z tekstem i opcjonalnym czasem trwania (domyœlnie 3 sekundy).
    /// </summary>
    public void ShowInfoPopup(string text, float duration = 3f)
    {
        if (messageTextInfo == null) return;

        if (infoFadeCoroutine != null)
        {
            StopCoroutine(infoFadeCoroutine);
        }

        messageTextInfo.text = text;
        messageTextInfo.alpha = 1f;
        messageTextInfo.gameObject.SetActive(true);

        // Debug - pokazanie popupu
        Debug.Log($"[Popup] ShowInfoPopup: {text}");

        infoFadeCoroutine = StartCoroutine(FadeOutInfo(duration));
    }
    public void ShowInfoPopup(string text, int fontSize, float duration = 3f)
    {
        if (messageTextInfo == null) return;

        if (infoFadeCoroutine != null)
        {
            StopCoroutine(infoFadeCoroutine);
        }

        messageTextInfo.fontSize = fontSize;
        messageTextInfo.text = text;
        messageTextInfo.alpha = 1f;
        messageTextInfo.gameObject.SetActive(true);

        // Debug - pokazanie popupu z rozmiarem
        Debug.Log($"[Popup] ShowInfoPopup: {text} (fontSize: {fontSize})");

        infoFadeCoroutine = StartCoroutine(FadeOutInfo(duration));
    }

    private IEnumerator FadeOutInfo(float duration)
    {
        yield return new WaitForSeconds(duration);

        float fadeTime = 1.0f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            SetTextAlpha(messageTextInfo, alpha);

            // Debug - fadeout alpha
            Debug.Log($"[Popup] FadeOut alpha: {alpha}");

            yield return null;
        }

        messageTextInfo.gameObject.SetActive(false);
        SetTextAlpha(messageTextInfo, 1f); // reset alpha na przysz³oœæ
        infoFadeCoroutine = null;

        // Debug - koniec popupu
        Debug.Log("[Popup] FadeOutInfo complete, popup hidden.");
    }

    private void SetTextAlpha(TMP_Text text, float alpha)
    {
        if (text == null) return;
        Color c = text.color;
        c.a = alpha;
        text.color = c;

        // Debug - ustawienie alpha
        Debug.Log($"[Prompt] SetTextAlpha: {text.name}, alpha: {alpha}");
    }
}