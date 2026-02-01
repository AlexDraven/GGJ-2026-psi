#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Crea el piso con baldosas repetidas (Piso.png). Cada baldosa no supera el tamaño del personaje.
/// Tools > Create Piso 24x24 o Create Piso 80x40 (con Escena-1 abierta).
/// </summary>
public static class CreatePisoFloor
{
    const string MenuPath24 = "Tools/Create Piso 24x24";
    const string MenuPath80x40 = "Tools/Create Piso 80x40";
    const string PisoSpritePath = "Assets/sprites/Piso.png";
    const string MaterialsFolder = "Assets/Materials";
    const string MaterialPath = "Assets/Materials/PisoTile.mat";
    const float TileSizeWorld = 1f;
    const float FloorWidth24 = 24f;
    const float FloorHeight24 = 24f;

    [MenuItem(MenuPath24, false, 2042)]
    public static void Create24x24() => CreateWithSize(FloorWidth24, FloorHeight24);

    [MenuItem(MenuPath80x40, false, 2043)]
    public static void Create80x40() => CreateWithSize(80f, 40f);

    static void CreateWithSize(float floorWidth, float floorHeight)
    {
        int tilesX = Mathf.Max(1, Mathf.RoundToInt(floorWidth / TileSizeWorld));
        int tilesY = Mathf.Max(1, Mathf.RoundToInt(floorHeight / TileSizeWorld));
        string title = $"Piso {tilesX}x{tilesY}";

        Texture2D texture = GetPisoTexture();
        if (texture == null)
        {
            EditorUtility.DisplayDialog(title, "No se encontró la textura de Piso en " + PisoSpritePath, "OK");
            return;
        }

        EnsureTextureRepeats(PisoSpritePath);

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        Material mat = CreateOrLoadMaterial(texture, tilesX, tilesY);
        if (mat == null)
        {
            EditorUtility.DisplayDialog(title, "No se pudo crear o cargar el material PisoTile.", "OK");
            return;
        }

        Transform background = FindBackground();
        if (background == null)
        {
            EditorUtility.DisplayDialog(title, "No se encontró el GameObject 'Background' en la escena. Abre Escena-1 y vuelve a ejecutar.", "OK");
            return;
        }

        Transform existingPiso = background.Find("Piso");
        if (existingPiso != null)
        {
            if (!EditorUtility.DisplayDialog(title, "Ya existe un hijo 'Piso' bajo Background. ¿Reemplazarlo?", "Sí", "No"))
                return;
            Object.DestroyImmediate(existingPiso.gameObject);
        }

        GameObject pisoGo = CreateQuadWithMaterial(mat);
        pisoGo.name = "Piso";
        pisoGo.transform.SetParent(background, false);
        pisoGo.transform.localPosition = new Vector3(0f, 0f, 0.5f);
        pisoGo.transform.localRotation = Quaternion.identity;
        pisoGo.transform.localScale = new Vector3(floorWidth, floorHeight, 1f);

        Transform square = background.Find("Square");
        if (square != null)
            square.gameObject.SetActive(false);

        Undo.RegisterCreatedObjectUndo(pisoGo, title);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog(title, $"Piso creado: {tilesX}x{tilesY} baldosas de {TileSizeWorld} unidad(es). El hijo 'Square' se desactivó.", "OK");
    }

    static void EnsureTextureRepeats(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;
        if (importer.wrapModeU == TextureWrapMode.Repeat && importer.wrapModeV == TextureWrapMode.Repeat) return;
        importer.wrapModeU = TextureWrapMode.Repeat;
        importer.wrapModeV = TextureWrapMode.Repeat;
        importer.SaveAndReimport();
    }

    static Texture2D GetPisoTexture()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PisoSpritePath);
        if (sprite != null && sprite.texture != null)
            return sprite.texture;
        return AssetDatabase.LoadAssetAtPath<Texture2D>(PisoSpritePath);
    }

    static Material CreateOrLoadMaterial(Texture2D texture, int tilesX, int tilesY)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat != null)
        {
            Shader s = mat.shader;
            if (s == null || !s.isSupported)
            {
                s = Shader.Find("Unlit/Texture");
                if (s == null) s = Shader.Find("Sprites/Default");
                if (s != null) mat.shader = s;
            }
            mat.mainTexture = texture;
            mat.mainTextureScale = new Vector2(tilesX, tilesY);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }

        Shader shader = Shader.Find("Unlit/Texture");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
        if (shader == null)
            return null;

        mat = new Material(shader);
        mat.mainTexture = texture;
        mat.mainTextureScale = new Vector2(tilesX, tilesY);
        AssetDatabase.CreateAsset(mat, MaterialPath);
        return mat;
    }

    static Transform FindBackground()
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root.name == "Background")
                return root.transform;
        }
        return null;
    }

    static GameObject CreateQuadWithMaterial(Material mat)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.DestroyImmediate(quad.GetComponent<Collider>());
        var renderer = quad.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return quad;
    }
}
#endif
