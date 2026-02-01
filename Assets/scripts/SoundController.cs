using UnityEngine;

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

    void Start()
    {
        if (audioPsychedelia != null)
            audioPsychedelia.loop = true;
    }

    void Update()
    {
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
    }

    /// <summary>Apaga la música de psicodelia y felicidad (p. ej. antes del sonido de impacto ventana).</summary>
    public void StopMusic()
    {
        if (audioPsychedelia != null)
            audioPsychedelia.Stop();
        if (audioHappiness != null)
            audioHappiness.Stop();
    }
}
