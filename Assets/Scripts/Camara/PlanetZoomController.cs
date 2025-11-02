using UnityEngine;
using System.Collections;
using System; // <-- para Action

// Pon este script en tu objeto Main Camera
public class PlanetZoomController : MonoBehaviour
{
    [Header("Configuración de Zoom")]
    public float zoomDuration = 1.5f; // Duración global del zoom

    private Camera cam;
    private Vector3 defaultCamPos;
    private Quaternion defaultCamRot;

    private bool isZooming = false;
    public PlanetClickable currentTarget;

    public event Action<bool> OnZoomCompleted; // true = zoom in, false = zoom out
    private bool lastZoomIn;                   // recordamos qué acción estamos haciendo



    void Start()
    {
        // Se asume que este script está en la cámara, pero
        // Camera.main la encontrará de todas formas.
        cam = Camera.main;
        if (cam != null)
        {
            defaultCamPos = cam.transform.position;
            defaultCamRot = cam.transform.rotation;
        }
    }

    /// <summary>
    /// El planeta llama a esta función para solicitar un zoom.
    /// </summary>
    public bool? RequestZoom(PlanetClickable planet)
    {
        if (isZooming) return null;

        if (currentTarget == planet)
        {
            // --- ZOOM OUT ---
            Debug.Log("Zoom Out");
            currentTarget = null;
            lastZoomIn = false;
            StartCoroutine(MoveCamera(defaultCamPos, defaultCamRot));
            return false;
        }
        else
        {
            // --- ZOOM IN ---
            currentTarget = planet;


            // Calcula el destino usando los datos del planeta
            float radius = 1f;
            MeshRenderer rend = planet.GetComponent<MeshRenderer>();
            if (rend != null) { radius = rend.bounds.extents.magnitude; }

            Vector3 sideDirection = Vector3.right;
            float safeDistance = radius * 2.5f + planet.zoomDistance;
            Vector3 targetPos = planet.transform.position + sideDirection * safeDistance;

            Vector3 lookPos = planet.transform.position;
            lookPos.y -= radius * 1.3f;
            Quaternion targetRot = Quaternion.LookRotation(lookPos - targetPos);

            // Inicia el movimiento
            lastZoomIn = true;
            StartCoroutine(MoveCamera(targetPos, targetRot));

            return true;
        }
    }

    /// <summary>
    /// Corrutina genérica para mover la cámara
    /// </summary>
    private IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot)
    {
        isZooming = true;
        float timer = 0f;

        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        while (timer < zoomDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / zoomDuration);
            t = t * t * (3f - 2f * t); // Suavizado

            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        cam.transform.position = targetPos;
        cam.transform.rotation = targetRot;
        isZooming = false;

        OnZoomCompleted?.Invoke(lastZoomIn);
    }

}