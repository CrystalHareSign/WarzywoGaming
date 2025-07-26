using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    [Tooltip("Œwiat³o do kontrolowania (jeœli nie ustawione, pobierze Light z tego obiektu)")]
    public Light lightSource;

    [Tooltip("Czy œwiat³o powinno migaæ?")]
    [SerializeField] private bool flicker = false;

    [Tooltip("Czy œwiat³o jest w³¹czone (globalnie)?")]
    [SerializeField] private bool enabledState = true;

    [Tooltip("Minimalna jasnoœæ œwiat³a (przy efektach zmêczenia)")]
    [SerializeField] private float minIntensity = 0.03f;
    [Tooltip("Bazowa jasnoœæ œwiat³a")]
    [SerializeField] private float baseIntensity = 0.8f;
    [Tooltip("Losowa szybkoœæ zmian jasnoœci")]
    [SerializeField] private float flickerSpeed = 1.0f;

    private float flickerTimer = 0f;
    private float tiredTimer = 0f;
    private float blackoutTimer = 0f; // na zanik œwiat³a

    void Awake()
    {
        if (lightSource == null)
            lightSource = GetComponent<Light>();
    }

    public void SetFlicker(bool value)
    {
        flicker = value;
        if (!flicker && lightSource != null)
            lightSource.enabled = enabledState;
    }

    public void SetEnabled(bool value)
    {
        enabledState = value;
        if (lightSource != null)
            lightSource.enabled = value;
    }

    void Update()
    {
        if (lightSource == null)
            return;

        if (!enabledState)
        {
            lightSource.enabled = false;
            return;
        }

        if (flicker)
        {
            // Efekt totalnego zaniku œwiat³a (co jakiœ czas robi siê zupe³nie ciemno)
            if (blackoutTimer > 0)
            {
                lightSource.enabled = true;
                lightSource.intensity = 0f;
                blackoutTimer -= Time.deltaTime;
                return;
            }

            tiredTimer += Time.deltaTime * flickerSpeed * Random.Range(0.7f, 1.3f);

            // P³ynna nieregularnoœæ (szum Perlin)
            float slowFlicker = Mathf.PerlinNoise(tiredTimer, 0.0f);
            float intensity = Mathf.Lerp(minIntensity, baseIntensity, slowFlicker);

            // Losowe migniêcia i zaniki
            flickerTimer -= Time.deltaTime;
            if (flickerTimer <= 0f)
            {
                float chance = Random.value;
                if (chance > 0.82f)
                {
                    // Krótki blackout
                    blackoutTimer = Random.Range(0.08f, 0.25f);
                    intensity = 0f;
                }
                else if (chance > 0.55f)
                {
                    // Mocny b³ysk
                    intensity += Random.Range(0.5f, 1.2f);
                }
                else if (chance > 0.35f)
                {
                    // Szybkie przygaszenie
                    intensity -= Random.Range(0.1f, 0.3f);
                }
                flickerTimer = Random.Range(0.13f, 0.37f);
            }

            // Drobne szumy
            intensity += Mathf.Sin(Time.time * Random.Range(8f, 18f)) * 0.09f;

            // Ustaw
            lightSource.enabled = true;
            lightSource.intensity = Mathf.Clamp(intensity, 0f, baseIntensity + 1.2f);
        }
        else
        {
            // Tryb normalny: œwiat³o po prostu œwieci
            lightSource.enabled = enabledState;
            lightSource.intensity = baseIntensity;
        }
    }

    public static void EnableAllLights()
    {
        foreach (var light in UnityEngine.Object.FindObjectsByType<FlickerLight>(FindObjectsSortMode.None))
            light.SetEnabled(true);
    }

    public static void DisableAllLights()
    {
        foreach (var light in UnityEngine.Object.FindObjectsByType<FlickerLight>(FindObjectsSortMode.None))
            light.SetEnabled(false);
    }
}