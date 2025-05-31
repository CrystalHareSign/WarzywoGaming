using UnityEngine;
using System.Collections;

public class TerminalEmission : MonoBehaviour
{
    public Renderer targetRenderer;
    public int materialIndex = 0;
    public Color emissionColor;
    public float flashDuration = 0.2f;

    [Header("Flash Settings")]
    public Color flashColor = Color.white;
    [Range(1f, 10f)]
    public float flashIntensity = 3f;

    private Material _instanceMaterial;
    private Coroutine flashCoroutine;

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        var mats = targetRenderer.materials;
        if (materialIndex < 0 || materialIndex >= mats.Length)
        {
            Debug.LogWarning("Podany indeks materia³u jest poza zakresem!");
            return;
        }
        _instanceMaterial = mats[materialIndex];
        mats[materialIndex] = _instanceMaterial;
        targetRenderer.materials = mats;

        emissionColor = _instanceMaterial.GetColor("_EmissionColor");
        DisableEmission();
    }

    public void EnableEmission()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        _instanceMaterial.EnableKeyword("_EMISSION");
        _instanceMaterial.SetColor("_EmissionColor", emissionColor);
    }

    public void DisableEmission()
    {
        if (_instanceMaterial == null) return;
        _instanceMaterial.DisableKeyword("_EMISSION");
        _instanceMaterial.SetColor("_EmissionColor", Color.black);
    }

    public void FlashEmission()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashEmissionCoroutine());
    }

    private IEnumerator FlashEmissionCoroutine()
    {
        if (_instanceMaterial == null) yield break;

        _instanceMaterial.EnableKeyword("_EMISSION");

        Color startColor = flashColor * flashIntensity; // HDR
        Color endColor = emissionColor;

        float elapsed = 0f;

        _instanceMaterial.SetColor("_EmissionColor", startColor);

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flashDuration);
            Color lerpedColor = Color.Lerp(startColor, endColor, t);
            _instanceMaterial.SetColor("_EmissionColor", lerpedColor);
            yield return null;
        }

        _instanceMaterial.SetColor("_EmissionColor", endColor);
        flashCoroutine = null;
    }
}