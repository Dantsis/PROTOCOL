using UnityEngine;

public class PaperAmmoPickup : MonoBehaviour
{
    [Header("Cantidad de munición que da este pickup")]
    public int ammoAmount = 3;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Buscamos PlayerShoot en el objeto o en sus padres
        var shooter = other.GetComponent<PlayerShoot>() ?? other.GetComponentInParent<PlayerShoot>();
        if (shooter != null)
        {
            shooter.AddAmmo(ammoAmount);
            Destroy(gameObject);
        }
    }
}

