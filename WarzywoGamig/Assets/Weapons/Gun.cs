using System.Collections;
using UnityEngine;
using TMPro; // ✅ Import TextMeshPro

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public int maxAmmo = 10;
    public int currentAmmo;
    public int totalAmmo = 30;
    public float reloadTime = 2f;
    public int damage = 20;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform shootingPoint;

    private bool isReloading = false;

    [Header("Ammo Settings")]
    public bool unlimitedAmmo = false;  // ✅ Teraz możesz włączać/wyłączać w `Inspectorze`

    [Header("UI Settings")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI totalAmmoText;
    public TextMeshProUGUI reloadingText;

    void Start()
    {
        currentAmmo = maxAmmo;
        reloadingText.gameObject.SetActive(false);
        UpdateAmmoUI();
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetButtonDown("Fire1") && (currentAmmo > 0 || unlimitedAmmo))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && !unlimitedAmmo && currentAmmo < maxAmmo && totalAmmo > 0)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        if (!unlimitedAmmo)
        {
            currentAmmo--;
        }

        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);
        Debug.Log("Ammo: " + currentAmmo);

        UpdateAmmoUI();

        if (currentAmmo <= 0 && !unlimitedAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        reloadingText.gameObject.SetActive(true);

        yield return new WaitForSeconds(reloadTime);

        int bulletsToReload = Mathf.Min(maxAmmo - currentAmmo, totalAmmo);
        currentAmmo += bulletsToReload;
        totalAmmo -= bulletsToReload;

        isReloading = false;
        reloadingText.gameObject.SetActive(false);

        UpdateAmmoUI();
        Debug.Log("Reloaded!");
    }

    void UpdateAmmoUI()
    {
        ammoText.text = currentAmmo.ToString();
        totalAmmoText.text = unlimitedAmmo ? "∞" : totalAmmo.ToString();
    }
}
