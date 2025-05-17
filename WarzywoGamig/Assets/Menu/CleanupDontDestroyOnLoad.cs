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
        // Dodaj SaveManagera do listy, je�li jeszcze go nie ma
        if (SaveManager.Instance != null && !allowedDDOLObjects.Contains(SaveManager.Instance.gameObject))
            allowedDDOLObjects.Add(SaveManager.Instance.gameObject);

        // Dodaj AudioManagera, je�li istnieje singleton
        if (AudioManager.Instance != null && !allowedDDOLObjects.Contains(AudioManager.Instance.gameObject))
            allowedDDOLObjects.Add(AudioManager.Instance.gameObject);

        // Dodaj LanguageManagera, je�li istnieje singleton
        if (LanguageManager.Instance != null && !allowedDDOLObjects.Contains(LanguageManager.Instance.gameObject))
            allowedDDOLObjects.Add(LanguageManager.Instance.gameObject);

        GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in all)
        {
            if (go.hideFlags == HideFlags.None &&
                go.transform.parent == null &&
                go.scene.name == "DontDestroyOnLoad")
            {
                if (!allowedDDOLObjects.Contains(go))
                {
                    Destroy(go);
                }
            }
        }
    }
}