using UnityEngine;
using Leap;

public class SwipeGestureDetector : MonoBehaviour
{
    public LeapServiceProvider provider;

    [Header("Parámetros del gesto")]
    public float swipeSpeed = 1.5f;           // velocidad mínima (m/s)
    public float limitTangent = 0.6f;         // (Antes verticalLimit) proporción para filtrar gestos en otros ejes
    public float cooldown = 1.0f;             // tiempo mínimo entre gestos (segundos)
    public float maxGrabStrength = 0.3f;      // mano abierta (no puño)

    private float lastSwipeTime = 0f;

    void Update()
    {
        if (provider == null) return;

        Frame frame = provider.CurrentFrame;
        if (frame == null || frame.Hands.Count == 0) return;

        // ⬇️ SOLO MANO DERECHA
        Hand rightHand = frame.Hands.Find(h => h.IsRight);
        if (rightHand == null) return;

        // Solo manos abiertas
        if (rightHand.GrabStrength < maxGrabStrength)
        {
            DetectSwipe(rightHand);
        }
    }

    void DetectSwipe(Hand hand)
    {
        // Evitar repetición por cooldown
        if (Time.time - lastSwipeTime < cooldown)
            return;

        // Obtenemos las velocidades en los 3 ejes
        float vx = hand.PalmVelocity.x;
        float vy = hand.PalmVelocity.y;
        float vz = hand.PalmVelocity.z;

        // Calculamos los valores absolutos para comparar magnitudes
        float absX = Mathf.Abs(vx);
        float absY = Mathf.Abs(vy);
        float absZ = Mathf.Abs(vz);

        // --- DETECCIÓN HORIZONTAL (Izquierda / Derecha) ---
        // El movimiento en X debe ser mayor que la velocidad mínima
        // Y debe ser el movimiento dominante (mayor que Y y mayor que Z)
        if (absX > swipeSpeed && absX > absY && absX > absZ)
        {
            // Filtro extra: que los otros ejes no superen el límite proporcional
            if (absY < limitTangent * absX && absZ < limitTangent * absX)
            {
                if (vx > 0)
                {
                    GameManager.Instance.HandleSwipeLeft(hand);
                    Debug.Log("➡️ Swipe Derecha");
                }
                else
                {
                    GameManager.Instance.HandleSwipeRight(hand);
                    Debug.Log("⬅️ Swipe Izquierda");
                }
                lastSwipeTime = Time.time;
            }
        }

        // --- DETECCIÓN PROFUNDIDAD (Atrás) ---
        // El movimiento en Z debe ser mayor que la velocidad mínima
        // Y debe ser dominante sobre X e Y
        else if (absZ > swipeSpeed && absZ > absY && absZ > absX)
        {
            // Filtro extra
            if (absY < limitTangent * absZ && absX < limitTangent * absZ)
            {
                // NOTA: En Unity/Leap, generalmente "Atrás" (hacia el usuario) es Z negativo
                // Si usas el Leap en modo "Head Mounted" (VR), esto podría invertirse.

                if (vz < 0) // < 0 suele ser "tirar hacia atrás" (pull)
                {
                    // Asegúrate de crear este método en tu GameManager
                    GameManager.Instance.HandleSwipeBack(hand); 
                    Debug.Log("⬇️ Swipe Atrás (Pull)");

                    lastSwipeTime = Time.time;
                }
                // Si quisieras swipe hacia adelante (Push), sería else { if (vz > 0) ... }
            }
        }
    }
}