using UnityEngine;

public class WheelManager : MonoBehaviour
{
    [Header("Prefaby - rotacja jazdy")]
    // Cztery transformy dla k�
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform backLeftWheel;
    public Transform backRightWheel;
    // Sta�a pr�dko�� rotacji dla wszystkich k�
    public float rotationSpeed = 200f;

    [Header("Obiekty - rotacja skr�tu")]
    // Transformy dla obiekt�w, kt�re b�d� odpowiedzialne za rotacj� k� w osi Y
    public Transform frontLeftRotationObject;
    public Transform frontRightRotationObject;

    // K�t maksymalnego skr�tu
    public float maxSteeringAngle = 30f;

    private float currentSteeringAngle = 0f;
    private bool isTurning = false;
    private float steeringTime = 1.0f;  // B�dzie ustawiane w AssignInteraction
    private Vector3 targetDirection;

    void Update()
    {
        // Rotacja wszystkich k� (dotyczy tylko rotacji jazdy, nie skr�tu)
        RotateWheel(frontLeftWheel, rotationSpeed);
        RotateWheel(frontRightWheel, rotationSpeed);
        RotateWheel(backLeftWheel, rotationSpeed);
        RotateWheel(backRightWheel, rotationSpeed);

        // P�ynne skr�canie przednich k�
        if (isTurning)
        {
            SteerWheels();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Symulacja wywo�ania skr�tu, gdy naci�niesz spacj�
            StartSteering(Vector3.left, 1.0f);
        }
    }

    // Funkcja do rotacji k� z uwzgl�dnieniem odbicia lustrzanego dla prawych k�
    private void RotateWheel(Transform wheel, float speed)
    {
        // Je�li to prawe ko�o, odbij kierunek rotacji
        if (wheel == frontRightWheel || wheel == backRightWheel)
        {
            wheel.Rotate(Vector3.back, -speed * Time.deltaTime);  // Obr�t w przeciwn� stron�
        }
        else
        {
            wheel.Rotate(Vector3.back, speed * Time.deltaTime);  // Standardowy obr�t
        }
    }

    // Funkcja odpowiedzialna za skr�canie tylko przednich k�
    private void SteerWheels()
    {
        // Sprawdzenie, czy skr�t jest aktywowany
        if (currentSteeringAngle == 0)
        {
            Debug.Log("Current Steering Angle is 0, no steering applied.");
        }

        // P�ynne przej�cie do docelowego k�ta rotacji w obiektach
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

        // Zako�czenie skr�tu, je�li k�t jest wystarczaj�co bliski
        if (Mathf.Abs(frontLeftRotationObject.localRotation.eulerAngles.y - currentSteeringAngle) < 1f)
        {
            isTurning = false;
            Debug.Log("Steering completed. Current angle reached.");
        }
    }

    // Funkcja wywo�ywana przez inne skrypty, aby zacz�� skr�t
    public void StartSteering(Vector3 direction, float moveDuration)
    {
        steeringTime = moveDuration;  // Ustawienie czasu skr�tu r�wnym czasowi ruchu
        targetDirection = direction;
        currentSteeringAngle = Mathf.Sign(direction.x) * maxSteeringAngle; // Okre�lenie k�ta skr�tu w zale�no�ci od kierunku
        isTurning = true;

        Debug.Log("StartSteering called, direction: " + direction + ", angle: " + currentSteeringAngle);
    }
}
