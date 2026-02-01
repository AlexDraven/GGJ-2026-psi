#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Crea el piso 20x20 usando Piso.png: material con tiling y Quad bajo Background.
/// Ejecutar con Escena-1 abierta: Tools > Create Piso 20x20.
/// </summary>
public static class CreatePisoFloor
{
    const string MenuPath = "Tools/Create Piso 20x20";
    const string PisoSpritePath = "Assets/sprites/Piso.png";
    const string MaterialsFolder = "Assets/Materials";
    const string MaterialPath = "Assets/Materials/PisoTile.mat";
    const float TileWidth = 127f / 100f;   // 1.27
    const float TileHeight = 128f / 100f;   // 1.28
    const int TilesX = 20;
    const int TilesY = 20;

    [MenuItem(MenuPath, false, 2042)]
    public static void Create()
    {
        Texture2D texture = GetPisoTexture();
        if (texture == null)
        {
            EditorUtility.DisplayDialog("Piso 20x20", "No se encontró la textura de Piso en " + PisoSpritePath, "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        Material mat = CreateOrLoadMaterial(texture);
        if (mat == null)
        {
            EditorUtility.DisplayDialog("Piso 20x20", "No se pudo crear o cargar el material PisoTile.", "OK");
            return;
        }

        Transform background = FindBackground();
        if (background == null)
        {
            EditorUtility.DisplayDialog("Piso 20x20", "No se encontró el GameObject 'Background' en la escena. Abre Escena-1 y vuelve a ejecutar.", "OK");
            return;
        }

        Transform existingPiso = background.Find("Piso");
        if (existingPiso != null)
        {
            if (!EditorUtility.DisplayDialog("Piso 20x20", "Ya existe un hijo 'Piso' bajo Background. ¿Reemplazarlo?", "Sí", "No"))
                return;
            Object.DestroyImmediate(existingPiso.gameObject);
        }

        GameObject pisoGo = CreateQuadWithMaterial(mat);
        pisoGo.name = "Piso";
        pisoGo.transform.SetParent(background, false);
        pisoGo.transform.localPosition = new Vector3(0f, 0f, 0.5f);
        pisoGo.transform.localRotation = Quaternion.identity;
        pisoGo.transform.localScale = new Vector3(TilesX * TileWidth, TilesY * TileHeight, 1f);

        Transform square = background.Find("Square");
        if (square != null)
            square.gameObject.SetActive(false);

        Undo.RegisterCreatedObjectUndo(pisoGo, "Create Piso 20x20");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Piso 20x20", "Piso creado bajo Background con material PisoTile (tiling 20x20). El hijo 'Square' se desactivó.", "OK");
    }

    static Texture2D GetPisoTexture()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PisoSpritePath);
        if (sprite != null && sprite.texture != null)
            return sprite.texture;
        return AssetDatabase.LoadAssetAtPath<Texture2D>(PisoSpritePath);
    }

    static Material CreateOrLoadMaterial(Texture2D texture)
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
            mat.mainTextureScale = new Vector2(TilesX, TilesY);
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
        mat.mainTextureScale = new Vector2(TilesX, TilesY);
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
