using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DroneMechAI : MonoBehaviour
{
    [Header("Refs")]
    public Transform[] waypoints;
    public Transform player;            // si no se asigna, se busca por tag "Player"
    public Camera cam;                  // opcional, por si luego usás algo visual

    [Header("Movimiento")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 3.5f;
    public float waypointTolerance = 0.1f;

    [Header("Detección")]
    public float detectionRadius = 7f;
    public LayerMask losMask;           // walls / obstaculos para línea de visión
    public bool requireLineOfSight = true;

    [Header("Daño por contacto")]
    public int contactDamage = 1;
    public float hitCooldown = 0.5f;    // tiempo mínimo entre golpes

    // Internos
    Rigidbody2D rb;
    int wpIndex = 0;
    Vector2 desiredVel = Vector2.zero;
    float nextHitTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inDetect = dist <= detectionRadius;
        bool hasLoS = !requireLineOfSight || HasLineOfSight();

        if (inDetect && hasLoS)
        {
            // Perseguir siempre que detecta
            Vector2 dir = (player.position - transform.position).normalized;
            SetDesiredVelocity(dir * chaseSpeed);
        }
        else
        {
            Patrol();
        }
    }

    void FixedUpdate()
    {
        if (rb)
            rb.MovePosition(rb.position + desiredVel * Time.fixedDeltaTime);
        else
            transform.position += (Vector3)(desiredVel * Time.fixedDeltaTime);
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            SetDesiredVelocity(Vector2.zero);
            return;
        }

        Transform targetWP = waypoints[wpIndex];
        Vector2 to = (targetWP.position - transform.position);
        if (to.magnitude <= waypointTolerance)
        {
            wpIndex = (wpIndex + 1) % waypoints.Length;
            targetWP = waypoints[wpIndex];
            to = (targetWP.position - transform.position);
        }
        SetDesiredVelocity(to.normalized * patrolSpeed);
    }

    void SetDesiredVelocity(Vector2 v) => desiredVel = v;

    bool HasLineOfSight()
    {
        if (!player) return false;
        if (losMask == 0) return true;

        Vector2 origin = (Vector2)transform.position;
        Vector2 to = (Vector2)player.position - origin;
        float dist = to.magnitude;
        var hit = Physics2D.Raycast(origin, to.normalized, dist, losMask);
        return hit.collider == null;
    }

    // --- Daño por contacto ---

    void TryHitPlayer(Hurtbox hb)
    {
        if (hb == null || hb.health == null || !hb.health.isPlayer) return;
        if (Time.time < nextHitTime) return;

        nextHitTime = Time.time + hitCooldown;

        Vector2 dir = ((Vector2)hb.transform.position - (Vector2)transform.position).normalized;
        hb.health.TakeDamage(contactDamage, hb.transform.position, dir);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        var hb = collision.collider.GetComponent<Hurtbox>() ??
                 collision.collider.GetComponentInParent<Hurtbox>();
        TryHitPlayer(hb);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        var hb = other.GetComponent<Hurtbox>() ??
                 other.GetComponentInParent<Hurtbox>();
        TryHitPlayer(hb);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}