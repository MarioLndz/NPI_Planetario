using UnityEngine;

public class RotateSelf : MonoBehaviour
{
    [Header("Rotación continua")]
    public float rotationSpeedY = 15f;  // velocidad de rotación en Y

    [Header("Oscilación suave")]
    public float tiltAmplitudeX = 5f;   // amplitud del balanceo (grados)
    public float tiltAmplitudeZ = 3f;
    public float tiltSpeed = 1f;        // velocidad del balanceo

    private Quaternion baseRotation;

    void Start()
    {
        baseRotation = transform.rotation; // guarda la rotación inicial
    }

    void Update()
    {
        // Rotación continua sobre el eje Y
        transform.Rotate(Vector3.up, rotationSpeedY * Time.deltaTime, Space.World);

        // Cálculo de las oscilaciones suaves (seno y coseno)
        float tiltX = Mathf.Sin(Time.time * tiltSpeed) * tiltAmplitudeX;
        float tiltZ = Mathf.Cos(Time.time * tiltSpeed * 0.8f) * tiltAmplitudeZ;

        // Aplica la inclinación sobre la rotación actual
        transform.rotation = transform.rotation * Quaternion.Euler(tiltX * Time.deltaTime, 0f, tiltZ * Time.deltaTime);
    }
}
