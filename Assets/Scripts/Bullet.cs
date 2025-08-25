using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    public enum Team { Player, Enemy }

    [Header("Ajustes")]
    public Team team = Team.Player;
    public int damage = 1;
    public float speed = 12f;
    public float life = 2.5f;
    public LayerMask destroyOnLayers;   // Walls (asigná acá la Layer Walls)

    Rigidbody2D rb;
    Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // TRIGGER obligatorio si usamos OnTriggerEnter2D
        col.isTrigger = true;
    }

    void OnEnable()
    {
        if (life > 0) Invoke(nameof(Die), life);
    }

    public void Launch(Vector2 dir)
    {
        rb.linearVelocity = dir.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Debug info
        Debug.Log($"[Bullet] Hit {other.name} (Layer={LayerMask.LayerToName(other.gameObject.layer)}, Tag={other.tag})");

        // 1) Paredes por Layer
        if (((1 << other.gameObject.layer) & destroyOnLayers) != 0)
        {
            Debug.Log("[Bullet] Destroy on Walls layer.");
            Die();
            return;
        }

        // 2) Daño por equipo/Tag
        if (team == Team.Player && other.CompareTag("Enemy"))
        {
            if (TryDamage(other)) Debug.Log("[Bullet] Damaged ENEMY!");
            Die();
        }
        else if (team == Team.Enemy && other.CompareTag("Player"))
        {
            if (TryDamage(other)) Debug.Log("[Bullet] Damaged PLAYER!");
            Die();
        }
        // Si choca con otras cosas no relevantes, ignoramos.
    }

    bool TryDamage(Collider2D col)
    {
        var health = col.GetComponentInParent<Health>();
        if (health != null && health.IsAlive)
        {
            Vector2 hitPoint = col.ClosestPoint(transform.position);
            Vector2 hitNormal = (col.transform.position - transform.position).normalized;
            health.TakeDamage(damage, hitPoint, hitNormal);
            return true;
        }
        return false;
    }

    void Die() => Destroy(gameObject);
}



