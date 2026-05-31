using UnityEngine;

/// <summary>
/// Basic health lifecycle for the player: damage, death, and reset.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public int health = 5;
    public GameObject DiePanel;
    public void Start()
    {
          DiePanel.SetActive(false);
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }

    }

    public void Die()
    {
        // Handle player death (e.g., play animation, disable controls, etc.)
        Debug.Log("Player has died.");
        // For now, just reset health for demonstration.
        //health = 5;
        DiePanel.SetActive(true);
    }
}

