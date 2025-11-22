using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainScene : MonoBehaviour
{
    // Pon aquí el nombre de la escena inicial
    public string mainSceneName = "EscenaPlanetas";

    public void LoadMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}

