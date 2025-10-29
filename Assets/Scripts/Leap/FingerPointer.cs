using UnityEngine;
using UnityEngine.UI;
using Leap;

public class LeapMousePointer : MonoBehaviour
{
    public LeapServiceProvider leapProvider; // Leap provider
    public RectTransform pointerUI;          // Imagen del cursor
    public float sensitivity = 0.003f;       // Escala de movimiento
    public bool invertX = false;
    public bool invertY = false;
    [Range(0f, 1f)]
    public float smoothing = 0.25f;          // Suavizado (0 = sin filtro, 1 = muy suave)

    private bool calibrated = false;
    private Vector3 neutralTip;
    private Vector3 smoothedTip;

    void Update()
    {
        Frame frame = leapProvider.CurrentFrame;

        if (frame.Hands.Count <= 0)
        {
            calibrated = false;
            pointerUI.gameObject.SetActive(false);
            return;
        }

        Hand hand = frame.Hands[0];
        Finger indexFinger = hand.fingers[(int)Finger.FingerType.INDEX];
        Finger middleFinger = hand.fingers[(int)Finger.FingerType.MIDDLE];

        if (!indexFinger.IsExtended)
        {
            calibrated = false;
            pointerUI.gameObject.SetActive(false);
            return;
        }
        else if(middleFinger.IsExtended)
        {
            // Aquí puedes agregar funcionalidad adicional cuando el pulgar también esté extendido
             return;

        }
        else
        {
            Vector3 tip = indexFinger.TipPosition;

            // 🔹 Ignorar el eje Z (mantener solo plano XY)
            tip.z = 0f;

            if (!calibrated)
            {
                neutralTip = tip;
                smoothedTip = tip;
                calibrated = true;
            }

            // Suavizado adaptativo
            float distance = Vector3.Distance(smoothedTip, tip);
            float adaptiveSmooth = Mathf.Lerp(0.8f, 0.2f, Mathf.Clamp01(distance * 10f));
            smoothedTip = Vector3.Lerp(smoothedTip, tip, (1 - smoothing) * (1 - adaptiveSmooth) + smoothing);

            // Diferencia en plano XY
            Vector3 offset = smoothedTip - neutralTip;

            // Solo usar X e Y
            float x = 0.5f + (invertX ? -1 : 1) * offset.x * sensitivity;
            float y = 0.5f + (invertY ? -1 : 1) * offset.y * sensitivity;

            // Posición en pantalla
            Vector3 screenPos = new Vector3(
                Mathf.Clamp01(x) * Screen.width,
                Mathf.Clamp01(y) * Screen.height,
                0
            );

            pointerUI.gameObject.SetActive(true);
            pointerUI.position = screenPos;
        }
        

        
    }
}
