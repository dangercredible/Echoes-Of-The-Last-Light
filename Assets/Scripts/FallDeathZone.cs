using UnityEngine;

public class FallDeathZone : MonoBehaviour
{
    public string deathMessage = "You fell.";

    void OnTriggerEnter2D(Collider2D other)
    {
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
