using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public Transform doorPivot;
    public Transform handleA;
    public Transform handleB;
    public GameObject doorLeaf;

    public float openAngleA = 90f;
    public float openAngleB = -90f;
    public float openSpeed = 360f;

    private float targetAngle = 0f;
    private bool isMoving = false;
    private float currentAngle = 0f;

    void Start()
    {
        currentAngle = 0f;
        doorPivot.localRotation = Quaternion.Euler(0, currentAngle, 0);
        //Debug.Log($"[START] Drzwi ustawione na k¹t: {currentAngle}");
    }

    void Update()
    {
        if (!doorPivot) return;

        if (isMoving)
        {
            // Jeœli gracz blokuje drzwi, zatrzymaj animacjê, pozwól na wznowienie przez interakcjê
            if (IsPlayerBlockingDoor())
            {
                //Debug.Log("[Drzwi] Gracz blokuje drzwi! Animacja zatrzymana.");
                isMoving = false; // zatrzymaj animacjê!
                return;
            }

            float nextAngle = Mathf.MoveTowards(currentAngle, targetAngle, openSpeed * Time.deltaTime);
            doorPivot.localRotation = Quaternion.Euler(0, nextAngle, 0);
            currentAngle = nextAngle;

            if (Mathf.Abs(currentAngle - targetAngle) < 0.01f)
            {
                doorPivot.localRotation = Quaternion.Euler(0, targetAngle, 0);
                currentAngle = targetAngle;
                isMoving = false;
                //Debug.Log($"[KONIEC RUCHU] K¹t koñcowy: {currentAngle}");
            }
        }
    }

    // Sprawdza czy gracz blokuje drzwi (przy ka¿dym kroku ruchu)
    bool IsPlayerBlockingDoor()
    {
        if (doorLeaf == null) return false;
        Collider doorCollider = doorLeaf.GetComponent<Collider>();
        if (doorCollider == null) return false;

        Collider[] hits = Physics.OverlapBox(
            doorCollider.bounds.center,
            doorCollider.bounds.extents,
            doorLeaf.transform.rotation
        );
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
                return true;
        }
        return false;
    }

    // Sprawdza czy drzwi s¹ zamkniête (k¹t ~0 lub ~360)
    bool IsDoorClosed()
    {
        float angle = doorPivot.localRotation.eulerAngles.y;
        return Mathf.Abs(angle) < 1f || Mathf.Abs(angle - 360f) < 1f;
    }

    // Sprawdza czy drzwi s¹ otwarte w prawo (k¹t ~openAngleA)
    bool IsDoorOpenRight()
    {
        float angle = doorPivot.localRotation.eulerAngles.y;
        return Mathf.Abs(Mathf.DeltaAngle(angle, openAngleA)) < 1f;
    }

    // Sprawdza czy drzwi s¹ otwarte w lewo (k¹t ~openAngleB)
    bool IsDoorOpenLeft()
    {
        float angle = doorPivot.localRotation.eulerAngles.y;
        return Mathf.Abs(Mathf.DeltaAngle(angle, openAngleB)) < 1f;
    }

    public void OpenDoorA()
    {
        if (isMoving)
        {
            //Debug.Log("[OpenDoorA] Ruch w trakcie.");
            return;
        }
        if (IsDoorOpenRight())
        {
            //Debug.Log("[OpenDoorA] Drzwi ju¿ otwarte w prawo.");
            return;
        }
        if (IsDoorOpenLeft())
        {
            //Debug.Log("[OpenDoorA] Najpierw zamknij drzwi zanim otworzysz w prawo!");
            return;
        }
        targetAngle = openAngleA;
        isMoving = true;
        //Debug.Log($"[OpenDoorA] Rozpoczynam otwieranie w prawo: z {currentAngle} do {targetAngle}");
    }

    public void OpenDoorB()
    {
        if (isMoving)
        {
            //Debug.Log("[OpenDoorB] Ruch w trakcie.");
            return;
        }
        if (IsDoorOpenLeft())
        {
            //Debug.Log("[OpenDoorB] Drzwi ju¿ otwarte w lewo.");
            return;
        }
        if (IsDoorOpenRight())
        {
            //Debug.Log("[OpenDoorB] Najpierw zamknij drzwi zanim otworzysz w lewo!");
            return;
        }
        targetAngle = openAngleB;
        isMoving = true;
        //Debug.Log($"[OpenDoorB] Rozpoczynam otwieranie w lewo: z {currentAngle} do {targetAngle}");
    }

    public void CloseDoor()
    {
        if (isMoving)
        {
            //Debug.Log("[CloseDoor] Ruch w trakcie.");
            return;
        }
        if (IsDoorClosed())
        {
            //Debug.Log("[CloseDoor] Drzwi ju¿ zamkniête.");
            return;
        }
        targetAngle = 0f;
        isMoving = true;
        //Debug.Log($"[CloseDoor] Rozpoczynam zamykanie: z {currentAngle} do 0");
    }
}