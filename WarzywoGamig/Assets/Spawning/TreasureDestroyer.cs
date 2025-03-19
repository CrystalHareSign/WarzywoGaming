using UnityEngine;

public class TreasureDestroyer : MonoBehaviour
{
    // Funkcja wywo³ywana, gdy inny obiekt wchodzi w kolizjê z tym obiektem
    private void OnCollisionEnter(Collision collision)
    {
        // SprawdŸ, czy obiekt, z którym kolidujemy, ma tag "Wheel"
        if (collision.gameObject.CompareTag("Wheel"))
        {
            // Zniszcz obiekt, na którym znajduje siê skrypt
            Destroy(gameObject);
            //Debug.Log("Obiekt zosta³ zniszczony, poniewa¿ wszed³ w kolizjê z obiektem o tagu 'Wheel'");
        }
    }
}
