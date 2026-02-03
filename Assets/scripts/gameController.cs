using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    [Header("Scenes")]
    [SerializeField] string gameSceneName = "Escena-1";
    [SerializeField] string mainMenuSceneName = "MainMenu";

    [Header("Pause")]
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] GameObject pauseMenuObject;

    InputAction pauseAction;
    GameState currentState = GameState.Playing;
    public GameState CurrentState => currentState;

    public static GameController Instance { get; private set; }

    [Header("Dialogue")]
    [Tooltip("Última respuesta del diálogo; lo usará el sistema de diálogo cuando se implemente.")]
    [SerializeField] string lastDialogueResponse = "";
    public string LastDialogueResponse { get => lastDialogueResponse; set => lastDialogueResponse = value; }

    bool isInDialogue;
    public bool IsInDialogue => isInDialogue;
    public void SetInDialogue(bool value) { isInDialogue = value; }

    bool isInVentanaSequence;
    public bool IsInVentanaSequence => isInVentanaSequence;
    public void SetInVentanaSequence(bool value) { isInVentanaSequence = value; }

    [Header("Psychedelia")]
    [Tooltip("Nivel de psicodelia del personaje (0-1). Fuente de verdad para el efecto de cámara.")]
    [SerializeField, Range(0f, 1f)] float psychedeliaLevel;
    public float PsychedeliaLevel => psychedeliaLevel;
    public void AddPsychedelia(float delta) { psychedeliaLevel = Mathf.Clamp01(psychedeliaLevel + delta); }

    [Header("Felicidad")]
    [Tooltip("Nivel de felicidad del personaje (0-1). Fuente de verdad para la cara Doom y el audio de felicidad.")]
    [SerializeField, Range(0f, 1f)] float happinessLevel;
    public float HappinessLevel => happinessLevel;
    public void AddHappiness(float delta) { happinessLevel = Mathf.Clamp01(happinessLevel + delta); }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (inputActions != null)
            pauseAction = inputActions.FindActionMap("Player").FindAction("Pause");
    }

    void OnEnable()
    {
        inputActions?.FindActionMap("Player")?.Enable();
    }

    void OnDisable()
    {
        inputActions?.FindActionMap("Player")?.Disable();
    }

    void Start()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        if (pauseMenuObject != null)
            pauseMenuObject.SetActive(false);
    }

    void Update()
    {
        if (pauseAction != null && pauseAction.triggered)
        {
            if (currentState == GameState.Playing)
                Pause();
            else if (currentState == GameState.Paused)
                Resume();
        }
    }

    public void Pause()
    {
        if (currentState != GameState.Playing)
            return;
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        if (pauseMenuObject != null)
            pauseMenuObject.SetActive(true);
    }

    public void Resume()
    {
        if (currentState != GameState.Paused)
            return;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        if (pauseMenuObject != null)
            pauseMenuObject.SetActive(false);
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        // timeScale queda en 1 para que la escena siga animándose; cambiar a 0f si prefieres congelar
    }

    /// <summary>Restaura el estado del juego a valores por defecto (como la primera vez).</summary>
    public void ResetToDefaults()
    {
        lastDialogueResponse = "";
        isInDialogue = false;
        isInVentanaSequence = false;
        psychedeliaLevel = 0f;
        happinessLevel = 0f;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void LoadScene(string sceneName)
    {
        if (sceneName == mainMenuSceneName)
        {
            ResetToDefaults();
            SoundController.SuppressMusic = false;
        }
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>Ejecuta la secuencia de trompada (pantalla negra, sonido, imagen piña). Se ejecuta en GameController para que no se interrumpa si el NPC se desactiva.</summary>
    public void RunPiñaSequence(Sprite piñaSprite, AudioClip sonidoTrompada)
    {
        StartCoroutine(PiñaSequenceCoroutine(piñaSprite, sonidoTrompada));
    }

    IEnumerator PiñaSequenceCoroutine(Sprite piñaSprite, AudioClip sonidoTrompada)
    {
        if (piñaSprite == null)
            yield break;

        var canvasGo = new GameObject("GustaBotPiñaOverlay");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = Color.clear;

        var imageGo = new GameObject("Image");
        imageGo.transform.SetParent(panelGo.transform, false);
        var rect = imageGo.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = imageGo.AddComponent<Image>();
        image.color = Color.black;
        image.preserveAspect = false;

        SetInVentanaSequence(true);

        SoundController.SuppressMusic = true;
        var soundController = FindFirstObjectByType<SoundController>();
        if (soundController != null)
            soundController.StopMusic();
        foreach (var source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            source.Stop();

        yield return new WaitForSeconds(1f);

        if (sonidoTrompada != null)
        {
            Vector3 listenerPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(sonidoTrompada, listenerPos, 1f);
        }

        float waitTime = (sonidoTrompada != null) ? sonidoTrompada.length : 1f;
        yield return new WaitForSeconds(2f);

        image.sprite = piñaSprite;
        image.color = Color.white;
        image.SetAllDirty();
        Canvas.ForceUpdateCanvases();
        yield return null;

        yield return new WaitForSeconds(5f);

        while (true)
        {
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                break;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                break;
            yield return null;
        }

        SoundController.SuppressMusic = false;
        SetInVentanaSequence(false);
        LoadScene(mainMenuSceneName);
        Destroy(canvasGo);
    }

    public void LoadGameScene()
    {
        ResetToDefaults();
        Debug.Log($"[GameController] Cargando escena de juego: {gameSceneName}");
        LoadScene(gameSceneName);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
