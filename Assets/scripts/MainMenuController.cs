using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Créditos")]
    [Tooltip("Panel de créditos (imagen + botón Volver). Asignar en el inspector.")]
    [SerializeField] GameObject creditsPanel;

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

    public void ShowCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }
}
