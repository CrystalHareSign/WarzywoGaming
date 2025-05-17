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
        // Dodaj SaveManagera do listy, jeœli jeszcze go nie ma
        if (SaveManager.Instance != null && !allowedDDOLObjects.Contains(SaveManager.Instance.gameObject))
            allowedDDOLObjects.Add(SaveManager.Instance.gameObject);

        // Dodaj AudioManagera, jeœli istnieje singleton
        if (AudioManager.Instance != null && !allowedDDOLObjects.Contains(AudioManager.Instance.gameObject))
            allowedDDOLObjects.Add(AudioManager.Instance.gameObject);

        // Dodaj LanguageManagera, jeœli istnieje singleton
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