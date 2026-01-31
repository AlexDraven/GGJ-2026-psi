using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PsychedelicCameraEffect : MonoBehaviour
{
    public static PsychedelicCameraEffect Instance { get; private set; }

    [SerializeField] Shader effectShader;

    Material material;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (effectShader == null)
            effectShader = Shader.Find("Hidden/PsychedelicEffect");
        if (effectShader != null)
            material = new Material(effectShader);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        if (material != null)
            Destroy(material);
    }

    /// <summary>Redirige a GameController; la intensidad se lee de GameController.PsychedeliaLevel.</summary>
    public void AddIntensity(float amount)
    {
        if (GameController.Instance != null)
            GameController.Instance.AddPsychedelia(amount);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        float intensity = (GameController.Instance != null) ? GameController.Instance.PsychedeliaLevel : 0f;
        if (material == null || intensity <= 0f)
        {
            Graphics.Blit(source, destination);
            return;
        }
        material.SetFloat("_Intensity", intensity);
        Graphics.Blit(source, destination, material);
    }
}
