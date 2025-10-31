﻿using UnityEngine;
using UnityEngine.UI;
using Leap;

// Añadimos esto para asegurarnos de que el CanvasGroup exista
[RequireComponent(typeof(CanvasGroup))]
public class LeapMousePointer : MonoBehaviour
{
    public LeapServiceProvider leapProvider; // Leap provider
    public RectTransform pointerUI;          // Imagen del cursor
    public float sensitivity = 0.003f;       // Escala de movimiento
    public bool invertX = false;
    public bool invertY = false;
    [Range(0f, 1f)]
    public float smoothing = 0.25f;          // Suavizado (0 = sin filtro, 1 = muy suave)

    // --- NUEVAS VARIABLES PARA EL FADE ---
    [Header("Fade Out Settings")]
    public float fadeOutDelay = 2.0f;        // Tiempo para empezar el fade out
    private float handLostTimer = 0.0f;      // Temporizador interno
    private CanvasGroup pointerCanvasGroup;  // Referencia al CanvasGroup para el fade
    // --- FIN NUEVAS VARIABLES ---

    private bool calibrated = false;
    private Vector3 neutralTip;
    private Vector3 smoothedTip;

    // private Vector3 lastPointerPosition; // Ya no necesitamos esta variable

    // --- NUEVO MÉTODO Start() ---
    void Start()
    {
        // Obtenemos el componente CanvasGroup
        pointerCanvasGroup = pointerUI.GetComponent<CanvasGroup>();
        if (pointerCanvasGroup == null)
        {
            Debug.LogError("No se encontró CanvasGroup en el pointerUI. Añadiendo uno...");
            pointerCanvasGroup = pointerUI.gameObject.AddComponent<CanvasGroup>();
        }

        // Empezar invisible y desactivado
        pointerCanvasGroup.alpha = 0f;
        pointerUI.gameObject.SetActive(false);
    }
    // --- FIN NUEVO MÉTODO Start() ---


    void Update()
    {
        Frame frame = leapProvider.CurrentFrame;

        // Comprobamos si hay manos
        if (frame.Hands.Count > 0)
        {
            // --- MANO DETECTADA ---

            // 1. Resetear el temporizador y asegurar que es visible
            handLostTimer = 0.0f;
            pointerUI.gameObject.SetActive(true);
            pointerCanvasGroup.alpha = 1.0f;

            // 2. Tomar toda tu lógica de cálculo de posición
            Hand hand = frame.Hands[0];
            Vector3 palm = hand.PalmPosition;

            // 🔹 Ignorar el eje Z (mantener solo plano XY)
            palm.z = 0f;

            if (!calibrated)
            {
                neutralTip = palm;
                smoothedTip = palm;
                calibrated = true;
            }

            // Suavizado adaptativo
            float distance = Vector3.Distance(smoothedTip, palm);
            float adaptiveSmooth = Mathf.Lerp(0.8f, 0.2f, Mathf.Clamp01(distance * 10f));
            smoothedTip = Vector3.Lerp(smoothedTip, palm, (1 - smoothing) * (1 - adaptiveSmooth) + smoothing);

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

            // Actualizar la posición del puntero
            pointerUI.position = screenPos;
        }
        else
        {
            // --- MANO NO DETECTADA ---

            calibrated = false; // Perder calibración

            // Solo procesar el fade si el puntero estaba activo
            if (pointerUI.gameObject.activeSelf)
            {
                // Incrementar el temporizador
                handLostTimer += Time.deltaTime;

                // Calcular el alfa (de 1 a 0)
                float newAlpha = 1.0f - (handLostTimer / fadeOutDelay);

                // Aplicar el alfa
                pointerCanvasGroup.alpha = Mathf.Clamp01(newAlpha);

                // ** IMPORTANTE: No actualizamos pointerUI.position **
                // Esto hace que se quede en la última posición conocida.

                // Si el temporizador se completó, desactivar el objeto
                if (handLostTimer >= fadeOutDelay)
                {
                    pointerUI.gameObject.SetActive(false);
                    handLostTimer = 0.0f; // Resetear
                }
            }
        }
    }
}