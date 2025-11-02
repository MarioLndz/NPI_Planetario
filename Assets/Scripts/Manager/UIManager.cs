using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class UIManager : MonoBehaviour
{
    // --- Singleton ---
    public static UIManager Instance { get; private set; }
     

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject startMenuCanvas;
    public GameObject PlanetMenu;

    public TMP_Text planetTitle;

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
        if (startMenuCanvas) startMenuCanvas.SetActive(false);
        GameManager.Instance.ToggleBackgroundBlur();
    }

    public void ShowPlanetPanel(bool show = true)
    {
        if (PlanetMenu) PlanetMenu.SetActive(show);
    }

    public void SetPlanetTitle(string title)
    {
        if (planetTitle) planetTitle.text = title;
    }
}
