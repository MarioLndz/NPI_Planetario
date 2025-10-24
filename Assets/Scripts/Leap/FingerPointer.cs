using UnityEngine;
using Leap;

public class LeapPointerTapSelect : MonoBehaviour
{
    [Header("References")]
    public LeapServiceProvider leapProvider;   // Asigna el LeapServiceProvider
    public RectTransform pointerUI;           // Asigna la Image del cursor (RectTransform)
    public Camera mainCamera;                 // Asigna la cámara principal
    public LayerMask planetLayer;             // Layer donde están los planetas

    [Header("Pointer")]
    public float sensitivity = 0.003f;        // Escala de movimiento XY
    [Range(0f, 1f)] public float smoothing = 0.25f; // Suavizado adaptativo
    public bool invertX = false;
    public bool invertY = false;

    [Header("Tap detection")]
    public float tapVelocityThreshold = 0.15f; // velocidad en Z para considerar "tap" (m/s)
    public float tapCooldown = 0.45f;          // segundos entre taps
    public bool invertTapDirection = false;    // true si hacia delante da valor positivo en tu setup

    // debug
    public bool debugLogs = true;

    // internals
    private bool calibrated = false;
    private Vector3 neutralTip;    // neutro en XY (Z ignorada)
    private Vector3 smoothedTip;
    private float lastTipZ = 0f;
    private float lastTapTime = -10f;

    void Update()
    {
        if (leapProvider == null || pointerUI == null || mainCamera == null)
        {
            if (debugLogs) Debug.LogWarning("[LeapPointerTapSelect] Asigna leapProvider, pointerUI y mainCamera en el inspector.");
            return;
        }

        Frame frame = leapProvider.CurrentFrame;
        if (frame == null) return;

        if (frame.Hands.Count == 0)
        {
            calibrated = false;
            return;
        }

        Hand hand = frame.Hands[0];
        Finger index = hand.GetFinger(Finger.FingerType.INDEX);

        if (!index.IsExtended)
        {
            calibrated = false;
            return;
        }

        // posición actual del tip en metros (espacio Leap)
        Vector3 tip = index.TipPosition;

        // Ignoramos Z para posición del cursor (solo plano XY)
        Vector3 tipXY = new Vector3(tip.x, tip.y, 0f);

        // Calibración inicial (cuando aparece el dedo)
        if (!calibrated)
        {
            neutralTip = tipXY;
            smoothedTip = tipXY;
            lastTipZ = tip.z;
            calibrated = true;
            if (debugLogs) Debug.Log("[LeapPointerTapSelect] Calibrado neutralTip = " + neutralTip);
        }

        // --- Suavizado adaptativo (evita jitter sin añadir inercia perceptible) ---
        float distance = Vector3.Distance(smoothedTip, tipXY);
        float adaptiveSmooth = Mathf.Lerp(0.8f, 0.2f, Mathf.Clamp01(distance * 10f));
        // factor final de mezcla; small smoothing = more reactive
        float blend = (1f - smoothing) * (1f - adaptiveSmooth) + smoothing;
        smoothedTip = Vector3.Lerp(smoothedTip, tipXY, blend);

        // Offset relativo al neutro (solo XY)
        Vector3 offset = smoothedTip - neutralTip;

        float xNorm = 0.5f + (invertX ? -1f : 1f) * offset.x * sensitivity;
        float yNorm = 0.5f + (invertY ? -1f : 1f) * offset.y * sensitivity;

        xNorm = Mathf.Clamp01(xNorm);
        yNorm = Mathf.Clamp01(yNorm);

        Vector3 screenPos = new Vector3(xNorm * Screen.width, yNorm * Screen.height, 0f);

        // Mover cursor UI directamente (sin lerp para mantener sensación de ratón)
        pointerUI.position = screenPos;

        // --- Detección de tap: usamos la velocidad del TIP en Z ---
        float tipZ = tip.z;
        float vz = 0f;
        if (Time.deltaTime > 0f) vz = (tipZ - lastTipZ) / Time.deltaTime; // m/s

        // Para muchos setups, un movimiento "hacia dentro" reduce Z (vz negativo).
        // Si tu sensor da la dirección opuesta, activa invertTapDirection.
        float checkVz = invertTapDirection ? -vz : vz;

        if (debugLogs)
        {
            // logs puntuales (puedes comentar para no saturar la consola)
            Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * 5f, Color.cyan, 0.05f);
            // Debug.Log($"tipZ={tipZ:F3} vz={vz:F3} checkVz={checkVz:F3} screenPos={screenPos}");
        }

        if (checkVz < -tapVelocityThreshold && (Time.time - lastTapTime) > tapCooldown)
        {
            lastTapTime = Time.time;
            if (debugLogs) Debug.Log("[LeapPointerTapSelect] Tap detectado (vz=" + vz.ToString("F3") + ")");
            TrySelectPlanetAt(screenPos);
        }

        lastTipZ = tipZ;
    }

    private void TrySelectPlanetAt(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (debugLogs) Debug.DrawRay(ray.origin, ray.direction * 50f, Color.yellow, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, planetLayer))
        {
            GameObject planet = hit.collider.gameObject;
            if (debugLogs) Debug.Log("🌍 Planeta seleccionado: " + planet.name + " (hit normal: " + hit.normal + ")");
            // Ejemplo de feedback:
            var rend = planet.GetComponent<Renderer>();
            if (rend != null)
            {
                // tiny flash
                StartCoroutine(FlashMaterial(rend));
            }
            // Aquí puedes llamar a tu lógica de selección
        }
        else
        {
            if (debugLogs) Debug.Log("Tap: no se hitteó ningún planeta.");
        }
    }

    private System.Collections.IEnumerator FlashMaterial(Renderer rend)
    {
        Color orig = rend.material.color;
        rend.material.color = Color.yellow;
        yield return new WaitForSeconds(0.25f);
        rend.material.color = orig;
    }
}
