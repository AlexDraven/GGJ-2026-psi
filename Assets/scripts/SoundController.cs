using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lee PsychedeliaLevel y HappinessLevel de GameController y aplica esos valores
/// como volumen a dos AudioSource (psicodelia y felicidad). No modifica los niveles.
/// </summary>
public class SoundController : MonoBehaviour
{
    [Header("Audio por nivel")]
    [Tooltip("Pista cuyo volumen sigue GameController.PsychedeliaLevel (0-1).")]
    [SerializeField] AudioSource audioPsychedelia;

    [Tooltip("Pista cuyo volumen sigue GameController.HappinessLevel (0-1).")]
    [SerializeField] AudioSource audioHappiness;

    [Tooltip("Volumen máximo de las pistas (0-1). El nivel del juego se multiplica por este valor.")]
    [SerializeField] [Range(0f, 1f)] float volumeMax = 0.5f;

    [Header("Ambiente oficina")]
    [Tooltip("Pista Oficina (SIN ruido ambiente); se reproduce solo en juego cuando ambos niveles < 0.5.")]
    [SerializeField] AudioSource audioOficina;
    [Tooltip("Solo reproducir oficina en esta escena (no en menú).")]
    [SerializeField] string gameSceneName = "Escena-1";
    [Tooltip("Volumen de la pista de oficina (0-1).")]
    [SerializeField] [Range(0f, 1f)] float volumeOficina = 0.5f;

    /// <summary>Si true, no se reproduce ninguna pista (p. ej. durante la secuencia de tirarse por la ventana).</summary>
    public static bool SuppressMusic { get; set; }

    void Start()
    {
        if (audioPsychedelia != null)
            audioPsychedelia.loop = true;
        if (audioOficina != null)
            audioOficina.loop = true;
    }

    void Update()
    {
        if (SuppressMusic)
        {
            StopMusic();
            return;
        }
        if (GameController.Instance == null)
            return;

        if (audioPsychedelia != null)
        {
            audioPsychedelia.volume = GameController.Instance.PsychedeliaLevel * volumeMax;
            if (audioPsychedelia.volume > 0f && !audioPsychedelia.isPlaying)
                audioPsychedelia.Play();
        }
        if (audioHappiness != null)
        {
            audioHappiness.volume = GameController.Instance.HappinessLevel * volumeMax;
            if (audioHappiness.volume > 0f && !audioHappiness.isPlaying)
                audioHappiness.Play();
        }

        bool playOficina = SceneManager.GetActiveScene().name == gameSceneName
            && GameController.Instance.CurrentState == GameController.GameState.Playing
            && GameController.Instance.PsychedeliaLevel < 0.5f
            && GameController.Instance.HappinessLevel < 0.5f;
        if (audioOficina != null)
        {
            if (playOficina)
            {
                audioOficina.volume = volumeOficina;
                if (!audioOficina.isPlaying)
                    audioOficina.Play();
            }
            else
                audioOficina.Stop();
        }
    }

    /// <summary>Apaga la música de psicodelia, felicidad y oficina (p. ej. antes del sonido de impacto ventana).</summary>
    public void StopMusic()
    {
        if (audioPsychedelia != null)
            audioPsychedelia.Stop();
        if (audioHappiness != null)
            audioHappiness.Stop();
        if (audioOficina != null)
            audioOficina.Stop();
    }
}
