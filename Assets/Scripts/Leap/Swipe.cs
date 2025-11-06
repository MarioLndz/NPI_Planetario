using UnityEngine;
using UnityEngine.EventSystems;
using Leap;

public class SwipeGestureDetector : MonoBehaviour
{
    public LeapServiceProvider provider;

    [Header("Parámetros del gesto")]
    public float swipeSpeed = 1.5f;           // velocidad mínima (m/s)
    public float verticalLimit = 0.6f;        // proporción para filtrar gestos verticales
    public float cooldown = 1.0f;             // tiempo mínimo entre gestos (segundos)
    public float maxGrabStrength = 0.3f;      // mano abierta (no puño)

    private float lastSwipeTime = 0f;

    // Eventos públicos
    public delegate void SwipeAction(Hand hand);

    void Update()
    {
        if (provider == null) return;

        Frame frame = provider.CurrentFrame;

        foreach (Hand hand in frame.Hands)
        {
            // Solo manos abiertas
            if (hand.GrabStrength < maxGrabStrength)
                DetectSwipe(hand);
        }
    }

    void DetectSwipe(Hand hand)
    {
        // Evitar repetición por cooldown
        if (Time.time - lastSwipeTime < cooldown)
            return;

       
        float vx = hand.PalmVelocity.x;
        float vy = hand.PalmVelocity.y;

        // Detectar movimiento lateral dominante
        if (Mathf.Abs(vx) > swipeSpeed && Mathf.Abs(vy) < verticalLimit * Mathf.Abs(vx))
        {
            if (vx > 0)
            {
                GameManager.Instance.HandleSwipeLeft(hand);
                Debug.Log("➡️ Swipe a la derecha detectado");
            }
            else
            {
                GameManager.Instance.HandleSwipeRight(hand);
                Debug.Log("⬅️ Swipe a la izquierda detectado");
            }

            lastSwipeTime = Time.time; // activar cooldown
        }
    }
}
