using UnityEngine;

public class NpcController : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] string speakerName = "";
    [SerializeField, TextArea(2, 6)] string dialogueText = "";
    [Tooltip("Opciones al final del diálogo (dejar vacío = solo texto).")]
    [SerializeField] string[] dialogueChoices;
    [Tooltip("Delta de psicodelia por opción (índice 0 = primera opción). Ej: +0.2, -0.1, 0. Mismo tamaño que Dialogue Choices.")]
    [SerializeField] float[] psychedeliaDeltas;
    [Tooltip("Delta de felicidad por opción (índice 0 = primera opción). Ej: +0.1, -0.05, 0. Mismo tamaño que Dialogue Choices.")]
    [SerializeField] float[] happinessDeltas;
    [Tooltip("Texto de respuesta del NPC/objeto por opción (índice 0 = primera opción). Varias líneas = separar con Enter. Vacío = no hay respuesta, se cierra el diálogo.")]
    [SerializeField] string[] responseAfterChoice;
    [Tooltip("Opcional: sonido a reproducir al elegir cada opción (índice 0 = primera opción). Ej: VOZ Comiendo para la opción de comer yogurt.")]
    [SerializeField] AudioClip[] audioOnChoice;
    [Tooltip("Si true, se reproduce VOZ - Dialogo mientras este NPC habla (solo personajes, no objetos).")]
    [SerializeField] bool playVoiceInDialogue;

    bool playerInRange;

    /// <summary>True si el DialogueManager debe reproducir VOZ - Dialogo mientras este NPC habla.</summary>
    public bool PlayVoiceInDialogue => playVoiceInDialogue;

    /// <summary>Devuelve las líneas de respuesta para la opción elegida, o null si no hay.</summary>
    public string[] GetResponseLinesAfterChoice(int choiceIndex)
    {
        if (responseAfterChoice == null || choiceIndex < 0 || choiceIndex >= responseAfterChoice.Length)
            return null;
        string s = responseAfterChoice[choiceIndex];
        if (string.IsNullOrWhiteSpace(s))
            return null;
        return s.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>Devuelve el delta de psicodelia para la opción elegida. 0 si no hay datos.</summary>
    public float GetPsychedeliaDeltaForChoice(int choiceIndex)
    {
        if (psychedeliaDeltas == null || choiceIndex < 0 || choiceIndex >= psychedeliaDeltas.Length)
            return 0f;
        return psychedeliaDeltas[choiceIndex];
    }

    /// <summary>Devuelve el delta de felicidad para la opción elegida. 0 si no hay datos.</summary>
    public float GetHappinessDeltaForChoice(int choiceIndex)
    {
        if (happinessDeltas == null || choiceIndex < 0 || choiceIndex >= happinessDeltas.Length)
            return 0f;
        return happinessDeltas[choiceIndex];
    }

    /// <summary>Llamado por DialogueManager al confirmar una opción. Override en herederos (p. ej. VentanaController) para reaccionar.</summary>
    public virtual void OnChoiceSelected(int choiceIndex, string chosen)
    {
        if (audioOnChoice != null && choiceIndex >= 0 && choiceIndex < audioOnChoice.Length && audioOnChoice[choiceIndex] != null)
            AudioSource.PlayClipAtPoint(audioOnChoice[choiceIndex], transform.position, 1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;
        playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;
        playerInRange = false;
    }

    static bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponent<PlayerController>() != null;
    }

    public bool PlayerInRange => playerInRange;

    public virtual void StartDialogue()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[NpcController] DialogueManager.Instance es null. ¿Hay un DialogueManager en la escena? Ejecuta Tools > Create Dialogue UI en la escena de juego.");
            return;
        }
        var lines = string.IsNullOrWhiteSpace(dialogueText)
            ? new string[0]
            : dialogueText.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            Debug.LogWarning("[NpcController] Dialogue Text está vacío. Escribe al menos una línea en el inspector.");
            return;
        }
        if (dialogueChoices == null || dialogueChoices.Length == 0)
            Debug.LogWarning("[NpcController] Dialogue Choices está vacío. Para mostrar opciones de respuesta, asigna Size ≥ 1 y rellena los elementos en el inspector.");
        DialogueManager.Instance.StartDialogue(speakerName, lines, dialogueChoices, this);
    }
}
