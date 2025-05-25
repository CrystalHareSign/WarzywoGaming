using UnityEngine;
using UnityEngine.UI;
using TMPro; // Dodaj import TMPRO

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;
    public Slider progressBar;

    public TMP_Text continueText; // Zmieñ na TMP_Text

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;
    }

    public void ShowContinuePrompt(bool show)
    {
        if (continueText != null)
            continueText.gameObject.SetActive(show);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}