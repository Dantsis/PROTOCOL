using UnityEngine;

public class DropOnDeath : MonoBehaviour
{
    [Header("Prefab que se va a dropear")]
    public GameObject healthPickupPrefab;

    [Header("Probabilidad de dropeo (0 = nunca, 1 = siempre)")]
    [Range(0f, 1f)]
    public float dropChance = 0.3f;

    private void OnDestroy()
    {
        // Evitar spawns raros cuando se detiene el play o se borra en el editor
        if (!Application.isPlaying) return;
        if (healthPickupPrefab == null) return;

        if (Random.value <= dropChance)
        {
            Instantiate(
                healthPickupPrefab,
                transform.position,
                Quaternion.identity
            );
        }
    }
}
