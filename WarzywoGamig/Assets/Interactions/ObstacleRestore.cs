using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public int damageAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Triggered by: " + other.gameObject.name);

        var interactableItem = other.GetComponent<InteractableItem>();
        if (interactableItem != null && interactableItem.usesHealthSystem)
        {
            interactableItem.TakeDamage(damageAmount);
            //Debug.Log($"Dealt {damageAmount} damage to {interactableItem.gameObject.name}");
        }
        else
        {
            //Debug.Log($"Obstacle collided with non-health-system object: {other.gameObject.name}");
        }
    }
}
