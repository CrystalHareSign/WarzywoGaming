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
    private Coroutine reloadCoroutine = null;

    [Header("Ammo Settings")]
    public bool unlimitedAmmo = false;

    private bool isWeaponEquipped = false;
    private InventoryUI inventoryUI;

    [Header("Full Auto Settings")]
    public bool isFullAuto = false;
    public float fireRate = 0.1f;

    private float nextFireTime = 0f;

    public AmmoType ammoType;

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
        else if (currentAmmo <= 0 && !unlimitedAmmo && totalAmmo > 0)
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

        // Wystrzeliwuje pocisk w miejsce kursora (środek ekranu lub pod myszą)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(1000f);
        }

        Vector3 direction = (targetPoint - shootingPoint.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        Instantiate(bulletPrefab, shootingPoint.position, lookRotation);

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
        if (!isWeaponEquipped) return;

        if (currentAmmo >= maxAmmo) return; // Magazynek pełny
        if (totalAmmo <= 0) return;         // Brak amunicji do przeładowania

        // Blokada przeładowania podczas używania itema (np. trzymania F)
        if (InventoryUI.Instance != null && InventoryUI.Instance.isHoldingUse)
            return;

        // PRZERWIJ SPRINT NA GRACZU jeśli trwa
        var player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player != null)
            player.StopSprinting();

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

        if (!isWeaponEquipped)
        {
            isReloading = false;
            reloadCoroutine = null;
            yield break;
        }

        int bulletsToReload = Mathf.Min(maxAmmo - currentAmmo, totalAmmo);
        currentAmmo += bulletsToReload;
        totalAmmo -= bulletsToReload;

        // Synchronizuj globalny magazyn po przeładowaniu!
        if (Inventory.Instance != null)
            Inventory.Instance.totalAmmoDict[ammoType] = totalAmmo;

        isReloading = false;

        if (inventoryUI != null)
        {
            inventoryUI.UpdateWeaponUI(this);
        }

        reloadCoroutine = null;
        Debug.Log("Reloaded!");
    }

    public void CancelReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        isReloading = false;

        if (inventoryUI != null)
        {
            inventoryUI.UpdateWeaponUI(this);
        }
    }

    void OnDisable()
    {
        CancelReload();
        isWeaponEquipped = false;
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public void AddAmmo(int amount)
    {
        totalAmmo += amount;

        // Synchronizuj globalny magazyn po dodaniu amunicji!
        if (Inventory.Instance != null)
            Inventory.Instance.totalAmmoDict[ammoType] = totalAmmo;

        Debug.Log($"[Gun] Dodano amunicję: {amount}, nowa ilość (totalAmmo): {this.totalAmmo}");
        if (inventoryUI != null)
        {
            inventoryUI.UpdateWeaponUI(this);
        }
    }
}