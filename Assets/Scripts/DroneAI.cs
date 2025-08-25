using UnityEngine;

public class DroneAI : MonoBehaviour
{
    [Header("Refs")]
    public Transform gunPivot;          // hijo que rota (GunPivot)
    public Transform cannonSprite;      // transform con SpriteRenderer del cañón (Cannon)
    public Transform firePoint;         // punta del cañón
    public GameObject bulletPrefab;     // prefab con Bullet.cs o Rigidbody2D
    public Transform[] waypoints;       // puntos de patrulla en orden
    public Transform player;            // si no se asigna, se busca por tag "Player"
    public Camera cam;                  // opcional

    [Header("Movimiento")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 3.5f;
    public float waypointTolerance = 0.1f;

    [Header("Detección y combate")]
    public float detectionRadius = 7f;   // rango para detectar al jugador
    public float fireRange = 6f;   // rango para disparar
    public float fireRate = 3f;   // balas / segundo
    public float bulletSpeed = 12f;  // para fallback si no usás Bullet.Launch
    public int bulletDamage = 1;    // daño de la bala enemiga
    public LayerMask losMask;            // walls / obstáculos para línea de visión
    public bool requireLineOfSight = true;

    // Internos
    Rigidbody2D rb;
    int wpIndex = 0;
    float nextShotTime = 0f;
    Vector2 desiredVel = Vector2.zero;

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
        bool inFire = dist <= fireRange;
        bool hasLoS = !requireLineOfSight || HasLineOfSight();

        // 1) Apuntar el cañón si detecta
        if (inDetect && hasLoS) AimGunAt(player.position);

        // 2) Movimiento: patrulla / persecución / detenerse para disparar
        if (inDetect && hasLoS)
        {
            if (!inFire)
                SetDesiredVelocity((player.position - transform.position).normalized * chaseSpeed);
            else
                SetDesiredVelocity(Vector2.zero);
        }
        else
        {
            Patrol();
        }

        // 3) Disparo
        if (inDetect && inFire && hasLoS) TryShoot(player.position);
    }

    void FixedUpdate()
    {
        if (rb)
            rb.MovePosition(rb.position + desiredVel * Time.fixedDeltaTime);
        else
            transform.position += (Vector3)(desiredVel * Time.fixedDeltaTime);
    }

    // --- LÓGICA ---

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

    void AimGunAt(Vector3 worldPos)
    {
        if (!gunPivot) return;

        Vector2 aimDir = (worldPos - gunPivot.position);
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        // rota el pivote (Z)
        gunPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        // flip visual del sprite del cañón para que no quede boca abajo
        if (cannonSprite != null)
        {
            bool left = (angle > 90f || angle < -90f);
            // flip por Y = 180° sin tocar escala
            cannonSprite.localRotation = Quaternion.Euler(0f, left ? 180f : 0f, 0f);
        }
    }

    void TryShoot(Vector3 targetPos)
    {
        if (Time.time < nextShotTime) return;
        nextShotTime = Time.time + (fireRate > 0f ? 1f / fireRate : 0f);

        if (!bulletPrefab || !firePoint) return;

        Vector2 dir = ((Vector2)targetPos - (Vector2)firePoint.position).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = gunPivot ? (Vector2)gunPivot.right : Vector2.right;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // A) Bullet con team/daño
        var b = go.GetComponent<Bullet>();
        if (b != null)
        {
            b.team = Bullet.Team.Enemy; // ← clave
            b.damage = bulletDamage;
            b.speed = bulletSpeed;
            b.Launch(dir);
            return;
        }

        // B) Solo Rigidbody2D (fallback)
        var rb2 = go.GetComponent<Rigidbody2D>();
        if (rb2 != null)
        {
            rb2.linearVelocity = dir * bulletSpeed;  // (fix) velocity, no linearVelocity
            Destroy(go, 3f);
        }
    }

    bool HasLineOfSight()
    {
        if (!player) return false;
        if (losMask == 0) return true;

        Vector2 origin = gunPivot ? (Vector2)gunPivot.position : (Vector2)transform.position;
        Vector2 to = (Vector2)player.position - origin;
        float dist = to.magnitude;
        var hit = Physics2D.Raycast(origin, to.normalized, dist, losMask);
        return hit.collider == null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.75f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, fireRange);
    }
}

