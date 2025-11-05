using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    // --- Singleton ---
    public static GameManager Instance { get; private set; }

    // Fields
    private UIManager uiManager;
    private PlanetTextCSVLoader textsDB;

    [SerializeField] private BlurVolume sceneVolume;
    [SerializeField] private PlanetZoomController cam;
    [SerializeField] private SwipeGestureDetector swipe;  // <-- NUEVO

    private GameStates _state;

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

    void OnEnable()
    {
        // <<< AÑADIR SUSCRIPCIÓN >>>
        if (swipe != null)
        {
            swipe.OnSwipeRight += HandleSwipeRight;
            swipe.OnSwipeLeft += HandleSwipeLeft;
        }
    }

    void OnDisable()
    {
        if (swipe != null)
        {
            swipe.OnSwipeRight -= HandleSwipeRight;
            swipe.OnSwipeLeft -= HandleSwipeLeft;
        }
    }

    private void Start()
    {
        uiManager = UIManager.Instance;
        textsDB = PlanetTextCSVLoader.Instance;

        _state = GameStates.MainPanel;
    }

    public void StartVisit ()
    {
        ToggleBackgroundBlur();
        _state = GameStates.MainView;
    }

    public void ToggleBackgroundBlur ()
    {
        sceneVolume.ToggleBackgroundBlur();
    }

    public void RequestZoom (PlanetClickable planet)
    {
        if (_state == GameStates.MainPanel)
        {
            return;
        }

        // Zoom al planeta
        bool? zoomIn = cam.RequestZoom (planet);

        if (zoomIn == false)
        {
            // Zoom out: ocultar panel directamente
            _state = GameStates.MainView;
            uiManager.ShowPlanetPanel(false);
        } else {
            // Zoom in:
            _state = GameStates.MainView;

            uiManager.SetPlanetInfo(textsDB.GetText("mercurio2", "SPANISH"));
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

    // --------- Swipe handlers ----------
    private void HandleSwipeRight(Leap.Hand _)
    {
        if (cam && cam.currentTarget) cam.SelectNeighbor(+1, wrap: true);
    }
    private void HandleSwipeLeft(Leap.Hand _)
    {
        if (cam && cam.currentTarget) cam.SelectNeighbor(-1, wrap: true);
    }


}
