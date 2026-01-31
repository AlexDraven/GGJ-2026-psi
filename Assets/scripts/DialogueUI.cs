using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI de diálogo estilo Earthbound: caja de texto, nombre del hablante, lista de opciones y cursor.
/// El DialogueManager actualiza este componente.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] GameObject panelRoot;

    [Header("Texto")]
    [SerializeField] TMP_Text speakerNameText;
    [SerializeField] TMP_Text dialogueText;

    [Header("Opciones")]
    [SerializeField] Transform optionsContainer;
    [SerializeField] RectTransform cursorRect;
    [SerializeField] float cursorOffsetX = -12f;
    [Tooltip("Desplazamiento vertical del cursor respecto a cada opción.")]
    [SerializeField] float cursorOffsetY = 0f;

    RectTransform[] optionRects;

    void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Show()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void SetSpeaker(string name)
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = name;
            speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(name));
        }
    }

    public void SetDialogueText(string text)
    {
        if (dialogueText != null)
            dialogueText.text = text;
    }

    public void ShowOptions(string[] options, int selectedIndex)
    {
        if (optionsContainer == null)
        {
            Debug.LogWarning("[DialogueUI] ShowOptions: optionsContainer es null. Asigna Options Container en el inspector del DialogueBox.");
            return;
        }
        if (options == null || options.Length == 0)
        {
            Debug.LogWarning("[DialogueUI] ShowOptions: options es null o vacío.");
            return;
        }
        optionsContainer.gameObject.SetActive(true);
        optionsContainer.SetAsLastSibling();
        ClearOptions();
        optionRects = new RectTransform[options.Length];

        var font = dialogueText != null ? dialogueText.font : TMP_Settings.defaultFontAsset;
        var fontSize = dialogueText != null ? dialogueText.fontSize : 24f;
        var rowHeight = fontSize * 1.5f;

        for (int i = 0; i < options.Length; i++)
        {
            var rowGo = new GameObject("Option_" + i);
            rowGo.transform.SetParent(optionsContainer, false);

            var rect = rowGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(0, rowHeight);

            var le = rowGo.AddComponent<LayoutElement>();
            le.minHeight = rowHeight;
            le.preferredHeight = rowHeight;

            var tmp = rowGo.AddComponent<TextMeshProUGUI>();
            tmp.text = options[i];
            tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;

            optionRects[i] = rect;
        }

        var containerRect = optionsContainer as RectTransform;
        if (containerRect != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }

        if (cursorRect != null)
        {
            cursorRect.gameObject.SetActive(true);
            SetSelectedIndex(selectedIndex);
        }
    }

    public void SetSelectedIndex(int index)
    {
        if (optionRects == null || index < 0 || index >= optionRects.Length || cursorRect == null)
            return;

        var containerRect = optionsContainer as RectTransform;
        if (containerRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

        // index = choiceSelectedIndex del DialogueManager: 0 = primera opción (arriba), 1 = segunda, etc.
        var optionRect = optionRects[index];
        cursorRect.SetParent(optionsContainer, true);
        // Misma ancla que las opciones (arriba) para usar el mismo sistema de referencia
        cursorRect.anchorMin = new Vector2(0, 1f);
        cursorRect.anchorMax = new Vector2(0, 1f);
        cursorRect.pivot = new Vector2(0, 1f);
        // Centro de la fila (desde arriba): top - altura/2. Con pivot en top, para centrar el cursor: top del cursor = centro fila + altura cursor/2.
        float rowCenterY = optionRect.anchoredPosition.y - optionRect.rect.height * 0.5f;
        float cursorTopY = rowCenterY + cursorRect.rect.height * 0.5f;
        cursorRect.anchoredPosition = new Vector2(cursorOffsetX, cursorTopY + cursorOffsetY);
        cursorRect.SetAsLastSibling();
    }

    public void HideOptions()
    {
        ClearOptions();
        if (cursorRect != null)
            cursorRect.gameObject.SetActive(false);
    }

    void ClearOptions()
    {
        if (optionsContainer == null)
            return;
        for (int i = optionsContainer.childCount - 1; i >= 0; i--)
        {
            var child = optionsContainer.GetChild(i);
            if (cursorRect != null && child == cursorRect)
                continue;
            Destroy(child.gameObject);
        }
        optionRects = null;
    }
}
