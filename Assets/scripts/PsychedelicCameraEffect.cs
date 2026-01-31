using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PsychedelicCameraEffect : MonoBehaviour
{
    public static PsychedelicCameraEffect Instance { get; private set; }

    [SerializeField] Shader effectShader;
    [Tooltip("Intensidad enviada al shader cuando PsychedeliaLevel = 1. Valores > 1 hacen el efecto mucho más fuerte al máximo.")]
    [SerializeField] float intensityAtMaxLevel = 2f;
    [SerializeField] Shader happinessColorShader;

    Material material;
    Material happinessMaterial;

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

        if (happinessColorShader == null)
            happinessColorShader = Shader.Find("Hidden/HappinessColorEffect");
        if (happinessColorShader != null)
            happinessMaterial = new Material(happinessColorShader);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        if (material != null)
            Destroy(material);
        if (happinessMaterial != null)
            Destroy(happinessMaterial);
    }

    /// <summary>Redirige a GameController; la intensidad se lee de GameController.PsychedeliaLevel.</summary>
    public void AddIntensity(float amount)
    {
        if (GameController.Instance != null)
            GameController.Instance.AddPsychedelia(amount);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        float happinessLevel = (GameController.Instance != null) ? GameController.Instance.HappinessLevel : 1f;
        RenderTexture happinessSource = source;

        if (happinessMaterial != null)
        {
            RenderTexture tempRT = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
            happinessMaterial.SetFloat("_Happiness", happinessLevel);
            Graphics.Blit(source, tempRT, happinessMaterial);
            happinessSource = tempRT;
        }

        float level = (GameController.Instance != null) ? GameController.Instance.PsychedeliaLevel : 0f;
        float intensity = level * intensityAtMaxLevel;
        if (material == null || intensity <= 0f)
        {
            Graphics.Blit(happinessSource, destination);
        }
        else
        {
            material.SetFloat("_Intensity", intensity);
            Graphics.Blit(happinessSource, destination, material);
        }

        if (happinessSource != source)
            RenderTexture.ReleaseTemporary(happinessSource);
    }
}
