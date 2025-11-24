using UnityEngine;
using Leap;

public class ExitGesture : MonoBehaviour
{
    public LeapProvider leapProvider; // Arrastra aquí tu LeapServiceProvider

    [Tooltip("Tiempo necesario manteniendo el gesto para salir")]
    public float holdTimeParams = 3.0f;

    private float currentHoldTime = 0f;
    private bool isGestureActive = false;

    void Update()
    {
        if (leapProvider == null) return;

        Frame frame = leapProvider.CurrentFrame;

        // Condición 1: Tienen que verse las DOS manos
        if (frame.Hands.Count == 2)
        {
            bool bothPalmsUp = true;

            foreach (Hand hand in frame.Hands)
            {
                // La normal de la palma es un vector perpendicular a la palma.
                // Si (0, 1, 0) es arriba, buscamos que se parezca a eso.
                // Un valor > 0.7f significa que está bastante inclinada hacia arriba.
                if (hand.PalmNormal.y < 0.7f)
                {
                    bothPalmsUp = false;
                    break;
                }
            }

            if (bothPalmsUp)
            {
                ProcesarGesto();
            }
            else
            {
                ResetGesto();
            }
        }
        else
        {
            ResetGesto();
        }
    }

    void ProcesarGesto()
    {
        if (!isGestureActive)
        {
            isGestureActive = true;
            Debug.Log("Gesto de salida detectado... Mantén la pose.");
        }

        currentHoldTime += Time.deltaTime;

        // Opcional: Feedback visual en consola tipo "Saliendo en 3, 2, 1..."
        // Debug.Log($"Saliendo en: {holdTimeParams - currentHoldTime:F1}");

        if (currentHoldTime >= holdTimeParams)
        {
            SalirDeLaApp();
        }
    }

    void ResetGesto()
    {
        currentHoldTime = 0f;
        isGestureActive = false;
    }

    void SalirDeLaApp()
    {
        Debug.Log("!!! SALIENDO DE LA APLICACIÓN !!!");

        // Esto cierra la app construida (.exe / .apk)
        Application.Quit();

        // Esto para el juego si estás en el Editor de Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}