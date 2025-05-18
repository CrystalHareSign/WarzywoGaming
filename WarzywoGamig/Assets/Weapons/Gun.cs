using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Nowa flaga do sprawdzania, czy broń jest aktywna
    private bool isWeaponEquipped = false;

    private InventoryUI inventoryUI; // Odwołanie do InventoryUI

    [Header("Full Auto Settings")]
    public bool isFullAuto = false; // Dodanie zmiennej, która kontroluje tryb full auto
    public float fireRate = 0.1f; // Czas między kolejnymi strzałami w trybie full auto

    private float nextFireTime = 0f; // Zmienna do kontrolowania tempa strzelania w trybie full auto

    void Start()
    {
        if (currentAmmo == 0)
            currentAmmo = maxAmmo;

        inventoryUI = Object.FindFirstObjectByType<InventoryUI>(); // Pobranie referencji do InventoryUI
    }

    void Update()
    {
        if (!isWeaponEquipped || isReloading) return;

        if (isFullAuto)
        {
            // Strzelanie w trybie full auto
            if (Input.GetButton("Fire1") && (currentAmmo > 0 || unlimitedAmmo) && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate; // Opóźnienie między strzałami
                Shoot();
            }
        }
        else
        {
            // Strzelanie w trybie semi-auto (kliknięcie raz)
            if (Input.GetButtonDown("Fire1") && (currentAmmo > 0 || unlimitedAmmo))
            {
                Shoot();
            }
        }

        // Obsługa przeładowania
        if (Input.GetKeyDown(KeyCode.R) && !unlimitedAmmo && currentAmmo < maxAmmo && totalAmmo > 0)
        {
            StartCoroutine(Reload());
        }
        else if (currentAmmo <= 0 && !unlimitedAmmo)  // Sprawdzenie, czy trzeba rozpocząć przeładowanie
        {
            StartCoroutine(Reload());
        }
    }

    public void EquipWeapon()
    {
        // Po wywołaniu tej metody broń staje się aktywna
        isWeaponEquipped = true;
    }

    void Shoot()
    {
        if (!unlimitedAmmo)
        {
            currentAmmo--;
        }

        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);
        //Debug.Log("Ammo: " + currentAmmo);

        if (inventoryUI != null)
        {
            inventoryUI.UpdateWeaponUI(this); // Powiadomienie UI o zmianie stanu broni
        }

        if (currentAmmo <= 0 && !unlimitedAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;

        // Uaktualnienie UI w momencie rozpoczęcia przeładowania
        if (inventoryUI != null)
        {
            inventoryUI.UpdateWeaponUI(this); // Odświeżenie UI przy rozpoczęciu przeładowania
        }

        yield return new WaitForSeconds(reloadTime);

        int bulletsToReload = Mathf.Min(maxAmmo - currentAmmo, totalAmmo);
        currentAmmo += bulletsToReload;
        totalAmmo -= bulletsToReload;

        isReloading = false;

        // Uaktualnienie UI po zakończeniu przeładowania
        if (inventoryUI != null)
        {
            inventoryUI.UpdateWeaponUI(this); // Odświeżenie UI po zakończeniu przeładowania
        }

        Debug.Log("Reloaded!");
    }

    // Nowa metoda do sprawdzania, czy broń jest w trakcie przeładowania
    public bool IsReloading()
    {
        return isReloading;
    }
}
