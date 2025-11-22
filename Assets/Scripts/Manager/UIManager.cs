using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;



public class UIManager : MonoBehaviour
{
    // --- Singleton ---
    public static UIManager Instance { get; private set; }

    [Header("Fade Settings")]
    [Tooltip("Duración por defecto del fade in/out")]
    public float defaultFadeDuration = 0.35f;
    public AnimationCurve fadeEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Control interno: una corrutina por panel para no solapar fades
    private readonly Dictionary<GameObject, Coroutine> _runningFades = new();


    [Header("------ Start Menu ------")]
    public GameObject startMenuCanvas;
    public TMP_Text startButtonText;
    public TMP_Text planetariumTitleText;

    [Header("------ Planet Elements ------")]
    public GameObject PlanetMenu;

    public TMP_Text planetTitle;
    public TMP_Text planetDescription;


    [Header("------ Mode Banner ------")]
    public GameObject modeBannerPanel;   // Panel raíz con CanvasGroup
    public TMP_Text modeButtonText;

    [Tooltip("Contenido visual de cada modo dentro del panel")]
    public GameObject kidContent;
    public GameObject normalContent;
    public GameObject expertContent;

    [Header("Mode Banner Behavior")]
    public float modeBannerDuration = 3.5f; // cuanto tiempo se ve el banner


    private PlanetTextCSVLoader textsDB;

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

        // Panel oculto desde el principio
        if (startMenuCanvas) startMenuCanvas.SetActive(true);
        if (PlanetMenu) PlanetMenu.SetActive(false);

