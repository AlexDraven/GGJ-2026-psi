#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Menú para resolver el error "Can't Generate Mesh, No Font Asset has been assigned".
/// Ejecuta la importación de TMP Essential Resources y muestra los pasos siguientes.
/// </summary>
public static class FixTMPFontAsset
{
    const string k_ImportMenuPath = "Window/TextMeshPro/Import TMP Essential Resources";

    [MenuItem("Tools/Fix TMP Font Asset (Import Essential Resources)", false, 2050)]
    public static void ImportTMPEssentialResources()
    {
        bool executed = EditorApplication.ExecuteMenuItem(k_ImportMenuPath);
        if (executed)
        {
            EditorUtility.DisplayDialog(
                "TMP Essential Resources",
                "Se abrirá el cuadro de importación del paquete.\n\n" +
                "Después de importar:\n" +
                "• Edit > Project Settings > TextMesh Pro > Settings\n" +
                "• Asigna \"Default Font Asset\" (p. ej. LiberationSans SDF).\n\n" +
                "Si ya tienes textos TMP en la escena, selecciónalos y asigna \"Font Asset\" en el Inspector.",
                "Entendido");
        }
        else
        {
            EditorUtility.DisplayDialog(
                "TMP Font Asset",
                "No se pudo abrir el menú de importación.\n\n" +
                "Hazlo manualmente: Window > TextMeshPro > Import TMP Essential Resources.\n\n" +
                "Luego: Edit > Project Settings > TextMesh Pro > Settings > Default Font Asset.",
                "Entendido");
        }
    }

    [MenuItem("Tools/Fix TMP Font Asset (Ver instrucciones)", false, 2051)]
    public static void ShowInstructions()
    {
        bool hasSettings = TMPro.TMP_Settings.instance != null;
        string msg = hasSettings
            ? "TMP Settings encontrado. Si sigues viendo \"No Font Asset\":\n\n" +
              "• Revisa en la Hierarchy objetos con Text - TextMeshPro o Button - TextMeshPro.\n" +
              "• En el Inspector, asigna el campo \"Font Asset\" a cada uno.\n" +
              "• O asigna \"Default Font Asset\" en Edit > Project Settings > TextMesh Pro > Settings."
            : "TMP Essential Resources no están importados.\n\n" +
              "1. Window > TextMeshPro > Import TMP Essential Resources\n" +
              "2. Edit > Project Settings > TextMesh Pro > Settings > Default Font Asset";
        EditorUtility.DisplayDialog("Fix TMP Font Asset", msg, "Entendido");
    }
}
#endif
