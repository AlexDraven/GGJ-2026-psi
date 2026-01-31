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

    bool playerInRange;

    /// <summary>Devuelve el delta de psicodelia para la opción elegida. 0 si no hay datos.</summary>
    public float GetPsychedeliaDeltaForChoice(int choiceIndex)
    {
        if (psychedeliaDeltas == null || choiceIndex < 0 || choiceIndex >= psychedeliaDeltas.Length)
            return 0f;
        return psychedeliaDeltas[choiceIndex];
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

    public void StartDialogue()
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
