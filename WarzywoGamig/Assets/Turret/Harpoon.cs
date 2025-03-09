using UnityEngine;

public class Harpoon : MonoBehaviour
{
    private Rigidbody harpoonRb;
    private HarpoonController harpoonController;
    private Light harpoonLight;

    private void Start()
    {
        harpoonRb = GetComponent<Rigidbody>();
        if (harpoonRb == null)
        {
            Debug.LogError("Nie znaleziono komponentu Rigidbody.");
        }

        harpoonController = Object.FindFirstObjectByType<HarpoonController>();
        if (harpoonController == null)
        {
            Debug.LogError("Nie znaleziono HarpoonController w scenie.");
        }

        // Dodaj œwiat³o do harpunu
        harpoonLight = gameObject.AddComponent<Light>();
        harpoonLight.color = Color.white;
        harpoonLight.intensity = 5f;
        harpoonLight.range = 10f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Harpoon zderzy³ siê z: " + collision.gameObject.name);

        // Zatrzymaj ruch harpunu
        harpoonRb.linearVelocity = Vector3.zero;
        harpoonRb.angularVelocity = Vector3.zero;
        harpoonRb.isKinematic = true;

        // Przeka¿ dane do HarpoonController
        if (harpoonController != null)
        {
            harpoonController.OnHarpoonCollision();
        }
    }
}