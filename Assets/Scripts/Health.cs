using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Salud")]
    public int maxHealth = 6;
    public int currentHealth;

    [Header("Tipo")]
    public bool isPlayer = false;

    // NUEVO: invulnerabilidad temporal (para spawn, etc.)
    [HideInInspector] public bool invulnerable = false;
    public void SetInvulnerable(bool v) => invulnerable = v;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public bool IsAlive => currentHealth > 0;

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitNormal)
    {
        if (!IsAlive) return;
        if (invulnerable) return;   // <<--- BLOQUEA DAÑO CUANDO ESTÁ INVULNERABLE

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}

