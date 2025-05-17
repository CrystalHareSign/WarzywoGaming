using UnityEngine;
using System.Collections.Generic;

public class CleanupDontDestroyOnLoad : MonoBehaviour
{
    // Obiekty ze sceny, kt�re maj� zosta� (przeci�gasz w Inspectorze)
    public List<GameObject> allowedSceneObjects;
    // Obiekty DDOL, kt�re maj� zosta� (przeci�gasz w Inspectorze)
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
                // je�li nie jest na li�cie allowedDDOLObjects � zniszcz!
                if (!allowedDDOLObjects.Contains(go))
                {
                    Destroy(go);
                }
            }
        }

        // (Opcjonalnie: je�li chcesz, mo�esz tu doda� logik� dla allowedSceneObjects,
        // ale one i tak nie s� w DDOL, wi�c nie b�d� niszczone przez powy�szy kod)
    }
}