using UnityEngine;

/// <summary>
/// Invisible trigger zone placed at the end of each region.
/// When Kael enters it, the scene transition fires.
/// </summary>
public class RegionExitTrigger : MonoBehaviour
{
    [Header("Destination")]
    [Tooltip("Exact scene name as it appears in Build Settings.")]
    public string destinationScene;

    [Header("Optional Delay")]
    [Tooltip("Seconds to wait before the transition starts. Useful for cutscene beats.")]
    public float triggerDelay = 0f;

    [Header("One-Shot")]
    [Tooltip("Prevents the trigger from firing again if the player lingers.")]
    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only Kael's collider should trigger this — tag your player "Player" in Unity.
        if (hasTriggered || !other.CompareTag("Player"))
            return;

        hasTriggered = true;

        if (triggerDelay > 0f)
            Invoke(nameof(FireTransition), triggerDelay);
        else
            FireTransition();
    }

    private void FireTransition()
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogWarning("RegionExitTrigger: No SceneTransitionManager found in scene.");
            return;
        }

        if (string.IsNullOrEmpty(destinationScene))
        {
            Debug.LogWarning("RegionExitTrigger: destinationScene is not set.", gameObject);
            return;
        }

        SceneTransitionManager.Instance.TransitionToScene(destinationScene);
    }

    // Draws the trigger zone in the Scene view so you can see it while editing.
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.35f);
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
            Gizmos.DrawCube(transform.position + (Vector3)col.offset, col.size);
    }
}