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
    private Coroutine reloadCoroutine = null; // <--- referencja do coroutine przeładowania

    [Header("Ammo Settings")]
    public bool unlimitedAmmo = false;

    private bool isWeaponEquipped = false;
    private InventoryUI inventoryUI;

    [Header("Full Auto Settings")]
    public bool isFullAuto = false;
    public float fireRate = 0.1f;

    private float nextFireTime = 0f;

    void Start()
    {

        inventoryUI = Object.FindFirstObjectByType<InventoryUI>();
    }

    void Update()
    {
        if (!isWeaponEquipped || isReloading) return;

        if (isFullAuto)
        {
            if (Input.GetButton("Fire1") && (currentAmmo > 0 || unlimitedAmmo) && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1") && (currentAmmo > 0 || unlimitedAmmo))
            {
                Shoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !unlimitedAmmo && currentAmmo < maxAmmo && totalAmmo > 0)
        {
            StartReload();
        }
        else if (currentAmmo <= 0 && !unlimitedAmmo)
        {
            StartReload();
        }
    }

    public void EquipWeapon()
    {
        isWeaponEquipped = true;
    }

    void Shoot()
    {
        if (!unlimitedAmmo)
        {
            currentAmmo--;
        }

        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);

        if (inventoryUI != null)
        {
            inventoryUI.UpdateWeaponUI(this);
        }

        if (currentAmmo <= 0 && !unlimitedAmmo)
        {
            StartReload();
        }
    }

    public void StartReload()
    {
        if (isReloading) return;
        if (!isWeaponEquipped) return; // <-- kluczowa linia, nie przeładowuj jeśli broń nie jest trzymana

        // Jeśli coroutine już działa, nie uruchamiaj kolejnej
        reloadCoroutine = StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;

        if (inventoryUI != null)
        {
            inventoryUI.UpdateWeaponUI(this);
        }

        yield return new WaitForSeconds(reloadTime);

        // Jeśli w trakcie reloadu broń została schowana, przerywamy!
        if (!isWeaponEquipped)
        {
            isReloading = false;
            reloadCoroutine = null;
            yield break;
        }

        int bulletsToReload = Mathf.Min(maxAmmo - currentAmmo, totalAmmo);
        currentAmmo += bulletsToReload;
        totalAmmo -= bulletsToReload;

        isReloading = false;

        if (inventoryUI != null)
        {
            inventoryUI.UpdateWeaponUI(this);
        }

        reloadCoroutine = null;
        Debug.Log("Reloaded!");
    }

    // Anulowanie przeładowania (np. przy chowaniu broni)
    public void CancelReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        isReloading = false;

        if (inventoryUI != null) // schowaj info o przeładowaniu
        {
            inventoryUI.UpdateWeaponUI(this);
        }
    }

    void OnDisable()
    {
        CancelReload(); // automatycznie kasuje przeładowanie przy Deactivate
        isWeaponEquipped = false;
    }

    public bool IsReloading()
    {
        return isReloading;
    }
}