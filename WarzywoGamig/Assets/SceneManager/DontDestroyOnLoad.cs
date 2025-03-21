using UnityEngine;

public class DontDestroyContainer : MonoBehaviour
{
    public GameObject[] objectsToPreserve;

    void Awake()
    {
        foreach (var obj in objectsToPreserve)
        {
            DontDestroyOnLoad(obj);
        }
    }
}
