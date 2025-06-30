using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.3f;
    private float fireCooldown = 0f;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (Input.GetMouseButton(0) && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = fireRate;
        }
    }

    void Shoot()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 shootDir = (mousePos - (Vector2)firePoint.position).normalized;


        if (shootDir == Vector2.zero) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().direction = shootDir;


        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}

