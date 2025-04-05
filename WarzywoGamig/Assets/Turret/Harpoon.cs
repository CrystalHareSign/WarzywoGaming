using UnityEngine;

public class Harpoon : MonoBehaviour
{
    private Rigidbody harpoonRb;
    private HarpoonController harpoonController;
    private TurretCollector turretCollector; // Dodaj referencję do TurretCollector
    public Transform treasureMountPoint; // Nowy punkt montażu dla Treasure
    public float treasureLifetime; // Czas, po którym Treasure zostaje zniszczone

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

        turretCollector = Object.FindFirstObjectByType<TurretCollector>();
        if (turretCollector == null)
        {
            Debug.LogError("Nie znaleziono TurretCollector w scenie.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!harpoonRb.isKinematic)
        {
            // Zatrzymaj ruch harpunu
            harpoonRb.linearVelocity = Vector3.zero;
            harpoonRb.angularVelocity = Vector3.zero;
        }

        harpoonRb.isKinematic = true;

        // Przekaż dane do HarpoonController
        if (harpoonController != null)
        {
            harpoonController.OnHarpoonCollision();
        }

        // Jeśli harpoon zderzył się z obiektem z tagiem "Treasure"
        if (collision.gameObject.CompareTag("Treasure"))
        {
            // Zapisz pozycję i rotację oryginalnego obiektu
            Vector3 originalPosition = collision.transform.position;
            Quaternion originalRotation = collision.transform.rotation;

            // Zapisz oryginalny obiekt
            GameObject originalObject = collision.gameObject;

            // Utwórz kopię obiektu bez skryptów
            GameObject treasureCopy = Instantiate(originalObject, originalPosition, originalRotation);

            // Usuń wszystkie skrypty poza TreasureResources
            foreach (var script in treasureCopy.GetComponents<MonoBehaviour>())
            {
                if (!(script is TreasureResources))
                {
                    Destroy(script);
                }
            }

            // Ustaw obiekt jako kinematyczny, wyłącz collider i zresetuj jego skalę
            Rigidbody treasureRb = treasureCopy.GetComponent<Rigidbody>();
            if (treasureRb != null)
            {
                treasureRb.isKinematic = true;
            }
            Collider treasureCollider = treasureCopy.GetComponent<Collider>();
            if (treasureCollider != null)
            {
                treasureCollider.enabled = false;
            }
            treasureCopy.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Przypisz kopię jako dziecko treasureMountPoint
            treasureCopy.transform.SetParent(treasureMountPoint);
            treasureCopy.transform.localPosition = Vector3.zero; // Ustaw lokalną pozycję na (0,0,0)

            // Przekaż zasoby do TurretCollector
            TreasureResources treasureResources = treasureCopy.GetComponent<TreasureResources>();
            if (treasureResources != null && turretCollector != null)
            {
                turretCollector.CollectResource(treasureResources);
            }

            // Zniszcz oryginalny obiekt
            Destroy(originalObject);

            // Zniszcz kopię po treasureLifetime
            Destroy(treasureCopy, treasureLifetime);
        }
    }
}