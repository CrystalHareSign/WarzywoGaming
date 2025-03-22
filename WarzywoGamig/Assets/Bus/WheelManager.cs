using UnityEngine;
using UnityEngine.SceneManagement;

public class WheelManager : MonoBehaviour
{
    [Header("Prefaby - rotacja jazdy")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform backLeftWheel;
    public Transform backRightWheel;
    public float rotationSpeed = 200f;

    [Header("Obiekty - rotacja skrêtu")]
    public Transform frontLeftRotationObject;
    public Transform frontRightRotationObject;

    public float maxSteeringAngle = 30f;

    private float steeringTime = 1.0f; // Czas ca³kowitego ruchu, pobierany z AssignInteraction
    private float halfSteeringTime;
    private bool isSteering = false;
    private float elapsedSteeringTime = 0f;
    private bool steeringLeft = true;

    void Update()
    {
        // Sprawdzanie, czy aktywna scena to "Main"
        if (SceneManager.GetActiveScene().name == "Main")
        {
            // Rotacja kó³
            RotateWheel(frontLeftWheel, rotationSpeed);
            RotateWheel(frontRightWheel, rotationSpeed);
            RotateWheel(backLeftWheel, rotationSpeed);
            RotateWheel(backRightWheel, rotationSpeed);
        }

        if (isSteering)
        {
            elapsedSteeringTime += Time.deltaTime;

            // Faza 1: Skrêt
            if (elapsedSteeringTime <= halfSteeringTime)
            {
                float t = elapsedSteeringTime / halfSteeringTime;
                ApplySteer(Mathf.Lerp(0f, maxSteeringAngle, t));
            }
            // Faza 2: Powrót
            else if (elapsedSteeringTime <= steeringTime)
            {
                float t = (elapsedSteeringTime - halfSteeringTime) / halfSteeringTime;
                ApplySteer(Mathf.Lerp(maxSteeringAngle, 0f, t));
            }
            // Koniec
            else
            {
                isSteering = false;
                ApplySteer(0f);
            }
        }
    }

    private void RotateWheel(Transform wheel, float speed)
    {
        if (wheel == frontRightWheel || wheel == backRightWheel)
        {
            wheel.Rotate(Vector3.back, -speed * Time.deltaTime);
        }
        else
        {
            wheel.Rotate(Vector3.back, speed * Time.deltaTime);
        }
    }

    private void ApplySteer(float angle)
    {
        if (steeringLeft)
        {
            // Skrêt w lewo
            frontLeftRotationObject.localRotation = Quaternion.Euler(0f, -angle, 0f);
            frontRightRotationObject.localRotation = Quaternion.Euler(0f, -angle, 0f);
        }
        else
        {
            // Skrêt w prawo
            frontLeftRotationObject.localRotation = Quaternion.Euler(0f, angle, 0f);
            frontRightRotationObject.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
    }


    // Funkcja wywo³ywana z AssignInteraction
    public void StartSteering(Vector3 direction, float moveDuration)
    {
        steeringTime = moveDuration;
        halfSteeringTime = steeringTime / 2f;
        elapsedSteeringTime = 0f;

        // Okreœlenie kierunku skrêtu
        steeringLeft = direction.z > 0; // forward = lewo, backward = prawo (mo¿esz to odwróciæ jeœli trzeba)
        isSteering = true;

        //Debug.Log($"[WheelManager] Steering started. Duration: {steeringTime}, Direction: {(steeringLeft ? "Left" : "Right")}");
    }
}
