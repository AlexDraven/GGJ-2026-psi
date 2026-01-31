using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PsychedelicCameraEffect : MonoBehaviour
{
    public static PsychedelicCameraEffect Instance { get; private set; }

    [SerializeField] Shader effectShader;
    [SerializeField] float intensityPerHit = 0.2f;
    [SerializeField] float decaySpeed = 0.2f;
    [SerializeField] bool useDecay = true;

    Material material;
    float intensity;

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

    void Update()
    {
        if (!useDecay || material == null)
            return;
        intensity -= decaySpeed * Time.deltaTime;
        intensity = Mathf.Max(0f, intensity);
    }

    public void AddIntensity(float amount)
    {
        intensity = Mathf.Min(1f, intensity + amount);
    }

    public void AddIntensity()
    {
        AddIntensity(intensityPerHit);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null || intensity <= 0f)
        {
            Graphics.Blit(source, destination);
            return;
        }
        material.SetFloat("_Intensity", intensity);
        Graphics.Blit(source, destination, material);
    }
}
