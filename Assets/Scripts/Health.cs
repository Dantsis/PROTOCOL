using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Salud")]
    public int maxHealth = 6;
    public int currentHealth;

    [Header("Tipo")]
    public bool isPlayer = false;

    // Invulnerabilidad temporal
    [HideInInspector] public bool invulnerable = false;
    public void SetInvulnerable(bool v) => invulnerable = v;

    // REFERENCIA AL FLASH
    private DamageFlash damageFlash;

    void Awake()
    {
        currentHealth = maxHealth;

        // Si este objeto tiene DamageFlash, lo tomamos
        damageFlash = GetComponent<DamageFlash>();
    }

    public bool IsAlive => currentHealth > 0;

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitNormal)
    {
        if (!IsAlive) return;
        if (invulnerable) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        // SOLO el Player hace el efecto de daño
        if (isPlayer && damageFlash != null)
        {
            damageFlash.PlayDamageFlash();
        }

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



