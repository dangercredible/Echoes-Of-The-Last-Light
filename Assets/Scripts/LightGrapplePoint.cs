using System.Collections.Generic;
using UnityEngine;

public class LightGrapplePoint : MonoBehaviour, ILightReactive
{
    public static readonly HashSet<LightGrapplePoint> ActivePoints = new HashSet<LightGrapplePoint>();

    [Header("State")]
    public bool startIlluminated;
    public bool IsIlluminated { get; private set; }

    [Header("Visual")]
    public SpriteRenderer indicator;
    public Color activeColor = new Color(1f, 0.95f, 0.4f, 1f);
    public Color inactiveColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    void OnEnable()
    {
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
