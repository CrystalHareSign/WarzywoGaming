using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;
    public Slider progressBar;

    private void Awake()
    {
        Instance = this;
    }

    public void SetProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;
    }
}