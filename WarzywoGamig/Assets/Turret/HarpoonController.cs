﻿using UnityEngine;

public class HarpoonController : MonoBehaviour
{
    public GameObject harpoonPrefab; // Prefab Harpoon
    public Transform firePoint; // Punkt, do którego przypisywany będzie Harpoon
    public float harpoonSpeed = 50f; // Prędkość wystrzeliwanego harpunu

    private TurretController turretController; // Referencja do skryptu TurretController
    private GameObject harpoonInstance; // Instancja harpunu
    private Rigidbody harpoonRb; // Rigidbody harpunu
    private Camera playerCamera; // Kamera gracza

    void Start()
    {
        if (harpoonPrefab == null)
        {
            Debug.LogError("Nie przypisano prefabrykatu Harpoon.");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("Nie przypisano FirePoint.");
            return;
        }

        // Znajdź obiekt z TurretController w scenie
        turretController = Object.FindFirstObjectByType<TurretController>();
        if (turretController == null)
        {
            Debug.LogError("Nie znaleziono TurretController w scenie.");
            return;
        }

        playerCamera = Camera.main;
        SpawnHarpoon();
    }

    void Update()
    {
        if (turretController.isUsingTurret && turretController.isRaised && Input.GetMouseButtonDown(0)) // LPM
        {
            ShootHarpoon();
        }
    }

    void SpawnHarpoon()
    {
        harpoonInstance = Instantiate(harpoonPrefab, firePoint.position, firePoint.rotation, firePoint);
        harpoonInstance.transform.localScale = Vector3.one; // Ustawienie skali na (1, 1, 1)

        harpoonRb = harpoonInstance.GetComponent<Rigidbody>();
        if (harpoonRb == null)
        {
            Debug.LogError("Prefabrykat Harpoon nie ma komponentu Rigidbody.");
        }
        harpoonInstance.SetActive(true); // Dezaktywuj harpun na starcie
    }

    void ShootHarpoon()
    {
        if (harpoonInstance == null || harpoonRb == null)
        {
            Debug.LogError("Harpun nie został poprawnie zainicjalizowany.");
            return;
        }

        harpoonRb.isKinematic = false; // Upewnij się, że harpun nie jest kinematic

        harpoonInstance.transform.position = firePoint.position;
        harpoonInstance.transform.rotation = firePoint.rotation;
        harpoonInstance.transform.SetParent(null); // Odłącz harpun od FirePoint
        harpoonInstance.SetActive(true); // Aktywuj harpun

        Vector3 shootDirection = GetShootDirection();
        harpoonRb.linearVelocity = shootDirection * harpoonSpeed;
    }

    Vector3 GetShootDirection()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return (hit.point - firePoint.position).normalized;
        }
        else
        {
            return ray.direction;
        }
    }

    public void OnHarpoonCollision()
    {
        // Funkcja wywołana po kolizji harpunu
        ReturnHarpoonToFirePoint();
    }

    void ReturnHarpoonToFirePoint()
    {
        if (harpoonInstance == null)
        {
            Debug.LogError("Harpun nie został poprawnie zainicjalizowany.");
            return;
        }

        harpoonInstance.transform.position = firePoint.position;
        harpoonInstance.transform.rotation = firePoint.rotation;
        harpoonInstance.transform.SetParent(firePoint); // Podłącz harpun do FirePoint
        harpoonInstance.SetActive(false); // Dezaktywuj harpun
    }
}