        if (modeBannerPanel)
        {
            modeBannerPanel.SetActive(false);
            HideAllModeContents();
        }

    }

    private void Start()
    {
        textsDB = PlanetTextCSVLoader.Instance;
    }
    public void ClickedStart()
    {
        //Debug.Log("Clicked Start");
        if (startMenuCanvas) ShowPanelFade(startMenuCanvas, false);
        GameManager.Instance.StartVisit();
    }

    public void ClickedMuseumMap()
    {
        if (startMenuCanvas) ShowPanelFade(startMenuCanvas, false);

        // delegamos la lógica al GameManager
        GameManager.Instance.GoToMuseumMap();
    }

    //Metodo para probar los modos porque no tengo el leap
    public void ChangeMode()
    {
        GameMode? pose = null;
        if (startMenuCanvas != null)
        {
            if (GameManager.Instance?.CurrentMode == GameMode.Kid)
            {
                pose = GameMode.Normal;
            }
            else if (GameManager.Instance?.CurrentMode == GameMode.Normal)
            {
                pose = GameMode.Expert;
            }
            else
            {
                pose = GameMode.Kid;
            }
            GameManager.Instance?.SetMode(pose.Value);
        }

    }

    // --------- NUEVO: método genérico con fade ----------
    /// <summary>
    /// Muestra u oculta un panel con fade usando CanvasGroup.
    /// </summary>
    public void ShowPanelFade(GameObject panel, bool show = true, float? duration = null, bool setInteractable = true, bool setBlocksRaycasts = true)
    {
        if (panel == null) return;

        // Si vamos a MOSTRAR y el panel está inactivo, creamos/aseguramos el CanvasGroup con alpha=0 antes de activarlo
        CanvasGroup cg = (!panel.activeSelf && show)
            ? EnsureCanvasGroup(panel, initialAlpha: 0f)   // ⟵ clave: invisible antes de SetActive(true)
            : EnsureCanvasGroup(panel);                    // reutiliza el existente sin forzar alpha

        if (show && !panel.activeSelf)
            panel.SetActive(true); // ya está con alpha=0, no habrá "pop"

        float dur = duration ?? defaultFadeDuration;

        // Evita solapar fades previos en este panel
        if (_runningFades.TryGetValue(panel, out var co) && co != null)
            StopCoroutine(co);

        // Si hacemos fade IN, habilita input durante la transición (opcional)
        if (show)
        {
            if (setInteractable) cg.interactable = true;
            if (setBlocksRaycasts) cg.blocksRaycasts = true;
        }

        _runningFades[panel] = StartCoroutine(FadeCanvasGroup(
            panel, cg,
            target: show ? 1f : 0f,
            duration: dur,
            setInteractable: setInteractable,
            setBlocksRaycasts: setBlocksRaycasts
        ));
    }

    private IEnumerator FadeCanvasGroup(GameObject panel, CanvasGroup cg, float target, float duration, bool setInteractable, bool setBlocksRaycasts)
    {
        float start = cg.alpha;
        float t = 0f;
        // Asegurar que durante el fade respondamos a input si estamos mostrando
        if (target > start)
        {
            if (setInteractable) cg.interactable = true;
            if (setBlocksRaycasts) cg.blocksRaycasts = true;
        }

        if (duration <= 0f)
        {
            cg.alpha = target;
        }
        else
        {
            while (t < duration)
            {
                t += Time.unscaledDeltaTime; // UI suele ir con tiempo no escalado
                float k = fadeEase.Evaluate(Mathf.Clamp01(t / duration));
                cg.alpha = Mathf.Lerp(start, target, k);
                yield return null;
            }
            cg.alpha = target;
        }

        // Si hemos hecho fade out completo, desactivar interacción y el propio panel
        if (Mathf.Approximately(target, 0f))
        {
            if (setInteractable) cg.interactable = false;
            if (setBlocksRaycasts) cg.blocksRaycasts = false;
            panel.SetActive(false);
        }

        _runningFades[panel] = null;
    }

    private CanvasGroup EnsureCanvasGroup(GameObject panel, float? initialAlpha = null)
    {
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        if (initialAlpha.HasValue) cg.alpha = initialAlpha.Value;
        return cg;
    }
    // -----------------------------------------------------

    // Mantengo tu API, pero ahora usa fade:
    public void ShowPlanetPanel(bool show = true)
    {
        if (!PlanetMenu)
        {
            return;
        }

        if (show == true)
        {
            PlanetClickable p = GameManager.Instance.GetCurrentTarget();
            if (p != null)
            {

            }
            else
            {
                Debug.Log("Error, planeta solicitado pero no hay ninguno como objetivo");
            }

            SetPlanetTitle(p);
            SetPlanetInfo(p, GameManager.Instance.CurrentMode);
        }

        ShowPanelFade(PlanetMenu, show);
    }

    public void SetPlanetTitle(PlanetClickable planet)
    {
        if (planetTitle) planetTitle.text = textsDB.GetNombre(planet);
    }

    public void SetPlanetInfo(PlanetClickable planet, GameMode mode)
    {
        if (planetDescription)
        {
            string info = textsDB.GetInfo(planet, mode);

            planetDescription.text = info;
        }
    }

    public void changeLanguage(string newLanguage)
    {
        Language lan;

        if (PlanetTextCSVLoader.TryParseLanguage(newLanguage, out lan))
        {
            PlanetTextCSVLoader.Instance.setLanguage(lan);
            refreshUI();
        }
    }

    public void refreshUI()
    {
        if (GameManager.Instance.GetState() == GameStates.MainPanel)
        {
            startButtonText.text = PlanetTextCSVLoader.Instance.GetText("start_button");
            modeButtonText.text = PlanetTextCSVLoader.Instance.GetText("mode_button");

            if (planetariumTitleText)
                planetariumTitleText.text = PlanetTextCSVLoader.Instance.GetText("planetarium_name");

            return;
        }

        if (GameManager.Instance.GetCurrentTarget() == null) return;

        // Si hay un target, actualizamos los textos
        PlanetClickable p = GameManager.Instance.GetCurrentTarget();
        SetPlanetInfo(p, GameManager.Instance.CurrentMode);

        //SetPlanetTitle(PlanetTextCSVLoader.Instance.GetNombre(p));
        //BuildPlanetPages(p.GetId());


    }

    private void HideAllModeContents()
    {
        if (kidContent) kidContent.SetActive(false);
        if (normalContent) normalContent.SetActive(false);
        if (expertContent) expertContent.SetActive(false);
    }

    public void ShowModeBanner(GameMode mode)
    {
        if (!modeBannerPanel) return;

        // Apagar todos los hijos primero
        HideAllModeContents();

        // Encender solo el que toca
        GameObject toActivate = null;
        switch (mode)
        {
            case GameMode.Kid:
                toActivate = kidContent;
                break;
            case GameMode.Normal:
                toActivate = normalContent;
                break;
            case GameMode.Expert:
                toActivate = expertContent;
                break;
        }

        if (toActivate) toActivate.SetActive(true);

        // Mostrar el panel con tu sistema de fade
        ShowPanelFade(modeBannerPanel, true);

        // Opcional: ocultarlo solo después de unos segundos
        StartCoroutine(HideModeBannerAfterDelay(modeBannerDuration));
    }

    private IEnumerator HideModeBannerAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ShowPanelFade(modeBannerPanel, false);
    }


}
