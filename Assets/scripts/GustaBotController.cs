using UnityEngine;

/// <summary>
/// Jefe GustaBot: si psicodelia > 0.8 muestra diálogo especial con dos opciones;
/// si no, usa el diálogo normal del inspector.
/// </summary>
public class GustaBotController : NpcController, IOnDialogueClosed
{
    const float PsychedeliaThreshold = 0.8f;

    static readonly string[] AlternateDialogueLines = new[]
    {
        "Que pasa te pasa GuillerMo Nitor? No me gusta tu actitud. Cuando termines tu trabajo tenemos que hablar..."
    };

    static readonly string[] AlternateChoices = new[]
    {
        "Si jefecito, cuando termine mi trabajo",
        "*Pegarle una trompada*"
    };

    [Header("GustaBot: diálogo alternativo (psicodelia > 0.8)")]
    [Tooltip("Delta de psicodelia por opción del diálogo alternativo: [0] Si jefecito, [1] Trompada.")]
    [SerializeField] float[] alternatePsychedeliaDeltas = new float[] { -0.1f, 0.1f };
    [Tooltip("Delta de felicidad por opción del diálogo alternativo: [0] Si jefecito, [1] Trompada.")]
    [SerializeField] float[] alternateHappinessDeltas = new float[] { 0f, 0.1f };
    [Tooltip("Sonido al elegir *Pegarle una trompada*.")]
    [SerializeField] AudioClip sonidoTrompada;

    bool alternateDialogueActive;

    public override void StartDialogue()
    {
        if (GameController.Instance == null)
        {
            base.StartDialogue();
            return;
        }

        if (GameController.Instance.PsychedeliaLevel > PsychedeliaThreshold)
        {
            if (DialogueManager.Instance == null)
                return;
            alternateDialogueActive = true;
            DialogueManager.Instance.StartDialogue(
                "Jefe GustaBot",
                AlternateDialogueLines,
                AlternateChoices,
                this);
            return;
        }

        base.StartDialogue();
    }

    public override float GetPsychedeliaDeltaForChoice(int choiceIndex)
    {
        if (alternateDialogueActive && alternatePsychedeliaDeltas != null
            && choiceIndex >= 0 && choiceIndex < alternatePsychedeliaDeltas.Length)
            return alternatePsychedeliaDeltas[choiceIndex];
        return base.GetPsychedeliaDeltaForChoice(choiceIndex);
    }

    public override float GetHappinessDeltaForChoice(int choiceIndex)
    {
        if (alternateDialogueActive && alternateHappinessDeltas != null
            && choiceIndex >= 0 && choiceIndex < alternateHappinessDeltas.Length)
            return alternateHappinessDeltas[choiceIndex];
        return base.GetHappinessDeltaForChoice(choiceIndex);
    }

    public override string[] GetResponseLinesAfterChoice(int choiceIndex)
    {
        if (alternateDialogueActive)
        {
            if (choiceIndex == 1)
                return new[] { "*¡POW!*" };
            return null;
        }
        return base.GetResponseLinesAfterChoice(choiceIndex);
    }

    public override void OnChoiceSelected(int choiceIndex, string chosen)
    {
        base.OnChoiceSelected(choiceIndex, chosen);
        if (choiceIndex == 1 && sonidoTrompada != null)
            AudioSource.PlayClipAtPoint(sonidoTrompada, transform.position, 1f);
    }

    public void OnDialogueClosed()
    {
        alternateDialogueActive = false;
    }
}
