using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Referencje")]
    public PlayerStats playerStats;

    [Header("Stamina Bar (LEWA STRONA)")]
    public Image staminaBonusBarL; // Przypisz: ¿ó³ty pasek (lewa po³owa)
    public Image staminaBaseBarL;  // Przypisz: zielony pasek (lewa po³owa)

    [Header("Stamina Bar (PRAWA STRONA)")]
    public Image staminaBonusBarR; // Przypisz: ¿ó³ty pasek (prawa po³owa)
    public Image staminaBaseBarR;  // Przypisz: zielony pasek (prawa po³owa)

    [Header("Damage Overlay")]
    public Image damageOverlay;

    [Tooltip("Progi HP w procentach od 1.0 (100%) do 0.0 (0%)")]
    public float[] hpThresholdPercents = new float[5] { 1.0f, 0.8f, 0.6f, 0.4f, 0.2f };

    [Tooltip("Alfa dla overlaya na ka¿dym progu, np. 0.0, 0.1, 0.2, 0.35, 0.5")]
    public float[] overlayAlphas = new float[5] { 0f, 0.1f, 0.2f, 0.35f, 0.5f };

    // Flaga do globalnego ukrywania pasków staminy
    [HideInInspector]
    public bool staminaBarsVisible = true;

    // RectTransformy s¹ automatycznie pobierane!
    private RectTransform staminaBaseRectL, staminaBonusRectL, staminaBaseRectR, staminaBonusRectR;

    private void Awake()
    {
        if (staminaBaseBarL) staminaBaseRectL = staminaBaseBarL.GetComponent<RectTransform>();
        if (staminaBonusBarL) staminaBonusRectL = staminaBonusBarL.GetComponent<RectTransform>();
        if (staminaBaseBarR) staminaBaseRectR = staminaBaseBarR.GetComponent<RectTransform>();
        if (staminaBonusBarR) staminaBonusRectR = staminaBonusBarR.GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (playerStats == null) playerStats = PlayerStats.Instance;
        if (staminaBonusBarL != null) staminaBonusBarL.gameObject.SetActive(false);
        if (staminaBonusBarR != null) staminaBonusBarR.gameObject.SetActive(false);
    }

    private void Update()
    {
        // --- Globalna kontrola widocznoœci pasków (zielone + ¿ó³te) ---
        if (!staminaBarsVisible)
        {
            if (staminaBaseBarL) staminaBaseBarL.gameObject.SetActive(false);
            if (staminaBonusBarL) staminaBonusBarL.gameObject.SetActive(false);
            if (staminaBaseBarR) staminaBaseBarR.gameObject.SetActive(false);
            if (staminaBonusBarR) staminaBonusBarR.gameObject.SetActive(false);
            return;
        }

        if (playerStats == null) return;

        float max = Mathf.Max(1f, playerStats.maxStamina);
        float bonus = Mathf.Max(0f, playerStats.staminaBonus);
        float current = Mathf.Clamp(playerStats.currentStamina, 0f, max + bonus);

        // ----------- LEWA PO£ÓWKA (od œrodka w lewo) -----------
        if (staminaBaseBarL != null && staminaBaseRectL != null)
        {
            staminaBaseBarL.gameObject.SetActive(true);
            staminaBaseBarL.fillAmount = Mathf.Clamp01(Mathf.Min(current, max) / max);
        }

        if (staminaBonusBarL != null && staminaBonusRectL != null && staminaBaseRectL != null)
        {
            if (bonus > 0f)
            {
                staminaBonusBarL.gameObject.SetActive(true);

                float baseWidth = staminaBaseRectL.rect.width;
                float bonusWidth = (max > 0f) ? baseWidth * (bonus / max) : 0f;

                staminaBonusRectL.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bonusWidth);
                staminaBonusRectL.anchoredPosition = new Vector2(staminaBaseRectL.anchoredPosition.x + baseWidth, staminaBaseRectL.anchoredPosition.y);

                float bonusCurrent = Mathf.Clamp(current - max, 0f, bonus);
                staminaBonusBarL.fillAmount = (bonus > 0f) ? (bonusCurrent / bonus) : 0f;
            }
            else
            {
                staminaBonusBarL.gameObject.SetActive(false);
            }
        }

        // ----------- PRAWA PO£ÓWKA (od œrodka w prawo, lustrzanie) -----------
        if (staminaBaseBarR != null && staminaBaseRectR != null)
        {
            staminaBaseBarR.gameObject.SetActive(true);
            staminaBaseBarR.fillAmount = Mathf.Clamp01(Mathf.Min(current, max) / max);
        }

        if (staminaBonusBarR != null && staminaBonusRectR != null && staminaBaseRectR != null)
        {
            if (bonus > 0f)
            {
                staminaBonusBarR.gameObject.SetActive(true);

                float baseWidthR = staminaBaseRectR.rect.width;
                float bonusWidthR = (max > 0f) ? baseWidthR * (bonus / max) : 0f;

                staminaBonusRectR.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bonusWidthR);
                // Lustrzane odbicie: przesuwamy na lewo
                staminaBonusRectR.anchoredPosition = new Vector2(staminaBaseRectR.anchoredPosition.x - baseWidthR, staminaBaseRectR.anchoredPosition.y);

                float bonusCurrentR = Mathf.Clamp(current - max, 0f, bonus);
                staminaBonusBarR.fillAmount = (bonus > 0f) ? (bonusCurrentR / bonus) : 0f;
            }
            else
            {
                staminaBonusBarR.gameObject.SetActive(false);
            }
        }

        // ----------- DAMAGE OVERLAY -----------
        if (damageOverlay != null && hpThresholdPercents != null && overlayAlphas != null)
        {
            float hpPercent = playerStats.currentHealth / Mathf.Max(1f, playerStats.maxHealth);
            float alpha = 0f;

            // Zak³adamy, ¿e progi s¹ malej¹co: 1.0, 0.8, 0.6, 0.4, 0.2
            // overlayAlphas: roœnie wraz z obra¿eniami (0, 0.1, ..., 0.5)
            for (int i = 0; i < hpThresholdPercents.Length && i < overlayAlphas.Length; i++)
            {
                if (hpPercent <= hpThresholdPercents[i])
                {
                    alpha = overlayAlphas[i];
                }
            }
            // Jeœli HP < najni¿szy próg, alpha = ostatni overlayAlpha
            if (hpPercent <= hpThresholdPercents[hpThresholdPercents.Length - 1] && overlayAlphas.Length > 0)
            {
                alpha = overlayAlphas[overlayAlphas.Length - 1];
            }
            Color col = damageOverlay.color;
            damageOverlay.color = new Color(col.r, col.g, col.b, alpha);
        }
    }
}