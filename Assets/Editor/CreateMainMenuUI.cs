#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Añade Canvas, EventSystem y botón "Jugar" a la escena MainMenu y conecta el OnClick.
/// Ejecutar con la escena MainMenu abierta: Tools > Setup Main Menu UI.
/// </summary>
public static class CreateMainMenuUI
{
    const string MenuPath = "Tools/Setup Main Menu UI";

    [MenuItem(MenuPath, false, 2040)]
    public static void SetupMainMenuUI()
    {
        if (EventSystem.current == null)
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystemGo, "Create EventSystem");
        }

        Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            if (existingCanvas.transform.Find("BtnPlay") != null)
            {
                EditorUtility.DisplayDialog("Main Menu UI", "El Canvas y el botón Jugar ya existen en la escena.", "OK");
                return;
            }
        }
        else
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
        }

        Canvas canvasToUse = Object.FindFirstObjectByType<Canvas>();
        if (canvasToUse == null)
            return;

        var btnGo = new GameObject("BtnPlay");
        btnGo.transform.SetParent(canvasToUse.transform, false);

        var rect = btnGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(160, 30);

        var image = btnGo.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        var button = btnGo.AddComponent<Button>();

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(btnGo.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var text = textGo.AddComponent<Text>();
        text.text = "Jugar";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        var menuController = Object.FindFirstObjectByType<MainMenuController>();
        if (menuController != null)
        {
            var onClick = new Button.ButtonClickedEvent();
            onClick.AddListener(menuController.Play);
            button.onClick = onClick;
        }

        Undo.RegisterCreatedObjectUndo(btnGo, "Create BtnPlay");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Main Menu UI", "Se añadieron Canvas, EventSystem y botón \"Jugar\". El OnClick está conectado a MainMenuController.Play.\n\nGuarda la escena (Ctrl+S) para conservar los cambios.", "OK");
    }
}
#endif
