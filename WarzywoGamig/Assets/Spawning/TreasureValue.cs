using UnityEngine;

public class TreasureValue : MonoBehaviour
{
    public string category;
    public float amount;

    void Awake()
    {
        // U¿ycie DontDestroyOnLoad na tym obiekcie, aby nie zosta³ zniszczony po zmianie sceny
        DontDestroyOnLoad(gameObject);
    }
}
