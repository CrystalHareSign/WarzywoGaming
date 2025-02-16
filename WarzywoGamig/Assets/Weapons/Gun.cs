using System.Collections; // To jest wymagane dla IEnumerator
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public int maxAmmo = 10;  // Maksymalna ilo�� pocisk�w w magazynku
    public int currentAmmo;  // Aktualna ilo�� pocisk�w
    public float reloadTime = 2f;  // Czas prze�adowania
    public int damage = 20;  // Obra�enia zadawane przez pocisk

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;  // Prefab pocisku
    public Transform shootingPoint;  // Punkt, z kt�rego wychodz� pociski

    private bool isReloading = false;  // Czy pistolet jest w trakcie prze�adowania

    [Header("UI Settings")]
    public Text reloadingText;  // Odwo�anie do napisu "Reloading..."

    void Start()
    {
        currentAmmo = maxAmmo;  // Na pocz�tku pistolet ma pe�ny magazynek
        reloadingText.gameObject.SetActive(false);  // Ukryj napis na pocz�tku
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetButtonDown("Fire1") && currentAmmo > 0)  // Sprawdzanie przycisku strza�u (zwykle lewy przycisk myszy)
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)  // Prze�adowanie (przycisk R)
        {
            StartCoroutine(Reload());
        }
    }

    // Funkcja do strzelania
    void Shoot()
    {
        currentAmmo--;  // Zmniejszamy ilo�� pocisk�w w magazynku

        // Tworzenie pocisku
        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);

        Debug.Log("Ammo: " + currentAmmo);

        // Sprawdzenie, czy trzeba prze�adowa�
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    // Funkcja prze�adowania
    IEnumerator Reload()
    {
        isReloading = true;
        reloadingText.gameObject.SetActive(true);  // Pokazujemy napis "Reloading..."

        // Czekanie na zako�czenie prze�adowania
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;  // Nape�niamy magazynek
        isReloading = false;
        reloadingText.gameObject.SetActive(false);  // Ukrywamy napis "Reloading..."
        Debug.Log("Reloaded!");
    }
}