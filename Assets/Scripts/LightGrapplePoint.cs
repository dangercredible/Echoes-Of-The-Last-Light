using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grapple anchor point that can be enabled/disabled by lantern light.
/// </summary>
public class LightGrapplePoint : MonoBehaviour, ILightReactive
{
    public static readonly HashSet<LightGrapplePoint> ActivePoints = new HashSet<LightGrapplePoint>();

    [Header("State")]
    public bool startIlluminated;
    public bool IsIlluminated { get; private set; }

    [Header("Visual")]
    public SpriteRenderer indicator;
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(1f, 1f, 1f, 0.12f);

    void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.95f;
        }
    }

    void OnEnable()
    {
        // Keep a global set so the player can query nearby/active anchors quickly.
        ActivePoints.Add(this);
        SetIlluminated(startIlluminated);
    }

    void OnDisable()
    {
        ActivePoints.Remove(this);
    }

    public void SetIlluminated(bool illuminated)
    {
        IsIlluminated = illuminated;
        if (indicator != null)
            indicator.color = illuminated ? activeColor : inactiveColor;
    }
}
