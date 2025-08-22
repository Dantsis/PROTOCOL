using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Tooltip("Velocidad en unidades/seg")]
    public float speed = 12f;

    [Tooltip("Segundos antes de autodestruirse")]
    public float life = 2f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Seguridad: sin gravedad
        rb.gravityScale = 0f;
        // No cuelgues el Y 🙂
        rb.constraints = RigidbodyConstraints2D.None;
    }

    // Llamalo al instanciar
    public void Launch(Vector2 dir)
    {
        rb.velocity = dir.normalized * speed;
        Invoke(nameof(Die), life);
    }

    void Die() => Destroy(gameObject);

    // IMPORTANTE: no muevas por Translate en Update; solo velocity.
}


