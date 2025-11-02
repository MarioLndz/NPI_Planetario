using UnityEngine;
using UnityEngine.EventSystems;
using Leap;

public class LeapPlanetSelector : MonoBehaviour
{
    [Header("Referencias")]
    public LeapServiceProvider leapProvider;
    public Camera mainCamera;
    public RectTransform pointerUI;

    [Header("Pinch Detection (Strength)")]
    [Range(0f, 1f)] public float pinchOnThreshold = 0.9f;
    [Range(0f, 1f)] public float pinchOffThreshold = 0.7f;

    // --- NUEVA VARIABLE ---
    [Header("Click 'Forgiveness'")]
    [Tooltip("El 'grosor' del rayo en unidades de Unity. Un valor más alto es más fácil de clickar.")]
    public float clickRadius = 0.5f;
    // --- FIN NUEVA VARIABLE ---

    private bool isPinching = false;
    private bool eventSystemChecked = false;

    void Start()
    {
        // ... (Tu código de Start() sigue igual)
        if (leapProvider == null) Debug.LogError("Asigna el LeapServiceProvider en el Inspector.");
        if (mainCamera == null) mainCamera = Camera.main;
        if (EventSystem.current == null)
        {
            Debug.LogError("¡FALTA UN EVENTSYSTEM! Añade uno desde 'UI > Event System'.");
            eventSystemChecked = false;
        }
        else
        {
            eventSystemChecked = true;
        }
    }

    void Update()
    {
        // ... (Tu código de Update() sigue igual)
        Frame frame = leapProvider.CurrentFrame;
        if (frame == null || frame.Hands.Count == 0)
        {
            if (isPinching) isPinching = false;
            return;
        }
        Hand hand = frame.Hands.Find(h => h.IsRight) ?? frame.Hands[0];
        float currentPinchStrength = hand.PinchStrength;
        if (currentPinchStrength > pinchOnThreshold && !isPinching)
        {
            isPinching = true;
            Debug.Log($"PINCH ON (Fuerza: {currentPinchStrength})");
            TrySelectPlanet();
        }
        else if (currentPinchStrength < pinchOffThreshold && isPinching)
        {
            isPinching = false;
        }
    }

    public void TrySelectPlanet()
    {
        if (!eventSystemChecked) return;
        if (mainCamera == null || pointerUI == null) return;

        Vector3 screenPos = pointerUI.position;
        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        // --- LÍNEA MODIFICADA ---
        // Cambiamos Physics.Raycast por Physics.SphereCast

        if (Physics.SphereCast(ray, clickRadius, out RaycastHit hit, 1000f))
        {
            // --- FIN LÍNEA MODIFICADA ---

            var clickable = hit.collider.GetComponent<PlanetClickable>();
            if (clickable != null)
            {
                var eventData = new PointerEventData(EventSystem.current);
                clickable.OnPointerClick(eventData);
                Debug.Log($"🌍 Pinch sobre {hit.collider.name} → Click ejecutado.");
            }
            else
            {
                Debug.Log($"Pinch sobre {hit.collider.name}, pero no tiene el script 'PlanetClickable'.");
            }
        }
        else
        {
            Debug.Log("Pinch en el aire, no se golpeó nada.");
        }
    }
}