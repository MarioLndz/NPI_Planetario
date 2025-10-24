using UnityEngine;
using UnityEngine.EventSystems;
using Leap;

public class LeapPlanetSelector : MonoBehaviour
{
    [Header("Referencias")]
    public LeapServiceProvider leapProvider;
    public Camera mainCamera;
    public RectTransform pointerUI;

    [Header("Pinch detection")]
    [Range(0f, 1f)] public float pinchOnThreshold = 0.8f;
    [Range(0f, 1f)] public float pinchOffThreshold = 0.7f;

    private bool isPinching = false;

    void Start()
    {
        if (leapProvider == null) Debug.Log("Añadir leapService");

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        Frame frame = leapProvider.CurrentFrame;
        if (frame == null || frame.Hands.Count == 0)
        {
            isPinching = false;
            return;
        }

        // Usa la mano derecha si existe
        Hand hand = frame.Hands.Find(h => h.IsRight) ?? frame.Hands[0];
        float pinchStrength = hand.PinchStrength;

        // Detección con histéresis
        if (pinchStrength > pinchOnThreshold && !isPinching)
        {
            isPinching = true;
            TrySelectPlanet();
        }
        else if (pinchStrength < pinchOffThreshold && isPinching)
        {
            isPinching = false;
        }
    }

    void TrySelectPlanet()
    {
        if (mainCamera == null || pointerUI == null)
            return;

        // Convertir posición del cursor UI (en pantalla) a un rayo 3D
        Vector3 screenPos = pointerUI.position;
        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            // ¿Tiene el planeta el script PlanetClickable?
            var clickable = hit.collider.GetComponent<PlanetClickable>();
            if (clickable != null)
            {
                // Crear datos de evento simulados (como si fuera un click real)
                var eventData = new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute(clickable.gameObject, eventData, ExecuteEvents.pointerClickHandler);

                Debug.Log($"🌍 Pinch sobre {hit.collider.name} → Click ejecutado.");
            }
            else
            {
                Debug.Log($"Pinch sobre {hit.collider.name}, pero no es clicable.");
            }
        }
    }
}