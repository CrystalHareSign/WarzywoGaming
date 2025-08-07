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

    // Stan: zamkniête, otwarte w prawo, otwarte w lewo
    private enum DoorState { Closed, OpenRight, OpenLeft }
    private DoorState state = DoorState.Closed;

    void Start()
    {
        currentAngle = 0f;
        doorPivot.localRotation = Quaternion.Euler(0, currentAngle, 0);
        state = DoorState.Closed;
        Debug.Log($"[START] Drzwi ustawione na k¹t: {currentAngle}");
    }

    void Update()
    {
        if (!doorPivot) return;

        if (isMoving)
        {
            float nextAngle = Mathf.MoveTowards(currentAngle, targetAngle, openSpeed * Time.deltaTime);
            doorPivot.localRotation = Quaternion.Euler(0, nextAngle, 0);
            currentAngle = nextAngle;

            if (Mathf.Abs(currentAngle - targetAngle) < 0.01f)
            {
                if (targetAngle == 0f)
                {
                    doorPivot.localRotation = Quaternion.Euler(0, 0, 0);
                    currentAngle = 0f;
                    state = DoorState.Closed;
                    Debug.Log($"[ZAMKNIÊTE] K¹t koñcowy: {currentAngle} (powinno byæ 0)");
                }
                else if (targetAngle == openAngleA)
                {
                    doorPivot.localRotation = Quaternion.Euler(0, openAngleA, 0);
                    currentAngle = openAngleA;
                    state = DoorState.OpenRight;
                    Debug.Log($"[OTWARTE PRAWO] K¹t koñcowy: {currentAngle} (powinno byæ 90)");
                }
                else if (targetAngle == openAngleB)
                {
                    doorPivot.localRotation = Quaternion.Euler(0, openAngleB, 0);
                    currentAngle = openAngleB;
                    state = DoorState.OpenLeft;
                    Debug.Log($"[OTWARTE LEWO] K¹t koñcowy: {currentAngle} (powinno byæ -90)");
                }
                isMoving = false;
            }
        }
    }

    public void OpenDoorA()
    {
        if (isMoving)
        {
            Debug.Log("[OpenDoorA] Ruch w trakcie.");
            return;
        }
        if (state == DoorState.OpenRight)
        {
            Debug.Log("[OpenDoorA] Drzwi ju¿ otwarte w prawo.");
            return;
        }
        if (state == DoorState.OpenLeft)
        {
            Debug.Log("[OpenDoorA] Najpierw zamknij drzwi zanim otworzysz w prawo!");
            return;
        }
        targetAngle = openAngleA;
        isMoving = true;
        Debug.Log($"[OpenDoorA] Rozpoczynam otwieranie w prawo: z {currentAngle} do {targetAngle}");
    }

    public void OpenDoorB()
    {
        if (isMoving)
        {
            Debug.Log("[OpenDoorB] Ruch w trakcie.");
            return;
        }
        if (state == DoorState.OpenLeft)
        {
            Debug.Log("[OpenDoorB] Drzwi ju¿ otwarte w lewo.");
            return;
        }
        if (state == DoorState.OpenRight)
        {
            Debug.Log("[OpenDoorB] Najpierw zamknij drzwi zanim otworzysz w lewo!");
            return;
        }
        targetAngle = openAngleB;
        isMoving = true;
        Debug.Log($"[OpenDoorB] Rozpoczynam otwieranie w lewo: z {currentAngle} do {targetAngle}");
    }

    public void CloseDoor()
    {
        if (isMoving)
        {
            Debug.Log("[CloseDoor] Ruch w trakcie.");
            return;
        }
        if (state == DoorState.Closed)
        {
            Debug.Log("[CloseDoor] Drzwi ju¿ zamkniête.");
            return;
        }
        targetAngle = 0f;
        isMoving = true;
        Debug.Log($"[CloseDoor] Rozpoczynam zamykanie: z {currentAngle} do 0");
    }
}