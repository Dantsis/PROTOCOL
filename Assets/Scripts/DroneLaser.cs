using UnityEngine;

public class DroneLaser : MonoBehaviour
{
    // ============================
    // ESTADOS
    // ============================
    public enum State { Idle, Telegraph, Firing, Cooldown }
    State state = State.Idle;

    // ============================
    // REFERENCIAS
    // ============================
    [Header("Refs")]
    public Transform gunPivot;
    public Transform firePoint;
    public LineRenderer telegraphRenderer;
    public LineRenderer laserRenderer;
    public Transform player;

    [Header("Detección")]
    public float detectionRadius = 10f;
    public LayerMask losMask;

    [Header("Ataque Láser")]
    public float telegraphTime = 1.5f;
    public float firingTime = 0.3f;
    public float cooldownTime = 1.5f;
    public float fireRange = 50f;

    public int damagePerBeam = 2;      // solo 1 hit por disparo
    public LayerMask hitMask;

    bool damageDoneThisCycle = false;

    float stateTimer = 0f;

    [Header("Movimiento")]
    public float moveSpeed = 3f;
    public float stopDistance = 9f;

    void Awake()
    {
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        telegraphRenderer.enabled = false;
        laserRenderer.enabled = false;
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool seesPlayer = dist <= detectionRadius && HasLineOfSight();

        switch (state)
        {
            // ============================
            // IDLE
            // ============================
            case State.Idle:
                MoveIfNeeded();

                if (seesPlayer)
                {
                    state = State.Telegraph;
                    stateTimer = telegraphTime;
                    telegraphRenderer.enabled = true;
                }
                break;

            // ============================
            // TELEGRAPH
            // ============================
            case State.Telegraph:
                MoveIfNeeded();
                AimGunAt(player.position);
                UpdateTelegraphBeam();

                stateTimer -= Time.deltaTime;

                if (!seesPlayer)
                {
                    telegraphRenderer.enabled = false;
                    state = State.Idle;
                }
                else if (stateTimer <= 0)
                {
                    telegraphRenderer.enabled = false;
                    laserRenderer.enabled = true;

                    damageDoneThisCycle = false; // reseteamos daño por rayo

                    state = State.Firing;
                    stateTimer = firingTime;
                }
                break;

            // ============================
            // FIRING (rayo sostenido)
            // ============================
            case State.Firing:
                AimGunAt(player.position);

                bool hitPlayer = UpdateLaserBeamAndDamage();

                stateTimer -= Time.deltaTime;

                if (!seesPlayer || stateTimer <= 0)
                {
                    laserRenderer.enabled = false;
                    state = State.Cooldown;
                    stateTimer = cooldownTime;
                }

                break;

            // ============================
            // COOLDOWN
            // ============================
            case State.Cooldown:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    state = State.Idle;
                }
                break;
        }
    }

    // ============================
    // MOVIMIENTO DEL DRON
    // ============================
    void MoveIfNeeded()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > stopDistance)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
        }
    }

    // ============================
    // TELEGRAPH
    // ============================
    void UpdateTelegraphBeam()
    {
        Vector3 origin = firePoint.position;
        Vector3 dir = gunPivot.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, fireRange, hitMask);

        telegraphRenderer.SetPosition(0, origin);
        telegraphRenderer.SetPosition(1, hit ? hit.point : origin + dir * fireRange);
    }

    // ============================
    // FIRING + DAÑO
    // ============================
    bool UpdateLaserBeamAndDamage()
    {
        Vector3 origin = firePoint.position;
        Vector3 dir = gunPivot.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, fireRange, hitMask);

        laserRenderer.SetPosition(0, origin);
        laserRenderer.SetPosition(1, hit ? hit.point : origin + dir * fireRange);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (!damageDoneThisCycle)
                {
                    Health hp = hit.collider.GetComponentInParent<Health>();
                    if (hp != null)
                    {
                        hp.TakeDamage(damagePerBeam, hit.point, -dir);
                    }

                    damageDoneThisCycle = true;
                }

                return true;
            }
        }

        return false;
    }

    // ============================
    // AIM
    // ============================
    public void AimGunAt(Vector3 worldPos)
    {
        Vector2 dir = worldPos - gunPivot.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.Euler(0, 0, angle);
    }

    // ============================
    // LINE OF SIGHT
    // ============================
    bool HasLineOfSight()
    {
        Vector2 origin = gunPivot.position;
        Vector2 dir = (player.position - gunPivot.position).normalized;
        float dist = Vector2.Distance(origin, player.position);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, losMask);

        return hit.collider == null;
    }
}
