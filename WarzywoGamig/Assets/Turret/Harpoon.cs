using UnityEngine;

public class Harpoon : MonoBehaviour
{
    private Rigidbody harpoonRb;
    private HarpoonController harpoonController;
    public float spawnedObjectSpeed;

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
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Harpoon zderzy³ siê z: " + collision.gameObject.name);

        // SprawdŸ, czy harpoonRb nie jest kinetyczny, zanim ustawisz prêdkoœæ
        if (!harpoonRb.isKinematic)
        {
            // Zatrzymaj ruch harpunu
            harpoonRb.velocity = Vector3.zero;
            harpoonRb.angularVelocity = Vector3.zero;
        }

        harpoonRb.isKinematic = true;

        // Przeka¿ dane do HarpoonController
        if (harpoonController != null)
        {
            harpoonController.OnHarpoonCollision();
        }

        //// Jeœli harpoon zderzy³ siê z obiektem z tagiem "Treasure", nadaj mu dodatkow¹ prêdkoœæ
        //if (collision.gameObject.CompareTag("Treasure"))
        //{
        //    harpoonRb.isKinematic = false;
        //    harpoonRb.velocity = transform.forward * spawnedObjectSpeed;
        //}
    }
}