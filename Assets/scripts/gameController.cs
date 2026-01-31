using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class gameController : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    [Header("Pause")]
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] GameObject pauseMenuObject;

    InputAction pauseAction;
    GameState currentState = GameState.Playing;
    public GameState CurrentState => currentState;

    void Awake()
    {
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
