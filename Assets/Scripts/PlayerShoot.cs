using UnityEngine;
using Debug = UnityEngine.Debug;

[DisallowMultipleComponent]
public class PlayerShoot : MonoBehaviour
{
    public enum FireMode { SemiAuto, AutoOnHold, Both }

    [Header("Refs")]
    public Transform firePoint;           // punta del cañón
    public GameObject bulletPrefab;       // prefab con Bullet.cs (recomendado)
    public Camera cam;                    // si null → Camera.main

    [Header("Modo de disparo")]
    public FireMode fireMode = FireMode.Both;

    [Header("Ajustes")]
    [Tooltip("Balas por segundo. Aplica al click y al mantener. Si = 0 → solo 1 por click (sin auto).")]
    public float fireRate = 8f;
    public float bulletSpeed = 12f;
    public int bulletDamage = 1;

    // Internos
    private float nextShotTime = 0f;
    private static int lastShotFrame = -999999;   // evita doble disparo en el mismo frame

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (fireRate < 0f) fireRate = 0f;

        // Aviso si hay dos PlayerShoot en el mismo GO (frecuente causa de doble tiro)
        var dups = GetComponents<PlayerShoot>();
        if (dups.Length > 1)
        {
            Debug.LogWarning($"[PlayerShoot] Hay {dups.Length} PlayerShoot en '{name}'. Dejá solo uno.");
        }
    }

    void Update()
    {
        bool pressed = Input.GetMouseButtonDown(0);
        bool held = Input.GetMouseButton(0);

        // 1) Click: dispara si el cooldown lo permite
        if ((fireMode == FireMode.Both || fireMode == FireMode.SemiAuto) && pressed && Time.time >= nextShotTime)
        {
            if (FireOnce())
                ScheduleNext();
            return; // evita que también dispare por la rama de "held" en el mismo frame
        }

        // 2) Mantener: dispara cuando pase el cooldown
        if ((fireMode == FireMode.Both || fireMode == FireMode.AutoOnHold) && held && Time.time >= nextShotTime)
        {
            if (FireOnce())
                ScheduleNext();
        }
    }

    void ScheduleNext()
    {
        if (fireRate > 0f)
            nextShotTime = Time.time + 1f / fireRate;
        else
            nextShotTime = Mathf.Infinity; // sin auto, solo semiauto por click
    }

    bool FireOnce()
    {
        if (!firePoint || !bulletPrefab || cam == null) return false;

        // Guard: NO permitir 2 disparos en el mismo frame (aunque otro script también llame)
        if (lastShotFrame == Time.frameCount) return false;
        lastShotFrame = Time.frameCount;

        // Dirección al mouse
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - firePoint.position);
        if (dir.sqrMagnitude < 1e-6f) dir = firePoint.right;
        dir.Normalize();

        // Instanciar bala
        GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        if (!go)
        {
            Debug.LogWarning("[PlayerShoot] Instantiate devolvió null.");
            return false;
        }

        // Preferimos Bullet.cs (team/daño/velocidad)
        var b = go.GetComponent<Bullet>();
        if (b != null)
        {
            b.team = Bullet.Team.Player;
            b.damage = bulletDamage;
            b.speed = bulletSpeed;
            b.Launch(dir);
            return true;
        }

        // Fallback: mover con Rigidbody2D
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = dir * bulletSpeed;
            Destroy(go, 3f);
            return true;
        }

        // Si no hay Bullet ni RB2D, abortar para evitar "bala flotando"
        Debug.LogError("[PlayerShoot] El bulletPrefab no tiene Bullet ni Rigidbody2D. Deshabilitando script.");
        enabled = false;
        Destroy(go);
        return false;
    }
}



