using System.Collections.Generic;
using UnityEngine;

public class LightLantern : MonoBehaviour
{
    [Header("Lantern")]
    public bool startsOn = true;
    public float lightRadius = 6f;
    public LayerMask affectedLayers;
    public bool IsOn { get; private set; }

    [Header("Optional Visuals")]
    public Behaviour[] visualsToToggle;

    private readonly HashSet<ILightReactive> litTargets = new HashSet<ILightReactive>();

    void Start()
    {
        SetLanternState(startsOn);
    }

    void Update()
    {
        if (!IsOn)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, lightRadius, affectedLayers);
        HashSet<ILightReactive> currentFrame = new HashSet<ILightReactive>();

        for (int i = 0; i < hits.Length; i++)
        {
            MonoBehaviour[] behaviours = hits[i].GetComponents<MonoBehaviour>();
            for (int b = 0; b < behaviours.Length; b++)
            {
                if (behaviours[b] is ILightReactive reactive)
                {
                    currentFrame.Add(reactive);
                    if (!litTargets.Contains(reactive))
                        reactive.SetIlluminated(true);
                }
            }
        }

        List<ILightReactive> toDisable = new List<ILightReactive>();
        foreach (ILightReactive reactive in litTargets)
        {
            if (!currentFrame.Contains(reactive))
                toDisable.Add(reactive);
        }

        for (int i = 0; i < toDisable.Count; i++)
            toDisable[i].SetIlluminated(false);

        litTargets.Clear();
        foreach (ILightReactive reactive in currentFrame)
            litTargets.Add(reactive);
    }

    public void ToggleLantern()
    {
        SetLanternState(!IsOn);
    }

    public void SetLanternState(bool enabledState)
    {
        IsOn = enabledState;

        for (int i = 0; i < visualsToToggle.Length; i++)
        {
            if (visualsToToggle[i] != null)
                visualsToToggle[i].enabled = enabledState;
        }

        if (!enabledState)
            ClearLitTargets();
    }

    void ClearLitTargets()
    {
        foreach (ILightReactive reactive in litTargets)
            reactive.SetIlluminated(false);
        litTargets.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lightRadius);
    }
}
