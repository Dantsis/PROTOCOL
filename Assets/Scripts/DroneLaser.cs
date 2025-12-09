using UnityEngine;

public class DroneLaser : MonoBehaviour
{
    [Header("Refs")]
    public Transform gunPivot;
    public Transform cannonSprite;
    public Transform firePoint;
    public LineRenderer laserRenderer;
    public Transform[] waypoints;
    public Transform player;
    public Camera cam;

    [Header("Movimiento")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 3.5f;
    public float waypointTolerance = 0.1f;

    [Header("Detección y combate")]
    public float detectionRadius = 7f;
    public float fireRange = 6f;
    public float fireRate = 1.2f;
    public int laserDamage = 1;
    public LayerMask losMask;
    public bool requireLineOfSight = true;

    [Header("Geometría del cañón")]
    public float hubRadius = 0.35f;
    public float barrelOffset = 0.22f;

    [Header("Giro del cañón")]
    public bool limitArc = false;
    [Range(0f, 180f)] public float arcHalfAngle = 110f;
    public bool smoothAim = true;
    public float aimLerpSpeed = 18f;

    Rigidbody2D rb;
    int wpIndex = 0;
    Vector2 desiredVel;
    float nextShotTime = 0f;
    float currentAimAngle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (gunPivot) currentAimAngle = gunPivot.eulerAngles.z;

        if (laserRenderer)
        {
            laserRenderer.enabled = false;
            laserRenderer.positionCount = 2;
        }
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inDetect = dist <= detectionRadius;
        bool inFire = dist <= fireRange;
        bool hasLoS = !requireLineOfSight || HasLineOfSight();

        if (inDetect && hasLoS) AimGunAt(player.position);

        if (inDetect && hasLoS)
        {
            if (!inFire)
                desiredVel = (player.position - transform.position).normalized * chaseSpeed;
            else
                desiredVel = Vector2.zero;
        }
        else Patrol();

        if (inDetect && inFire && hasLoS)
            ShootLaser();
        else if (laserRenderer)
            laserRenderer.enabled = false;
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
            desiredVel = Vector2.zero;
            return;
        }

        Transform wp = waypoints[wpIndex];
        Vector2 dir = wp.position - transform.position;

        if (dir.magnitude <= waypointTolerance)
        {
            wpIndex = (wpIndex + 1) % waypoints.Length;
            wp = waypoints[wpIndex];
            dir = wp.position - transform.position;
        }
        desiredVel = dir.normalized * patrolSpeed;
    }

    void AimGunAt(Vector3 worldPos)
    {
        if (!gunPivot) return;

        Vector2 aimDir = worldPos - gunPivot.position;
        float targetAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        if (limitArc)
        {
            float a = Mathf.Repeat(targetAngle + 180f, 360f) - 180f;
            a = Mathf.Clamp(a, -arcHalfAngle, arcHalfAngle);
            targetAngle = a;
        }

        if (smoothAim)
            currentAimAngle = Mathf.LerpAngle(currentAimAngle, targetAngle, 1f - Mathf.Exp(-aimLerpSpeed * Time.deltaTime));
        else
            currentAimAngle = targetAngle;

        gunPivot.rotation = Quaternion.Euler(0f, 0f, currentAimAngle);

        if (cannonSprite)
            cannonSprite.localPosition = new Vector3(hubRadius, 0f, 0f);
        if (firePoint)
            firePoint.localPosition = new Vector3(hubRadius + barrelOffset, 0, 0);

        if (cannonSprite)
        {
            bool left = currentAimAngle > 90 || currentAimAngle < -90;
            cannonSprite.localRotation = Quaternion.Euler(0, left ? 180f : 0f, 0);
        }
    }

    void ShootLaser()
    {
        if (!laserRenderer || !firePoint || player == null) return;

        if (Time.time < nextShotTime) return;
        nextShotTime = Time.time + (1f / fireRate);

        laserRenderer.enabled = true;
        Vector3 origin = firePoint.position;
        Vector3 target = player.position;

        laserRenderer.SetPosition(0, origin);
        laserRenderer.SetPosition(1, target);

        // daño: usa tu script Health
        var hp = player.GetComponent<Health>();
        if (hp != null)
            hp.TakeDamage(laserDamage, (Vector2)origin, (Vector2)(target - origin).normalized);
    }

    bool HasLineOfSight()
    {
        if (!player) return false;
        if (losMask == 0) return true;

        Vector2 origin = gunPivot ? (Vector2)gunPivot.position : (Vector2)transform.position;
        Vector2 dir = (Vector2)player.position - origin;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, dir.magnitude, losMask);
        return hit.collider == null;
    }
}
