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
        //Debug.Log("Harpoon zderzy� si� z: " + collision.gameObject.name);

        // Sprawd�, czy harpoonRb nie jest kinetyczny, zanim ustawisz pr�dko��
        if (!harpoonRb.isKinematic)
        {
            // Zatrzymaj ruch harpunu
            harpoonRb.linearVelocity = Vector3.zero;
            harpoonRb.angularVelocity = Vector3.zero;
        }

        harpoonRb.isKinematic = true;

        // Przeka� dane do HarpoonController
        if (harpoonController != null)
        {
            harpoonController.OnHarpoonCollision();
        }

        //// Je�li harpoon zderzy� si� z obiektem z tagiem "Treasure", nadaj mu dodatkow� pr�dko��
        //if (collision.gameObject.CompareTag("Treasure"))
        //{
        //    harpoonRb.isKinematic = false;
        //    harpoonRb.velocity = transform.forward * spawnedObjectSpeed;
        //}
    }
}