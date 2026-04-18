using UnityEngine;

public class RoundCheckpoint : MonoBehaviour
{
    bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
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
