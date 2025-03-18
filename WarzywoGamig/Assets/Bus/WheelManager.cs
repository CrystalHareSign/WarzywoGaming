using UnityEngine;

public class WheelManager : MonoBehaviour
{
    [Header("Prefaby - rotacja jazdy")]
    // Cztery transformy dla kó³
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform backLeftWheel;
    public Transform backRightWheel;
    // Sta³a prêdkoœæ rotacji dla wszystkich kó³
    public float rotationSpeed = 200f;

    [Header("Obiekty - rotacja skrêtu")]
    // Transformy dla obiektów, które bêd¹ odpowiedzialne za rotacjê kó³ w osi Y
    public Transform frontLeftRotationObject;
    public Transform frontRightRotationObject;

    // K¹t maksymalnego skrêtu
    public float maxSteeringAngle = 30f;

    private float currentSteeringAngle = 0f;
    private bool isTurning = false;
    private float steeringTime = 1.0f;  // Bêdzie ustawiane w AssignInteraction
    private Vector3 targetDirection;

    void Update()
    {
        // Rotacja wszystkich kó³ (dotyczy tylko rotacji jazdy, nie skrêtu)
        RotateWheel(frontLeftWheel, rotationSpeed);
        RotateWheel(frontRightWheel, rotationSpeed);
        RotateWheel(backLeftWheel, rotationSpeed);
        RotateWheel(backRightWheel, rotationSpeed);

        // P³ynne skrêcanie przednich kó³
        if (isTurning)
        {
            SteerWheels();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Symulacja wywo³ania skrêtu, gdy naciœniesz spacjê
            StartSteering(Vector3.left, 1.0f);
        }
    }

    // Funkcja do rotacji kó³ z uwzglêdnieniem odbicia lustrzanego dla prawych kó³
    private void RotateWheel(Transform wheel, float speed)
    {
        // Jeœli to prawe ko³o, odbij kierunek rotacji
        if (wheel == frontRightWheel || wheel == backRightWheel)
        {
            wheel.Rotate(Vector3.back, -speed * Time.deltaTime);  // Obrót w przeciwn¹ stronê
        }
        else
        {
            wheel.Rotate(Vector3.back, speed * Time.deltaTime);  // Standardowy obrót
        }
    }

    // Funkcja odpowiedzialna za skrêcanie tylko przednich kó³
    private void SteerWheels()
    {
        // Sprawdzenie, czy skrêt jest aktywowany
        if (currentSteeringAngle == 0)
        {
            Debug.Log("Current Steering Angle is 0, no steering applied.");
        }

        // P³ynne przejœcie do docelowego k¹ta rotacji w obiektach
        Debug.Log("Steering wheels, current angle: " + frontLeftRotationObject.localRotation.eulerAngles.y);
        frontLeftRotationObject.localRotation = Quaternion.RotateTowards(
            frontLeftRotationObject.localRotation,
            Quaternion.Euler(0f, currentSteeringAngle, 0f),
            Mathf.Abs(currentSteeringAngle - frontLeftRotationObject.localRotation.eulerAngles.y) * Time.deltaTime / steeringTime
        );
        Debug.Log("Front Left Wheel Target Angle: " + currentSteeringAngle);

        frontRightRotationObject.localRotation = Quaternion.RotateTowards(
            frontRightRotationObject.localRotation,
            Quaternion.Euler(0f, -currentSteeringAngle, 0f),
            Mathf.Abs(currentSteeringAngle - frontRightRotationObject.localRotation.eulerAngles.y) * Time.deltaTime / steeringTime
        );
        Debug.Log("Front Right Wheel Target Angle: " + -currentSteeringAngle);

        // Zakoñczenie skrêtu, jeœli k¹t jest wystarczaj¹co bliski
        if (Mathf.Abs(frontLeftRotationObject.localRotation.eulerAngles.y - currentSteeringAngle) < 1f)
        {
            isTurning = false;
            Debug.Log("Steering completed. Current angle reached.");
        }
    }

    // Funkcja wywo³ywana przez inne skrypty, aby zacz¹æ skrêt
    public void StartSteering(Vector3 direction, float moveDuration)
    {
        steeringTime = moveDuration;  // Ustawienie czasu skrêtu równym czasowi ruchu
        targetDirection = direction;
        currentSteeringAngle = Mathf.Sign(direction.x) * maxSteeringAngle; // Okreœlenie k¹ta skrêtu w zale¿noœci od kierunku
        isTurning = true;

        Debug.Log("StartSteering called, direction: " + direction + ", angle: " + currentSteeringAngle);
    }
}
