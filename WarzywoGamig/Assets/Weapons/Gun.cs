using System.Collections;
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

    void Start()
    {
        currentAmmo = maxAmmo;
        inventoryUI = Object.FindFirstObjectByType<InventoryUI>(); // Pobranie referencji do InventoryUI
    }

    void Update()
    {
        if (!isWeaponEquipped || isReloading) return;

        if (Input.GetButtonDown("Fire1") && (currentAmmo > 0 || unlimitedAmmo))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && !unlimitedAmmo && currentAmmo < maxAmmo && totalAmmo > 0)
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
        Debug.Log("Ammo: " + currentAmmo);

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

        yield return new WaitForSeconds(reloadTime);

        int bulletsToReload = Mathf.Min(maxAmmo - currentAmmo, totalAmmo);
        currentAmmo += bulletsToReload;
        totalAmmo -= bulletsToReload;

        isReloading = false;

        Debug.Log("Reloaded!");

        if (inventoryUI != null)
        {
            inventoryUI.UpdateWeaponUI(this); // Powiadomienie UI o zakończeniu przeładowania
        }
    }

    // Nowa metoda do sprawdzania, czy broń jest w trakcie przeładowania
    public bool IsReloading()
    {
        return isReloading;
    }
}
