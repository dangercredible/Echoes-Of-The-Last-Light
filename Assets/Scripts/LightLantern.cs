using System.Collections.Generic;
using UnityEngine;

public class LightLantern : MonoBehaviour
{
    [Header("Lantern")]
    [Tooltip("If false, press F to turn the lantern on — light platforms and grapple points only react while the lantern is on.")]
    public bool startsOn = false;
    public float lightRadius = 6f;
    public LayerMask affectedLayers;
    public bool IsOn { get; private set; }

    [Header("Optional Visuals")]
    public Behaviour[] visualsToToggle;
    public SpriteRenderer[] moodRenderers;
    public Color lightModeTint = new Color(0.92f, 0.9f, 1f, 1f);
    public Color darkModeTint = new Color(0.38f, 0.42f, 0.58f, 1f);
    public float tintLerpSpeed = 8f;
    public Transform auraVisual;
    public float auraPulseSpeed = 4f;
    public float auraPulseAmount = 0.1f;

    private readonly HashSet<ILightReactive> litTargets = new HashSet<ILightReactive>();
    private Vector3 auraBaseScale = Vector3.one;

    void Start()
    {
        if (moodRenderers == null || moodRenderers.Length == 0)
            moodRenderers = GetComponentsInParent<SpriteRenderer>(true);

        if (auraVisual != null)
            auraBaseScale = auraVisual.localScale;

        SetLanternState(startsOn);
    }

    void Update()
    {
        if (!IsOn)
            return;

        Collider2D[] hits = GetOverlapHits();

        for (int i = 0; i < hits.Length; i++)
        {
            MonoBehaviour[] behaviours = hits[i].GetComponentsInParent<MonoBehaviour>(true);
            for (int b = 0; b < behaviours.Length; b++)
            {
                if (behaviours[b] is ILightReactive reactive)
                {
                    if (!litTargets.Contains(reactive))
                    {
                        reactive.SetIlluminated(true);
                        litTargets.Add(reactive);
                    }
                }
            }
        }

        PlayerController pcSwing = Object.FindFirstObjectByType<PlayerController>();
        if (pcSwing != null && pcSwing.IsGrappling())
        {
            LightGrapplePoint swingTarget = pcSwing.GetActiveGrappleTarget();
            if (swingTarget != null && !litTargets.Contains(swingTarget))
            {
                swingTarget.SetIlluminated(true);
                litTargets.Add(swingTarget);
            }
        }
    }

    public void ToggleLantern()
    {
        SetLanternState(!IsOn);
    }

    public void SetLanternState(bool enabledState)
    {
        IsOn = enabledState;

        if (visualsToToggle != null)
        {
            for (int i = 0; i < visualsToToggle.Length; i++)
            {
                if (visualsToToggle[i] != null)
                    visualsToToggle[i].enabled = enabledState;
            }
        }

        if (!enabledState)
            ClearLitTargets();
        else
            IlluminateInRange();

        if (auraVisual != null)
            auraVisual.gameObject.SetActive(enabledState);
    }

    void ClearLitTargets()
    {
        List<ILightReactive> toRemove = new List<ILightReactive>();
        foreach (ILightReactive reactive in litTargets)
        {
            if (ShouldKeepGrappleLitForActiveSwing(reactive))
                continue;
            reactive.SetIlluminated(false);
            toRemove.Add(reactive);
        }

        for (int i = 0; i < toRemove.Count; i++)
            litTargets.Remove(toRemove[i]);
    }

    bool ShouldKeepGrappleLitForActiveSwing(ILightReactive reactive)
    {
        if (reactive is not LightGrapplePoint point)
            return false;

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        return player != null && player.IsGrappling() && player.GetActiveGrappleTarget() == point;
    }

    void LateUpdate()
    {
        if (!Application.isPlaying)
            return;

        if (!IsOn)
        {
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null || !player.IsGrappling())
            {
                List<ILightReactive> toRemove = new List<ILightReactive>();
                foreach (ILightReactive reactive in litTargets)
                {
                    if (ShouldKeepGrappleLitForActiveSwing(reactive))
                        continue;
                    reactive.SetIlluminated(false);
                    toRemove.Add(reactive);
                }

                for (int i = 0; i < toRemove.Count; i++)
                    litTargets.Remove(toRemove[i]);
            }
        }

        UpdateMoodVisuals();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lightRadius);
    }

    void UpdateMoodVisuals()
    {
        if (moodRenderers == null || moodRenderers.Length == 0)
            return;

        Color targetTint = IsOn ? lightModeTint : darkModeTint;
        for (int i = 0; i < moodRenderers.Length; i++)
        {
            if (moodRenderers[i] == null)
                continue;

            moodRenderers[i].color = Color.Lerp(moodRenderers[i].color, targetTint, Time.deltaTime * tintLerpSpeed);
        }

        if (auraVisual != null && IsOn)
        {
            float pulse = 1f + Mathf.Sin(Time.time * auraPulseSpeed) * auraPulseAmount;
            auraVisual.localScale = auraBaseScale * pulse;
        }
    }

    Collider2D[] GetOverlapHits()
    {
        if (affectedLayers.value != 0)
        {
            Collider2D[] masked = Physics2D.OverlapCircleAll(transform.position, lightRadius, affectedLayers);
            if (masked.Length == 0)
                return Physics2D.OverlapCircleAll(transform.position, lightRadius);
            return masked;
        }

        return Physics2D.OverlapCircleAll(transform.position, lightRadius);
    }

    void IlluminateInRange()
    {
        Collider2D[] hits = GetOverlapHits();

        for (int i = 0; i < hits.Length; i++)
        {
            MonoBehaviour[] behaviours = hits[i].GetComponentsInParent<MonoBehaviour>(true);
            for (int b = 0; b < behaviours.Length; b++)
            {
                if (behaviours[b] is ILightReactive reactive)
                {
                    reactive.SetIlluminated(true);
                    litTargets.Add(reactive);
                }
            }
        }
    }
}
