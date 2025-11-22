using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para reiniciar la escena
using Leap;

public class SceneAutoReset : MonoBehaviour
{
    [Header("Configuración Leap")]
    public LeapServiceProvider provider;

    [Header("Configuración de Reinicio")]
    [Tooltip("Tiempo en segundos sin manos detectadas antes de reiniciar.")]
    public float timeToReset = 5.0f;

    [Header("Debug (Solo lectura)")]
    [SerializeField] private float currentTimer = 0f;

    void Update()
    {
        if (provider == null) return;

        if (GameManager.Instance.GetState() == GameStates.MainPanel)
        {
            currentTimer = 0f;
            return;
        }

        // Obtenemos el frame actual
        Frame frame = provider.CurrentFrame;

        // Verificamos si hay ALGUNA mano visible (Count > 0)
        if (frame != null && frame.Hands.Count > 0)
        {
            // Si hay manos, reseteamos el temporizador a 0
            currentTimer = 0f;
        }
        else
        {
            // Si NO hay manos, empezamos a sumar tiempo
            currentTimer += Time.deltaTime;

            // Si el tiempo supera el límite establecido
            if (currentTimer >= timeToReset)
            {
                Debug.Log("⏳ Tiempo de inactividad excedido. Reiniciando escena...");
                GameManager.Instance.ResetScene();
            }
        }
    }
}