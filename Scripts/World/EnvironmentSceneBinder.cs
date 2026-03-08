using UnityEngine;
using UnityEngine.UI;

public class EnvironmentSceneBinder : MonoBehaviour
{
    [Header("Art")]
    [SerializeField] private EnvironmentArtLibrary environmentArt;

    [Header("Scene Renderers (SpriteRenderer)")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private SpriteRenderer tableRenderer;

    [Header("UI Renderers (optional if you use Canvas)")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image tableImage;

    [Header("Mood Lighting")]
    [SerializeField] private Light sceneLight;
    [SerializeField] private Color baseLightColor = new Color(0.62f, 0.56f, 0.5f, 1f);
    [SerializeField] private Color dangerLightColor = new Color(0.82f, 0.18f, 0.18f, 1f);
    [SerializeField] private float baseIntensity = 0.65f;
    [SerializeField] private float pulseIntensity = 0.95f;
    [SerializeField] private float pulseSpeed = 1.8f;
    [SerializeField] private float flickerNoise = 0.18f;

    [Header("Optional risk source")]
    [SerializeField] private SawRigController sawRig;

    private void Awake()
    {
        ApplyEnvironmentSprites();
    }

    private void OnEnable()
    {
        ApplyEnvironmentSprites();
    }

    private void Start()
    {
        ApplyEnvironmentSprites();
    }

    private void Update()
    {
        if (sceneLight == null)
        {
            return;
        }

        float risk = 0f;
        if (sawRig != null)
        {
            float total = Mathf.Max(0.001f, sawRig.DistanceToPlayer + sawRig.DistanceToOpponent);
            float distanceToSafeCenter = Mathf.Abs(sawRig.DistanceToPlayer - (total * 0.5f));
            risk = 1f - Mathf.Clamp01(distanceToSafeCenter / (total * 0.5f));
        }

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
        float noise = Mathf.PerlinNoise(Time.time * pulseSpeed * 1.37f, 0f);
        float flicker = Mathf.Lerp(1f - flickerNoise, 1f, noise);

        sceneLight.color = Color.Lerp(baseLightColor, dangerLightColor, risk);
        sceneLight.intensity = Mathf.Lerp(baseIntensity, pulseIntensity, pulse * risk) * flicker;
    }

    [ContextMenu("Apply Environment Sprites")]
    public void ApplyEnvironmentSprites()
    {
        if (environmentArt == null)
        {
            Debug.LogWarning("EnvironmentSceneBinder: environmentArt is not assigned.", this);
            return;
        }

        Sprite background = environmentArt.GetBackgroundSprite();
        Sprite table = environmentArt.GetTableSprite();

        if (background == null)
        {
            Debug.LogWarning("EnvironmentSceneBinder: background sprite is null. Check Resources/Environment/background.", this);
        }

        if (table == null)
        {
            Debug.LogWarning("EnvironmentSceneBinder: table sprite is null. Check Resources/Environment/table.", this);
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = background;
        }

        if (tableRenderer != null)
        {
            tableRenderer.sprite = table;
        }

        if (backgroundImage != null)
        {
            backgroundImage.sprite = background;
            backgroundImage.SetNativeSize();
        }

        if (tableImage != null)
        {
            tableImage.sprite = table;
            tableImage.SetNativeSize();
        }
    }
}
