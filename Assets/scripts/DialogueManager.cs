using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestiona el diálogo estilo Earthbound: typewriter, avanzar línea y opciones con joystick.
/// Singleton; el PlayerController llama a Advance() al pulsar Interact.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] DialogueUI dialogueUI;

    [Header("Typewriter")]
    [SerializeField] float secondsPerCharacter = 0.03f;

    [Header("Input")]
    [SerializeField] InputActionAsset inputActions;

    string[] lines;
    string[] choices;
    string speakerName;
    int lineIndex;
    int choiceSelectedIndex;
    bool typewriterRunning;
    Coroutine typewriterCoroutine;
   NpcController dialogueOwner;

    InputAction navigateAction;
    float lastNavigateY;

    public bool IsInDialogue { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (inputActions != null)
            navigateAction = inputActions.FindActionMap("UI")?.FindAction("Navigate");
    }

    void Update()
    {
        if (!IsInDialogue || dialogueUI == null || choices == null || choices.Length == 0)
            return;

        if (navigateAction != null)
        {
            var move = navigateAction.ReadValue<Vector2>();
            if (move.y > 0.3f && lastNavigateY <= 0.3f)
            {
                choiceSelectedIndex = Mathf.Max(0, choiceSelectedIndex - 1);
                dialogueUI.SetSelectedIndex(choiceSelectedIndex);
            }
            else if (move.y < -0.3f && lastNavigateY >= -0.3f)
            {
                choiceSelectedIndex = Mathf.Min(choices.Length - 1, choiceSelectedIndex + 1);
                dialogueUI.SetSelectedIndex(choiceSelectedIndex);
            }
            lastNavigateY = move.y;
        }
    }

    /// <summary>
    /// Inicia un diálogo lineal (solo líneas) o con opciones al final.
    /// </summary>
    /// <param name="speaker">Nombre del hablante (opcional).</param>
    /// <param name="dialogueLines">Líneas de texto en orden.</param>
    /// <param name="dialogueChoices">Opciones al final (null = sin opciones).</param>
    /// <param name="owner">NPC que inicia el diálogo (opcional); se usa para activar efecto psicodélico según opción configurada.</param>
    public void StartDialogue(string speaker, string[] dialogueLines, string[] dialogueChoices = null, NpcController owner = null)
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
            return;

        if (dialogueUI == null)
        {
            dialogueUI = FindFirstObjectByType<DialogueUI>();
            if (dialogueUI == null)
            {
                Debug.LogWarning("[DialogueManager] No se encontró DialogueUI en la escena. Ejecuta Tools > Create Dialogue UI en la escena de juego y guarda.");
                return;
            }
        }

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        speakerName = speaker ?? "";
        lines = dialogueLines;
        choices = dialogueChoices != null && dialogueChoices.Length > 0 ? dialogueChoices : null;
        dialogueOwner = owner;
        lineIndex = 0;
        choiceSelectedIndex = 0;
        typewriterRunning = false;

        IsInDialogue = true;
        lastNavigateY = 0f;
        if (GameController.Instance != null)
            GameController.Instance.SetInDialogue(true);
        inputActions?.FindActionMap("UI")?.Enable();

        dialogueUI.SetSpeaker(speakerName);
        dialogueUI.Show();
        dialogueUI.HideOptions();
        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        if (lines == null || lineIndex >= lines.Length)
        {
            if (choices != null && choices.Length > 0)
                ShowChoices();
            else
                Close();
            return;
        }

        var fullText = lines[lineIndex];
        dialogueUI.SetDialogueText("");
        typewriterRunning = true;
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(fullText));
    }

    IEnumerator TypewriterRoutine(string fullText)
    {
        if (dialogueUI == null)
            yield break;

        for (int i = 0; i <= fullText.Length; i++)
        {
            if (!IsInDialogue)
                yield break;
            dialogueUI.SetDialogueText(fullText.Substring(0, i));
            if (i < fullText.Length)
                yield return new WaitForSeconds(secondsPerCharacter);
        }

        typewriterRunning = false;
        typewriterCoroutine = null;

        if (lineIndex == lines.Length - 1 && choices != null && choices.Length > 0)
        {
            lineIndex++;
            ShowCurrentLine();
        }
    }

    void ShowChoices()
    {
        Debug.Log("[DialogueManager] Mostrando opciones: " + (choices != null ? choices.Length : 0));
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowOptions(choices, choiceSelectedIndex);
    }

    /// <summary>
    /// Avanza el diálogo: completa typewriter, siguiente línea o confirma opción.
    /// Lo llama PlayerController al pulsar Interact.
    /// </summary>
    public void Advance()
    {
        if (!IsInDialogue || dialogueUI == null)
            return;

        if (choices != null && choices.Length > 0 && !typewriterRunning)
        {
            var chosen = choices[choiceSelectedIndex];
            if (GameController.Instance != null)
                GameController.Instance.LastDialogueResponse = chosen;
            if (dialogueOwner != null && dialogueOwner.ChoiceIndexThatTriggersEffect == choiceSelectedIndex)
            {
                if (PsychedelicCameraEffect.Instance != null)
                    PsychedelicCameraEffect.Instance.AddIntensity(0.2f);
            }
            Close();
            return;
        }

        if (typewriterRunning && typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            typewriterRunning = false;
            if (lines != null && lineIndex < lines.Length)
                dialogueUI.SetDialogueText(lines[lineIndex]);
            // Si es la última línea y hay opciones, avanzar para mostrarlas
            if (lineIndex == lines.Length - 1 && choices != null && choices.Length > 0)
            {
                lineIndex++;
                ShowCurrentLine();
            }
            return;
        }

        lineIndex++;
        ShowCurrentLine();
    }

    public void Close()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        typewriterRunning = false;
        IsInDialogue = false;
        if (GameController.Instance != null)
            GameController.Instance.SetInDialogue(false);
        inputActions?.FindActionMap("UI")?.Disable();

        if (dialogueUI != null)
        {
            dialogueUI.HideOptions();
            dialogueUI.Hide();
        }
    }
}
