using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    // --- Singleton ---
    public static GameManager Instance { get; private set; }

    // Fields
    private UIManager uiManager;
    [SerializeField] private BlurVolume sceneVolume;
    [SerializeField] private PlanetZoomController cam;

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


        if (cam != null)
        {
            cam.OnZoomCompleted -= HandleZoomCompleted; // evita doble suscripción
            cam.OnZoomCompleted += HandleZoomCompleted;
        }
    }

    private void Start()
    {
        uiManager = UIManager.Instance;
    }

    public void ToggleBackgroundBlur ()
    {
        sceneVolume.ToggleBackgroundBlur();
    }

    public void RequestZoom (PlanetClickable planet)
    {
        // Zoom al planeta
        bool? zoomIn = cam.RequestZoom (planet);

        if (zoomIn == false)
        {
            // Zoom out: ocultar panel directamente
            uiManager.ShowPlanetPanel(false);
        }
    }

    private void HandleZoomCompleted(bool zoomIn)
    {
        if (zoomIn)
        {
            // Mostrar panel del planeta ACTUAL que está como target en la cámara
            var planet = cam.currentTarget;
            if (planet != null)
            {
                string title = string.IsNullOrWhiteSpace(planet.displayName) ? planet.gameObject.name : planet.displayName;
                uiManager.SetPlanetTitle(title);
                uiManager.ShowPlanetPanel(true);
            }
        }
    }

}
