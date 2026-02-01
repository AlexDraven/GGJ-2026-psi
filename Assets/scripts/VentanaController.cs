using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Ventana interactuable: con felicidad > 0.2 muestra "*mejor dejo la ventana cerrada...*";
/// con felicidad <= 0.2 muestra diálogo con opciones; al elegir "tirarse" pantalla negra, sonido y vuelve al MainMenu.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class VentanaController : NpcController
{
    const float HappinessThreshold = 0.2f;
    const string MainMenuSceneName = "MainMenu";

    [Header("Ventana: pantalla negra y sonido")]
    [Tooltip("Overlay negro a pantalla completa. Si no está asignado, se crea en runtime.")]
    [SerializeField] Image blackOverlay;

    [Tooltip("Sonido de impacto/caída al tirarse por la ventana.")]
    [SerializeField] AudioClip impactoCaidaVentana;

    [Tooltip("Imagen 'Moriste' a pantalla completa tras el audio. Si está asignado, se muestra hasta pulsar A (Xbox) o E (teclado).")]
    [SerializeField] Sprite moristeSprite;

    GameObject runtimeBlackOverlay;

    public override void StartDialogue()
    {
        if (GameController.Instance == null)
            return;

        if (GameController.Instance.HappinessLevel > HappinessThreshold)
        {
            if (DialogueManager.Instance == null)
                return;
            DialogueManager.Instance.StartDialogue("", new[] { "*mejor dejo la ventana cerrada...*" }, null, null);
            return;
        }

        base.StartDialogue();
    }

    public override void OnChoiceSelected(int choiceIndex, string chosen)
    {
        if (choiceIndex == 1)
            StartCoroutine(TirarseSequence());
    }

    IEnumerator TirarseSequence()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.Close();

        ShowBlackOverlay();

        if (GameController.Instance != null)
        {
            GameController.Instance.SetInVentanaSequence(true);
            GameController.Instance.AddPsychedelia(-GameController.Instance.PsychedeliaLevel);
            GameController.Instance.AddHappiness(-GameController.Instance.HappinessLevel);
        }

        SoundController.SuppressMusic = true;
        var soundController = FindFirstObjectByType<SoundController>();
        if (soundController != null)
            soundController.StopMusic();

        var ventanaAudio = GetComponent<AudioSource>();
        foreach (var source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (source != ventanaAudio)
                source.Stop();
        }

        yield return new WaitForSeconds(1f);

        var audioSource = ventanaAudio;
        if (audioSource != null && impactoCaidaVentana != null)
            audioSource.PlayOneShot(impactoCaidaVentana);

        float waitTime = (impactoCaidaVentana != null) ? impactoCaidaVentana.length : 1f;
        yield return new WaitForSeconds(waitTime);

        if (moristeSprite != null)
        {
            var overlayImage = GetOverlayImage();
            if (overlayImage != null)
            {
                overlayImage.sprite = moristeSprite;
                overlayImage.color = Color.white;
            }
            while (true)
            {
                if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                    break;
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                    break;
                yield return null;
            }
        }

        SoundController.SuppressMusic = false;
        if (GameController.Instance != null)
        {
            GameController.Instance.SetInVentanaSequence(false);
            GameController.Instance.LoadScene(MainMenuSceneName);
        }
    }

    Image GetOverlayImage()
    {
        if (blackOverlay != null)
            return blackOverlay;
        if (runtimeBlackOverlay != null)
            return runtimeBlackOverlay.GetComponentInChildren<Image>(true);
        return null;
    }

    void ShowBlackOverlay()
    {
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.color = Color.black;
            return;
        }

        if (runtimeBlackOverlay != null)
        {
            runtimeBlackOverlay.SetActive(true);
            return;
        }

        var canvasGo = new GameObject("VentanaBlackOverlay");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        var imageGo = new GameObject("Image");
        imageGo.transform.SetParent(canvasGo.transform, false);
        var rect = imageGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = imageGo.AddComponent<Image>();
        image.color = Color.black;

        runtimeBlackOverlay = canvasGo;
    }
}
