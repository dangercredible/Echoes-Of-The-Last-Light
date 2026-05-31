using UnityEngine;

public class HealthLantern : MonoBehaviour
{
    [SerializeField] private float startingHealth;
    public float CurrentHealth { get; private set; }

    private void Awake()
    {
        CurrentHealth = startingHealth;
    }
    public void TakeDamage(float _damage)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - _damage, 0, startingHealth);

        if (CurrentHealth > 0)
        {
            //player hurt    
        }
        else
        {
            //player dead
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            TakeDamage(1);
    }
}
