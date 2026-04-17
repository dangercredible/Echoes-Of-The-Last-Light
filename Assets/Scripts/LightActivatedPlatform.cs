using UnityEngine;

public class LightActivatedPlatform : MonoBehaviour, ILightReactive
{
    [Header("References")]
    public Collider2D solidCollider;
    public SpriteRenderer visual;

    [Header("Behavior")]
    public bool startsEnabled;
    public bool staysEnabledAfterFirstLight;
    public bool IsIlluminated { get; private set; }
    private bool activatedOnce;

    [Header("Visual Colors")]
    public Color enabledColor = new Color(1f, 1f, 1f, 1f);
    public Color disabledColor = new Color(1f, 1f, 1f, 0.35f);
    public float pulseSpeed = 3.5f;
    public float litPulseStrength = 0.18f;
    public float darkPulseStrength = 0.08f;

    void Awake()
    {
        if (solidCollider == null)
            solidCollider = GetComponent<Collider2D>();
        if (visual == null)
            visual = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        SetIlluminated(startsEnabled);
    }

    public void SetIlluminated(bool illuminated)
    {
        if (staysEnabledAfterFirstLight && activatedOnce)
            illuminated = true;

        IsIlluminated = illuminated;
        if (illuminated)
            activatedOnce = true;

        if (solidCollider != null)
            solidCollider.enabled = illuminated;

        if (visual != null)
            visual.color = illuminated ? enabledColor : disabledColor;
    }

    void Update()
    {
        if (visual == null)
            return;

        Color baseColor = IsIlluminated ? enabledColor : disabledColor;
        float pulseStrength = IsIlluminated ? litPulseStrength : darkPulseStrength;
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseStrength;
        visual.color = new Color(
            Mathf.Clamp01(baseColor.r * pulse),
            Mathf.Clamp01(baseColor.g * pulse),
            Mathf.Clamp01(baseColor.b * pulse),
            baseColor.a
        );
    }
}
