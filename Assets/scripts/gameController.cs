using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadGameScene()
    {
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
