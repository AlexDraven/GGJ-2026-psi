using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void Play()
    {
        Debug.Log("[MainMenu] Botón Jugar clicado.");
        if (GameController.Instance == null)
        {
            Debug.LogWarning("[MainMenu] GameController.Instance es null; no se puede cargar la escena.");
            return;
        }
        GameController.Instance.LoadGameScene();
    }
}
