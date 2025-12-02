using UnityEngine;

public class AimHand : MonoBehaviour
{
    [Header("Sprites de la mano")]
    public SpriteRenderer handSprite;        // SpriteRenderer de la mano
    public Sprite emptyHandSprite;           // mano vacía (sin bola)
    public Sprite handWithAmmoSprite;        // mano sosteniendo la bola
    public Sprite handThrowSprite;           // mano soltando la bola

    [Header("Refs")]
    public Camera cam;                       // si null, usa Camera.main

    [Header("Animación de disparo")]
    public float throwSpriteTime = 0.08f;    // tiempo que se ve la mano soltando

    private float revertTime = 0f;
    private bool hasAmmo = true;             // estado actual

    void Awake()
    {
        if (!cam) cam = Camera.main;
        ApplyIdleSprite();
    }

    void LateUpdate()
    {
        RotateToMouse();

        // Volver al sprite idle después del disparo
        if (revertTime > 0f && Time.time >= revertTime)
        {
            revertTime = 0f;
            ApplyIdleSprite();
        }
    }

    void RotateToMouse()
    {
        if (!handSprite || cam == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 dir = mouseWorld - transform.position;
        if (dir.sqrMagnitude < 0.000001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Rotamos todo el objeto Hand
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // La mano no se voltea en Y, solo rota
        handSprite.flipY = false;
        handSprite.transform.localRotation = Quaternion.identity;
    }

    void ApplyIdleSprite()
    {
        if (!handSprite) return;

        if (hasAmmo && handWithAmmoSprite != null)
            handSprite.sprite = handWithAmmoSprite;
        else if (!hasAmmo && emptyHandSprite != null)
            handSprite.sprite = emptyHandSprite;
    }

    /// <summary>
    /// Llamado por PlayerShoot cuando cambia la munición.
    /// </summary>
    public void SetHasAmmo(bool value)
    {
        hasAmmo = value;
        // Si no estamos en medio de la animación de disparo, actualizamos el idle.
        if (revertTime <= 0f)
            ApplyIdleSprite();
    }

    /// <summary>
    /// Llamado por PlayerShoot cada vez que se dispara UNA bala.
    /// </summary>
    public void PlayThrowAnimation()
    {
        if (!handSprite) return;
        if (handThrowSprite == null) return;

        // Solo tiene sentido mostrar la animación de soltar si había munición
        if (!hasAmmo) return;

        handSprite.sprite = handThrowSprite;
        revertTime = Time.time + throwSpriteTime;
    }
}

