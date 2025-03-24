using UnityEngine;

public class DontDestroyContainer : MonoBehaviour
{
    public GameObject[] objectsToPreserve;
    public string[] tagsToCheck; // Lista tagów, które bêd¹ sprawdzane

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
                        break; // WyjdŸ z pêtli po zniszczeniu
                    }
                }
            }

            if (!shouldCheck || obj != null) // Jeœli nie sprawdzaliœmy lub obiekt nie zosta³ zniszczony
            {
                DontDestroyOnLoad(obj);
            }
        }
    }
}
