#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

/// <summary>
/// Crea la UI de diálogo estilo Earthbound en la escena: Canvas, panel inferior, TMP, opciones y cursor.
/// Ejecutar con la escena de juego abierta: Tools > Create Dialogue UI.
/// </summary>
public static class CreateDialogueUI
{
    const string MenuPath = "Tools/Create Dialogue UI";

    static TMP_FontAsset GetDefaultFont()
    {
        return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
    }

    [MenuItem(MenuPath, false, 2041)]
    public static void Create()
    {
        if (EventSystem.current == null)
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystemGo, "Create EventSystem");
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
        }

        if (canvas.transform.Find("DialogueBox") != null)
        {
            EditorUtility.DisplayDialog("Dialogue UI", "DialogueBox ya existe en la escena.", "OK");
            return;
        }

        var font = GetDefaultFont();
        if (font == null)
        {
            EditorUtility.DisplayDialog("Dialogue UI", "No se encontró la fuente TMP 'LiberationSans SDF' en Resources.", "OK");
            return;
        }

        var panelGo = new GameObject("DialogueBox");
        panelGo.transform.SetParent(canvas.transform, false);

        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0f);
        panelRect.anchorMax = new Vector2(0.9f, 0.35f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        var padding = 16f;

        var speakerGo = new GameObject("SpeakerName");
        speakerGo.transform.SetParent(panelGo.transform, false);
        var speakerRect = speakerGo.AddComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0, 1);
        speakerRect.anchorMax = new Vector2(1, 1);
        speakerRect.pivot = new Vector2(0, 1);
        speakerRect.anchoredPosition = new Vector2(padding, -padding);
        speakerRect.sizeDelta = new Vector2(-padding * 2, 24);
        var speakerTmp = speakerGo.AddComponent<TextMeshProUGUI>();
        speakerTmp.font = font;
        speakerTmp.fontSize = 18;
        speakerTmp.color = new Color(0.9f, 0.85f, 0.6f);
        speakerTmp.text = "";

        var textGo = new GameObject("DialogueText");
        textGo.transform.SetParent(panelGo.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(padding, padding);
        textRect.offsetMax = new Vector2(-padding, -40);
        var dialogueTmp = textGo.AddComponent<TextMeshProUGUI>();
        dialogueTmp.font = font;
        dialogueTmp.fontSize = 22;
        dialogueTmp.color = Color.white;
        dialogueTmp.text = "";
        dialogueTmp.textWrappingMode = TMPro.TextWrappingModes.Normal;

        var optionsGo = new GameObject("OptionsContainer");
        optionsGo.transform.SetParent(panelGo.transform, false);
        var optionsRect = optionsGo.AddComponent<RectTransform>();
        optionsRect.anchorMin = new Vector2(0, 0);
        optionsRect.anchorMax = new Vector2(1, 0);
        optionsRect.pivot = new Vector2(0, 0);
        optionsRect.anchoredPosition = new Vector2(padding, padding);
        optionsRect.sizeDelta = new Vector2(-padding * 2, 80);
        var layout = optionsGo.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.spacing = 4;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        var cursorGo = new GameObject("Cursor");
        cursorGo.transform.SetParent(optionsGo.transform, false);
        var cursorRect = cursorGo.AddComponent<RectTransform>();
        cursorRect.anchorMin = new Vector2(0, 0.5f);
        cursorRect.anchorMax = new Vector2(0, 0.5f);
        cursorRect.pivot = new Vector2(0, 0.5f);
        cursorRect.sizeDelta = new Vector2(20, 24);
        cursorRect.anchoredPosition = new Vector2(-12, 30);
        var cursorLE = cursorGo.AddComponent<LayoutElement>();
        cursorLE.ignoreLayout = true;
        var cursorTmp = cursorGo.AddComponent<TextMeshProUGUI>();
        cursorTmp.font = font;
        cursorTmp.fontSize = 22;
        cursorTmp.color = Color.yellow;
        cursorTmp.text = ">";
        cursorGo.SetActive(false);

        var dialogueUI = panelGo.AddComponent<DialogueUI>();
        var so = new SerializedObject(dialogueUI);
        so.FindProperty("panelRoot").objectReferenceValue = panelGo;
        so.FindProperty("speakerNameText").objectReferenceValue = speakerTmp;
        so.FindProperty("dialogueText").objectReferenceValue = dialogueTmp;
        so.FindProperty("optionsContainer").objectReferenceValue = optionsGo.transform;
        so.FindProperty("cursorRect").objectReferenceValue = cursorRect;
        so.ApplyModifiedPropertiesWithoutUndo();

        var dialogueManager = Object.FindFirstObjectByType<DialogueManager>();
        if (dialogueManager == null)
        {
            var dmGo = new GameObject("DialogueManager");
            dialogueManager = dmGo.AddComponent<DialogueManager>();
            Undo.RegisterCreatedObjectUndo(dmGo, "Create DialogueManager");
        }
        var dmSo = new SerializedObject(dialogueManager);
        dmSo.FindProperty("dialogueUI").objectReferenceValue = dialogueUI;
        var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (inputAsset != null)
            dmSo.FindProperty("inputActions").objectReferenceValue = inputAsset;
        dmSo.ApplyModifiedPropertiesWithoutUndo();

        Undo.RegisterCreatedObjectUndo(panelGo, "Create DialogueBox");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Dialogue UI", "Se creó DialogueBox y DialogueManager. Asigna InputActions al DialogueManager si no se asignó automáticamente.", "OK");
    }
}
#endif
