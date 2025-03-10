using UnityEngine;

public class TreasureTracker : MonoBehaviour
{
    public Vector3 CurrentVelocity { get; private set; }
    public Vector3 CurrentDirection { get; private set; }

    private Rigidbody treasureRb;

    void Start()
    {
        treasureRb = GetComponent<Rigidbody>();
        if (treasureRb == null)
        {
            Debug.LogError("Treasure object does not have a Rigidbody component.");
        }
    }

    void Update()
    {
        if (treasureRb != null)
        {
            CurrentVelocity = treasureRb.velocity;
            CurrentDirection = CurrentVelocity.normalized;
        }
    }
}