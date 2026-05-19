using UnityEngine;

/// <summary>
/// Watches the player's Y position and kills them below a configured threshold.
/// </summary>
public class FallOutOfBoundsDeath : MonoBehaviour
{
    [Tooltip("If the player's Y drops below this, they die and the game ends.")]
    public float killY = -30f;
    public GameObject winPanel;

    [Tooltip("Message shown on the game over screen.")]
    public string deathMessage = "You fell into the abyss.";

    PlayerHealth health;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        winPanel.SetActive(false);
    }

    void Update()
    {
        if (health == null || health.IsDead)
            return;

        // Out-of-bounds fail-safe for any gaps not covered by explicit trigger volumes.
        if (transform.position.y < killY)
            health.Kill(deathMessage);

        // Win condition: if the player reaches the end of the level, show the win panel.
        //Time.timeScale = 0f;
    }
}

