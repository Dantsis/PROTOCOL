using UnityEngine;

public class AimGun : MonoBehaviour
{
    [Header("Refs")]
    public Transform gun;                    // tu objeto "Gun" (hijo de Hand)
    public SpriteRenderer gunSprite;         // SpriteRenderer del arma (nerf32px)
    public SpriteRenderer handSprite;        // SpriteRenderer de la mano (mano2)
    public Camera cam;                       // si está vacío, usa Camera.main

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void LateUpdate()
    {
        // 1) Ángulo al mouse desde el pivote (Hand)
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - transform.position);
        if (dir.sqrMagnitude < 0.000001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 2) Rotamos el PIVOTE (Hand) 360°
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 3) ¿Mirando a la izquierda?
        bool lookingLeft = (angle > 90f || angle < -90f);

        // 4) El arma SÍ se voltea para no quedar boca abajo
        if (gunSprite) gunSprite.flipY = lookingLeft;

        // 5) La mano NUNCA se voltea (pero sigue rotando con el pivote)
        if (handSprite)
        {
            handSprite.flipY = false;                 // <- clave
            handSprite.transform.localRotation = Quaternion.identity;
        }

        // 6) Asegurar que el "Gun" no acumule rotaciones locales
        if (gun) gun.localRotation = Quaternion.identity;
    }
}
