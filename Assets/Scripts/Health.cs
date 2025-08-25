using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Salud")]
    public int maxHealth = 6;
    public int currentHealth;

    [Header("Tipo")]
    public bool isPlayer = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public bool IsAlive => currentHealth > 0;

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitNormal)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Por ahora: desaparecer el objeto cuando muere
        Destroy(gameObject);

        // Más adelante, para el Player podés cambiar esto
        // por respawn, perder una vida, etc.
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}

