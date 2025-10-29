using UnityEngine;

public class PlanetFocus : MonoBehaviour {
    [Header("Planetas del sistema solar (en orden)")]
    public Transform[] planets;   // Asignar en el Inspector

    [Header("Referencia de cámara o punto de vista")]
    public Transform cameraTransform; // Main Camera

    [Header("Parámetros de movimiento")]
    public float moveSpeed = 2f;      // velocidad de transición
    public float rotateSpeed = 2f;    // velocidad de rotación

    private int currentIndex = 0;
    private Vector3 targetPosition;

    void Start() {
        if (planets.Length == 0) {
            Debug.LogError("⚠️ No se han asignado planetas en PlanetFocus.");
            return;
        }
        if (cameraTransform == null) {
            cameraTransform = Camera.main.transform;
        }

        // Posición inicial
        targetPosition = planets[currentIndex].position;
        MoveInstant();
    }

    void Update() {
        // Movimiento suave de cámara hacia el planeta objetivo
        if (cameraTransform != null) {
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                targetPosition,
                Time.deltaTime * moveSpeed
            );

            // La cámara mira al planeta actual
            Vector3 lookDir = (planets[currentIndex].position - cameraTransform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            cameraTransform.rotation = Quaternion.Slerp(
                cameraTransform.rotation,
                targetRot,
                Time.deltaTime * rotateSpeed
            );
        }
    }

    public void FocusNextPlanet() {
        if (planets.Length == 0) return;
        currentIndex = (currentIndex + 1) % planets.Length;
        targetPosition = planets[currentIndex].position;
        Debug.Log("➡️ Foco en " + planets[currentIndex].name);
    }

    public void FocusPreviousPlanet() {
        if (planets.Length == 0) return;
        currentIndex = (currentIndex - 1 + planets.Length) % planets.Length;
        targetPosition = planets[currentIndex].position;
        Debug.Log("⬅️ Foco en " + planets[currentIndex].name);
    }

    private void MoveInstant() {
        cameraTransform.position = planets[currentIndex].position;
        cameraTransform.LookAt(planets[currentIndex]);
    }
}

