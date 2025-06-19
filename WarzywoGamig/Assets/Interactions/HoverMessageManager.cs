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
                    keyText.text = hoverMessage.keyText;

                    messageText.fontSize = hoverMessage.messageFontSize;
                    keyText.fontSize = hoverMessage.keyFontSize;

                    messageText.gameObject.SetActive(true);
                    keyText.gameObject.SetActive(true);
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
        else
        {
            if (messageText != null && keyText != null)
            {
                messageText.gameObject.SetActive(false);
                keyText.gameObject.SetActive(false);
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
            yield return null;
        }

        messageTextInfo.gameObject.SetActive(false);
        SetTextAlpha(messageTextInfo, 1f); // reset alpha na przysz³oœæ
        infoFadeCoroutine = null;
    }

    private void SetTextAlpha(TMP_Text text, float alpha)
    {
        if (text == null) return;
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }
}