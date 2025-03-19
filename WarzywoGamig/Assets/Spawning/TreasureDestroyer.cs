using UnityEngine;

public class TreasureDestroyer : MonoBehaviour
{
    // Funkcja wywo�ywana, gdy inny obiekt wchodzi w kolizj� z tym obiektem
    private void OnCollisionEnter(Collision collision)
    {
        // Sprawd�, czy obiekt, z kt�rym kolidujemy, ma tag "Wheel"
        if (collision.gameObject.CompareTag("Wheel"))
        {
            // Zniszcz obiekt, na kt�rym znajduje si� skrypt
            Destroy(gameObject);
            //Debug.Log("Obiekt zosta� zniszczony, poniewa� wszed� w kolizj� z obiektem o tagu 'Wheel'");
        }
    }
}
