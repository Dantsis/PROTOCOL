using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Curación que otorga")]
    public int healAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Buscamos un Health en el objeto o en sus padres
        Health h = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
        if (h != null && h.isPlayer)
        {
            h.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
