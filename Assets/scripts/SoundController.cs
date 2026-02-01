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
            audioPsychedelia.volume = GameController.Instance.PsychedeliaLevel;

        if (audioHappiness != null)
            audioHappiness.volume = GameController.Instance.HappinessLevel;
    }
}
