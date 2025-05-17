using UnityEngine;
using System.Collections.Generic;

public class CleanupDontDestroyOnLoad : MonoBehaviour
{
    // Obiekty ze sceny, które maj¹ zostaæ (przeci¹gasz w Inspectorze)
    public List<GameObject> allowedSceneObjects;
    // Obiekty DDOL, które maj¹ zostaæ (przeci¹gasz w Inspectorze)
    public List<GameObject> allowedDDOLObjects;

    void Start()
    {
        GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in all)
        {
            // Sprawdzamy czy to rootowy obiekt z DDOL
            if (go.hideFlags == HideFlags.None &&
                go.transform.parent == null &&
                go.scene.name == "DontDestroyOnLoad")
            {
                // jeœli nie jest na liœcie allowedDDOLObjects – zniszcz!
                if (!allowedDDOLObjects.Contains(go))
                {
                    Destroy(go);
                }
            }
        }

        // (Opcjonalnie: jeœli chcesz, mo¿esz tu dodaæ logikê dla allowedSceneObjects,
        // ale one i tak nie s¹ w DDOL, wiêc nie bêd¹ niszczone przez powy¿szy kod)
    }
}