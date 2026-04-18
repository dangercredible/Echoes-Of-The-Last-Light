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
    public Color enabledColor = Color.white;
    public Color disabledColor = new Color(1f, 1f, 1f, 0.04f);
    public float pulseSpeed = 4.5f;
    public float litPulseStrength = 0.35f;
    public float darkPulseStrength = 0.04f;
    public float litScaleBoost = 1.08f;
    public float stateFlashDuration = 0.15f;
    public Color flashColor = Color.white;

    private Vector3 baseScale = Vector3.one;
    private int baseSortingOrder;
    private float flashTimer;

    void Awake()
    {
        if (solidCollider == null)
            solidCollider = GetComponent<Collider2D>();
        if (visual == null)
            visual = GetComponent<SpriteRenderer>();

        if (visual != null)
        {
            baseScale = visual.transform.localScale;
            baseSortingOrder = visual.sortingOrder;
        }

        EnsureLightOverlapCollider();
    }

    void EnsureLightOverlapCollider()
    {
        if (solidCollider == null)
            return;

        foreach (Collider2D existing in GetComponents<Collider2D>())
        {
            if (existing != null && existing != solidCollider && existing.isTrigger)
                return;
        }

        BoxCollider2D sensor = gameObject.AddComponent<BoxCollider2D>();
        sensor.isTrigger = true;
        sensor.enabled = true;

        if (solidCollider is BoxCollider2D boxSolid)
        {
            sensor.offset = boxSolid.offset;
            sensor.size = boxSolid.size;
        }
        else
        {
            Bounds worldBounds = solidCollider.bounds;
            Vector2 localCenter = transform.InverseTransformPoint(worldBounds.center);
            float sx = Mathf.Abs(transform.lossyScale.x) > 0.0001f ? transform.lossyScale.x : 1f;
            float sy = Mathf.Abs(transform.lossyScale.y) > 0.0001f ? transform.lossyScale.y : 1f;
            sensor.offset = localCenter;
            sensor.size = new Vector2(worldBounds.size.x / sx, worldBounds.size.y / sy);
        }
    }

    void Start()
    {
        SetIlluminated(startsEnabled);
    }

    public void SetIlluminated(bool illuminated)
    {
        bool changed = IsIlluminated != illuminated;

        if (staysEnabledAfterFirstLight && activatedOnce)
            illuminated = true;

        IsIlluminated = illuminated;
        if (illuminated)
            activatedOnce = true;

        if (changed)
            flashTimer = stateFlashDuration;

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
        Color pulsedColor = new Color(
            Mathf.Clamp01(baseColor.r * pulse),
            Mathf.Clamp01(baseColor.g * pulse),
            Mathf.Clamp01(baseColor.b * pulse),
            baseColor.a
        );

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(flashTimer / stateFlashDuration);
            pulsedColor = Color.Lerp(pulsedColor, flashColor, t);
        }

        visual.color = pulsedColor;
        visual.sortingOrder = IsIlluminated ? baseSortingOrder + 2 : baseSortingOrder;

        float targetScale = IsIlluminated ? litScaleBoost : 1f;
        float scalePulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * (IsIlluminated ? 0.035f : 0.01f);
        visual.transform.localScale = Vector3.Lerp(visual.transform.localScale, baseScale * targetScale * scalePulse, Time.deltaTime * 8f);
    }
}
