using UnityEngine;
using Debug = UnityEngine.Debug;

public class PlayerShoot : MonoBehaviour
{
    public Transform firePoint;       // punta del cañón
    public GameObject bulletPrefab;   // prefab con Bullet.cs
    public Camera cam;                // si null → Camera.main

    [Header("Disparo")]
    public float fireRate = 8f;       // balas/seg mientras se mantiene
    public float bulletSpeed = 12f;

    private float nextShotTime;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        // semi-auto: una inmediatamente
        if (Input.GetMouseButtonDown(0)) TryShoot();

        // auto-fire: mantener apretado
        if (Input.GetMouseButton(0) && Time.time >= nextShotTime) TryShoot();
    }

    void TryShoot()
    {
        if (!firePoint || !bulletPrefab) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 dir = (mouseWorld - firePoint.position).normalized;

        // Visualizá la dirección 0.5u en Scene View
        Debug.DrawRay(firePoint.position, dir * 0.5f, Color.cyan, 0.1f);
        Debug.Log($"Shoot dir: {dir}");

        var go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // pasar velocidad deseada
        var b = go.GetComponent<Bullet>();
        b.speed = bulletSpeed;
        b.Launch(dir);

        // programar próximo tiro
        if (fireRate > 0f) nextShotTime = Time.time + 1f / fireRate;
    }
}


