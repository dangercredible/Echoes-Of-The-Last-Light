using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        ResetHealth();
    }

    public void ResetHealth()
    {
        IsDead = false;
        CurrentHealth = maxHealth;
    }

    public void Damage(int amount, string reason = null)
    {
        if (IsDead)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - Mathf.Abs(amount));
        if (CurrentHealth <= 0)
            Kill(reason);
    }

    public void Kill(string reason = null)
    {
        if (IsDead)
            return;

        IsDead = true;
        CurrentHealth = 0;

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.Die();

        GameOverController gameOver = GameOverController.InstanceOrFind();
        if (gameOver != null)
            gameOver.GameOver(reason ?? "You died.");
    }
}

