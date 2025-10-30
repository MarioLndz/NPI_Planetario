using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Manager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject startMenuCanvas;
    public GameObject PlanetMenu;

    public TMP_Text planetTitle;

    void Awake()
    {
        // Panel oculto desde el principio
        if (startMenuCanvas) startMenuCanvas.SetActive(true);
        if (PlanetMenu) PlanetMenu.SetActive(false);
    }

    public void ClickedStart()
    {
        //Debug.Log("Clicked Start");
        if (startMenuCanvas) startMenuCanvas.SetActive(false);
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
