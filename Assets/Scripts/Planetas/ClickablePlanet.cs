using UnityEngine;
using UnityEngine.EventSystems;

public class PlanetClickable : MonoBehaviour, IPointerClickHandler
{
    public float zoomDistance = 5f;       // How close the camera gets
    public float zoomSpeed = 3f;          // Smoothness of zoom

    private static Transform currentTarget; // Planet currently zoomed
    private static Vector3 defaultCamPos;
    private static Quaternion defaultCamRot;
    private static bool isZooming = false;

    [SerializeField] private Manager manager;
    public string displayName;
    void Awake()
    {
        if (manager == null)
            manager = Object.FindFirstObjectByType<Manager>(FindObjectsInactive.Include);
    }

    void Start()
    {
        // Store camera default position once
        if (defaultCamPos == Vector3.zero)
        {
            defaultCamPos = Camera.main.transform.position;
            defaultCamRot = Camera.main.transform.rotation;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentTarget == transform)
        {
            // Zoom out
            currentTarget = null;
            if (!isZooming) StartCoroutine(ZoomOut());

            if (manager) manager.ShowPlanetPanel(false);
        }
        else
        {
            // Zoom into this planet
            currentTarget = transform;
            if (!isZooming) StartCoroutine(ZoomIn());

            string title = string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;
            if (manager) manager.SetPlanetTitle(title);   // <-- poner el nombre
            if (manager) manager.ShowPlanetPanel(true);
        }
    }

    private System.Collections.IEnumerator ZoomIn()
    {
        isZooming = true;

        Camera cam = Camera.main;

        // Estimate planet size (radius)
        float radius = 1f;
        MeshRenderer rend = GetComponent<MeshRenderer>();
        if (rend != null)
        {
            // El tamaño visible (en unidades del mundo)
            radius = rend.bounds.extents.magnitude;
        }
        // We want to position the camera to the *side* of the planet
        // Vector3.right means we move along the X-axis (you can change to forward/back/right/left)
        Vector3 sideDirection = Vector3.right;
        // Optional: use a fixed direction if you want all side views from same direction, e.g. Vector3.right

        // Distance from planet
        float safeDistance = radius * 2.5f + zoomDistance;

        // Compute side position
        Vector3 targetPos = transform.position + sideDirection * safeDistance;

        Vector3 lookPos = transform.position;
        lookPos.y -= radius*1.3f;

        while (Vector3.Distance(cam.transform.position, targetPos) > 0.01f)
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, zoomSpeed * Time.deltaTime);
            cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, Quaternion.LookRotation(lookPos - cam.transform.position), zoomSpeed * Time.deltaTime);
            yield return null;
        }

        isZooming = false;
    }



    private System.Collections.IEnumerator ZoomOut()
    {
        isZooming = true;

        Camera cam = Camera.main;

        while (Vector3.Distance(cam.transform.position, defaultCamPos) > 0.01f)
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, defaultCamPos, zoomSpeed * Time.deltaTime);
            cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, defaultCamRot, zoomSpeed * Time.deltaTime);
            yield return null;
        }

        isZooming = false;
    }
}
