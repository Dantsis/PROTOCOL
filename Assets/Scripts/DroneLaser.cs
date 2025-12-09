using UnityEngine;

public class DroneLaser : MonoBehaviour
{
    // -----------------------------------------------------
    // ESTADOS
    // -----------------------------------------------------
    public enum State { Idle, Telegraph, Firing, Cooldown }
    State state = State.Idle;

    // -----------------------------------------------------
    // REFERENCIAS
    // -----------------------------------------------------
    [Header("Refs")]
    public Transform gunPivot;
    public Transform firePoint;
    public LineRenderer telegraphRenderer;
    public LineRenderer laserRenderer;
    public Transform player;

    [Header("Detección")]
    public float detectionRadius = 7f;
    public LayerMask losMask;

    [Header("Ataque Láser")]
    public float telegraphTime = 2f;         // delay ANTES de disparar
    public float firingTime = 2f;            // duración del rayo
    public float cooldownTime = 2f;          // después de pegar
    public float fireRange = 6f;

    public int dps = 12;                     // daño por segundo del rayo
    public LayerMask hitMask;                // paredes / jugador / objetos con collider

    float stateTimer = 0f;

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
            // ----------------------------------------------------------
            //  IDLE
            // ----------------------------------------------------------
            case State.Idle:
                if (seesPlayer)
                {
                    state = State.Telegraph;
                    stateTimer = telegraphTime;
                    telegraphRenderer.enabled = true;
                }
                break;

            // ----------------------------------------------------------
            //  TELEGRAPH (rayo tenue)
            // ----------------------------------------------------------
            case State.Telegraph:
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
                    state = State.Firing;
                    stateTimer = firingTime;
                }
                break;

            // ----------------------------------------------------------
            //  FIRING (láser fuerte, daño continuo)
            // ----------------------------------------------------------
            case State.Firing:
                AimGunAt(player.position);

                bool hitPlayer = UpdateLaserBeamAndDamage();

                stateTimer -= Time.deltaTime;

                // Si pega al jugador → cortar ataque + cooldown
                if (hitPlayer)
                {
                    laserRenderer.enabled = false;
                    state = State.Cooldown;
                    stateTimer = cooldownTime;
                }
                // Si pierde al jugador → apagarse
                else if (!seesPlayer || stateTimer <= 0)
                {
                    laserRenderer.enabled = false;
                    state = State.Idle;
                }

                break;

            // ----------------------------------------------------------
            //  COOLDOWN
            // ----------------------------------------------------------
            case State.Cooldown:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    state = State.Idle;
                }
                break;
        }
    }

    // ==========================================================
    //   BEAMS (TELEGRAPH + FIRING)
    // ==========================================================

    void UpdateTelegraphBeam()
    {
        Vector3 origin = firePoint.position;
        Vector3 dir = gunPivot.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, fireRange, hitMask);

        telegraphRenderer.SetPosition(0, origin);

        if (hit.collider != null)
            telegraphRenderer.SetPosition(1, hit.point);
        else
            telegraphRenderer.SetPosition(1, origin + dir * fireRange);
    }

    bool UpdateLaserBeamAndDamage()
    {
        Vector3 origin = firePoint.position;
        Vector3 dir = gunPivot.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, fireRange, hitMask);

        laserRenderer.SetPosition(0, origin);

        if (hit.collider != null)
        {
            // fin del rayo
            laserRenderer.SetPosition(1, hit.point);

            // --------------------------------------------
            // HIT AL JUGADOR
            // --------------------------------------------
            if (hit.collider.CompareTag("Player"))
            {
                Health hp = hit.collider.GetComponent<Health>();
                if (hp != null)
                {
                    int dmg = Mathf.RoundToInt(dps * Time.deltaTime);

                    hp.TakeDamage(
                        dmg,
                        hit.point,          // punto exacto del impacto
                        -dir                // normal hacia atrás
                    );
                }

                return true; // esto corta el ataque
            }
        }
        else
        {
            laserRenderer.SetPosition(1, origin + dir * fireRange);
        }

        return false;
    }

    // ==========================================================
    //   AIM
    // ==========================================================

    public void AimGunAt(Vector3 worldPos)
    {
        Vector2 dir = worldPos - gunPivot.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.Euler(0, 0, angle);
    }

    // ==========================================================
    //   LINE OF SIGHT
    // ==========================================================

    bool HasLineOfSight()
    {
        Vector2 origin = gunPivot.position;
        Vector2 dir = (player.position - gunPivot.position).normalized;
        float dist = Vector2.Distance(origin, player.position);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, losMask);

        return hit.collider == null; // nada bloquea
    }
}
