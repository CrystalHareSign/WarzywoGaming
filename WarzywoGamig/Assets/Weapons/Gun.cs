using System.Collections; // To jest wymagane dla IEnumerator
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public int maxAmmo = 10;  // Maksymalna iloœæ pocisków w magazynku
    public int currentAmmo;  // Aktualna iloœæ pocisków
    public float reloadTime = 2f;  // Czas prze³adowania
    public int damage = 20;  // Obra¿enia zadawane przez pocisk

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;  // Prefab pocisku
    public Transform shootingPoint;  // Punkt, z którego wychodz¹ pociski

    private bool isReloading = false;  // Czy pistolet jest w trakcie prze³adowania

    [Header("UI Settings")]
    public Text reloadingText;  // Odwo³anie do napisu "Reloading..."

    void Start()
    {
        currentAmmo = maxAmmo;  // Na pocz¹tku pistolet ma pe³ny magazynek
        reloadingText.gameObject.SetActive(false);  // Ukryj napis na pocz¹tku
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetButtonDown("Fire1") && currentAmmo > 0)  // Sprawdzanie przycisku strza³u (zwykle lewy przycisk myszy)
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)  // Prze³adowanie (przycisk R)
        {
            StartCoroutine(Reload());
        }
    }

    // Funkcja do strzelania
    void Shoot()
    {
        currentAmmo--;  // Zmniejszamy iloœæ pocisków w magazynku

        // Tworzenie pocisku
        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);

        Debug.Log("Ammo: " + currentAmmo);

        // Sprawdzenie, czy trzeba prze³adowaæ
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    // Funkcja prze³adowania
    IEnumerator Reload()
    {
        isReloading = true;
        reloadingText.gameObject.SetActive(true);  // Pokazujemy napis "Reloading..."

        // Czekanie na zakoñczenie prze³adowania
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;  // Nape³niamy magazynek
        isReloading = false;
        reloadingText.gameObject.SetActive(false);  // Ukrywamy napis "Reloading..."
        Debug.Log("Reloaded!");
    }
}