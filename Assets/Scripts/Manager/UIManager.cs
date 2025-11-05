using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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


    public GameObject startMenuCanvas;
    public GameObject PlanetMenu;

    public TMP_Text planetTitle;
    public TMP_Text planetDescription;

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
    }

    public void ClickedStart()
    {
        //Debug.Log("Clicked Start");
        if (startMenuCanvas) ShowPanelFade(startMenuCanvas, false);
        GameManager.Instance.StartVisit();
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
        if (PlanetMenu) ShowPanelFade(PlanetMenu, show);
    }

    public void SetPlanetTitle(string title)
    {
        if (planetTitle) planetTitle.text = title;
    }

    public void SetPlanetInfo(string info)
    {
        if (planetDescription) planetDescription.text = info;
    }
}
