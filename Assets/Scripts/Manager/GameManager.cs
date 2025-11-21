using UnityEngine;
using UnityEngine.Rendering;

public enum GameMode { Kid, Normal, Expert }
public class GameManager : MonoBehaviour
{
    // --- Singleton ---
    public static GameManager Instance { get; private set; }
    public GameMode CurrentMode { get; private set; } = GameMode.Normal;

    // Fields
    private UIManager uiManager;
    private PlanetTextCSVLoader textsDB;

    [SerializeField] private BlurVolume sceneVolume;
    [SerializeField] private PlanetZoomController cam;

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

    public PlanetClickable GetCurrentTarget()
    {
        return cam.currentTarget;
    }

    public GameStates GetState ()
    {
        return _state;
    }
    public void SetMode(GameMode mode)
    {
        if (CurrentMode == mode) return;
        CurrentMode = mode;
        Debug.Log($"Modo cambiado a: {mode}");

        UIManager.Instance?.ShowModeBanner(mode);
        // Aquí activa la versión del juego:
        // UIManager.Instance.ShowBanner(mode);
        // Cargar perfil de dificultad, etc.
    }
    

    private void Start()
    {
        uiManager = UIManager.Instance;
        textsDB = PlanetTextCSVLoader.Instance;

        _state = GameStates.MainPanel;

        uiManager.refreshUI();
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

    /*public void RequestZoom (PlanetClickable planet)
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

            uiManager.SetPlanetTitle(textsDB.GetNombre(planet));
            uiManager.SetPlanetInfo(textsDB.GetInfo(planet, 0));
        }
    }*/

    public void RequestZoom(PlanetClickable planet)
    {
        if (_state == GameStates.MainPanel)
        {
            return;
        }

        bool? zoomIn = cam.RequestZoom(planet);

        if (zoomIn == false)
        {
            // Zoom out: ocultar panel directamente
            _state = GameStates.MainView;
            uiManager.ShowPlanetPanel(false);
        }
        else
        {
            // Zoom in: cuando termine el zoom, HandleZoomCompleted se encargará de mostrar panel y refrescar
            _state = GameStates.MainView;

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
                uiManager.ShowPlanetPanel(true);
                uiManager.refreshUI();
            }
        }
    }

    // --------- Swipe handlers ----------
    public void HandleSwipeRight(Leap.Hand _)
    {
        if (cam && cam.currentTarget)
        {
            cam.SelectNeighbor(+1, wrap: true);
            if (_state != GameStates.MainPanel)
            {
                uiManager.refreshUI();    // planeta nuevo → páginas nuevas
            }
        }
    }
    public void HandleSwipeLeft(Leap.Hand _)
    {
        if (cam && cam.currentTarget)
        {
            cam.SelectNeighbor(-1, wrap: true);
            if (_state != GameStates.MainPanel)
            {
                uiManager.refreshUI();
            }
        }
    }


}
