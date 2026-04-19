using UnityEngine;

/// <summary>
/// End-of-level trigger that shows the round complete screen once.
/// </summary>
public class RoundCheckpoint : MonoBehaviour
{
    bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only the player can complete the round.
        if (triggered)
            return;

        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        triggered = true;
        RoundCompleteController ctrl = RoundCompleteController.InstanceOrFind();
        if (ctrl != null)
            ctrl.ShowRoundComplete();
    }
}
