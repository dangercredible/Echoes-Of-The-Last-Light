using UnityEngine;

/// <summary>
/// When the lantern is off, the BoxCollider2D is solid; when the lantern is on, collision is disabled so the player falls through.
/// </summary>
public class LightOffSolidPlatform : MonoBehaviour
{
    [SerializeField] LightLantern lantern;
    [SerializeField] BoxCollider2D boxCollider2D;
    [Tooltip("If true: solid when lantern is off. If false: solid when lantern is on (inverted).")]
    [SerializeField] bool solidWhenLanternOff = true;

    void Awake()
    {
        if (boxCollider2D == null)
            boxCollider2D = GetComponent<BoxCollider2D>();
        if (lantern == null)
            lantern = FindFirstObjectByType<LightLantern>();
    }

    void LateUpdate()
    {
        if (boxCollider2D == null)
            return;

        bool lanternOn = lantern != null && lantern.IsOn;
        bool wantSolid = solidWhenLanternOff ? !lanternOn : lanternOn;
        if (boxCollider2D.enabled != wantSolid)
            boxCollider2D.enabled = wantSolid;
    }
}
