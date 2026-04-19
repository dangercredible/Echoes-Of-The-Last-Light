using UnityEngine;

/// <summary>
/// Trigger volume that kills the player when entered (for pits/voids).
/// </summary>
public class FallDeathZone : MonoBehaviour
{
    public string deathMessage = "You fell.";

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to the player body.
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.Kill(deathMessage);
            return;
        }

        player.Die();
        GameOverController gameOver = GameOverController.InstanceOrFind();
        if (gameOver != null)
            gameOver.GameOver(deathMessage);
    }
}
