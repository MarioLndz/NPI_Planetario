using UnityEngine;
using System.Collections;

// Pon este script en tu objeto Main Camera
public class PlanetZoomController : MonoBehaviour
{
    // --- Singleton ---
    // Esto permite que los planetas lo encuentren fácilmente
    public static PlanetZoomController Instance { get; private set; }

    [Header("Configuración de Zoom")]
    public float zoomDuration = 1.5f; // Duración global del zoom

    [Header("Referencias")]
    // Arrastra tu Manager de UI aquí
    [SerializeField] private Manager manager;

    private Camera cam;
    private Vector3 defaultCamPos;
    private Quaternion defaultCamRot;

    private bool isZooming = false;
    private Transform currentTarget;

    void Awake()
    {
        // Configura el Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Busca el Manager si no está asignado
        if (manager == null)
            manager = Object.FindFirstObjectByType<Manager>(FindObjectsInactive.Include);
    }

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
    public void RequestZoom(PlanetClickable planet)
    {
        if (isZooming) return;

        if (currentTarget == planet.transform)
        {
            // --- ZOOM OUT ---
            Debug.Log("Zoom Out");
            currentTarget = null;
            StartCoroutine(MoveCamera(defaultCamPos, defaultCamRot));
            if (manager) manager.ShowPlanetPanel(false);
        }
        else
        {
            // --- ZOOM IN ---
            currentTarget = planet.transform;

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
            StartCoroutine(MoveCamera(targetPos, targetRot));

            // Actualiza la UI
            string title = string.IsNullOrWhiteSpace(planet.displayName) ? planet.gameObject.name : planet.displayName;
            if (manager) manager.SetPlanetTitle(title);
            if (manager) manager.ShowPlanetPanel(true);
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
    }

}