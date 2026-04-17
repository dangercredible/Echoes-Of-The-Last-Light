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
}
