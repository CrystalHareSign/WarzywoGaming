using UnityEngine;

public class DontDestroyContainer : MonoBehaviour
{
    public GameObject[] objectsToPreserve;
    public string[] tagsToCheck; // Lista tag�w, kt�re b�d� sprawdzane

    void Awake()
    {
        foreach (var obj in objectsToPreserve)
        {
            bool shouldCheck = false;

            foreach (var tag in tagsToCheck)
            {
                if (obj.CompareTag(tag))
                {
                    shouldCheck = true;
                    GameObject[] duplicates = GameObject.FindGameObjectsWithTag(tag);

                    if (duplicates.Length > 1)
                    {
                        Destroy(obj);
                        break; // Wyjd� z p�tli po zniszczeniu
                    }
                }
            }

            if (!shouldCheck || obj != null) // Je�li nie sprawdzali�my lub obiekt nie zosta� zniszczony
            {
                DontDestroyOnLoad(obj);
            }
        }
    }
}
