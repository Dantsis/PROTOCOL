using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AmmoItemSpawner : MonoBehaviour
{
    [Header("Prefab de Ammo en el piso")]
    public GameObject ammoPickupPrefab;      // Ej: PaperAmmoPickup prefab

    [Header("Cantidad y respawn")]
    [Tooltip("Cuántos ítems se generan al inicio.")]
    public int initialSpawn = 3;

    [Tooltip("Máximo de ítems simultáneos en el área.")]
    public int maxSimultaneous = 5;

    [Tooltip("Tiempo de espera antes de intentar respawnear un ítem después de que falten.")]
    public float respawnDelay = 4f;

    [Header("Random dentro del collider")]
    [Tooltip("Margen interno para no spawnear pegado a las paredes del collider.")]
    public float roomMargin = 0.5f;

    [Tooltip("Radio que se usa para chequear que no haya paredes/obstáculos en el punto de spawn.")]
    public float spawnCheckRadius = 0.3f;

    [Tooltip("Capas consideradas como bloqueo para el spawn (ej: Walls).")]
    public LayerMask blockLayers;

    [Tooltip("Intentos máximos por ítem para encontrar una posición válida.")]
    public int maxAttemptsPerSpawn = 20;

    private Collider2D areaCollider;
    private readonly List<GameObject> aliveAmmo = new List<GameObject>();
    private bool running = false;

    void Awake()
    {
        areaCollider = GetComponent<Collider2D>();
        if (areaCollider && !areaCollider.isTrigger)
            areaCollider.isTrigger = true; // no hace falta colisionar físicamente
    }

    void Start()
    {
        if (!ammoPickupPrefab)
        {
            Debug.LogWarning($"[AmmoItemSpawner] No hay prefab asignado en '{name}'.");
            enabled = false;
            return;
        }

        // Spawnear los iniciales
        int toSpawn = Mathf.Clamp(initialSpawn, 0, maxSimultaneous);
        for (int i = 0; i < toSpawn; i++)
        {
            TrySpawnOne();
        }

        // Loop de respawn
        StartCoroutine(RespawnLoop());
    }

    IEnumerator RespawnLoop()
    {
        running = true;
        while (running)
        {
            CleanupDead();

            // Si hay hueco para spawnear más
            if (aliveAmmo.Count < maxSimultaneous)
            {
                // Esperar el delay antes de intentar respawn
                yield return new WaitForSeconds(respawnDelay);

                CleanupDead(); // limpiar por las dudas

                // Chequear de nuevo (podría haber cambiado en ese tiempo)
                while (aliveAmmo.Count < maxSimultaneous)
                {
                    if (!TrySpawnOne())
                        break; // si no encontramos posición, salimos del while

                    CleanupDead();
                }
            }

            yield return null;
        }
    }

    bool TrySpawnOne()
    {
        if (!areaCollider || !ammoPickupPrefab) return false;

        Vector2 pos;
        if (!TryFindSpawnPosition(out pos))
            return false;

        var go = Instantiate(ammoPickupPrefab, pos, Quaternion.identity);
        aliveAmmo.Add(go);
        return true;
    }

    bool TryFindSpawnPosition(out Vector2 pos)
    {
        pos = Vector2.zero;
        if (!areaCollider) return false;

        Bounds b = areaCollider.bounds;

        for (int i = 0; i < maxAttemptsPerSpawn; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(b.min.x + roomMargin, b.max.x - roomMargin),
                Random.Range(b.min.y + roomMargin, b.max.y - roomMargin)
            );

            // Chequear que no haya paredes / cosas que bloqueen
            if (!Physics2D.OverlapCircle(candidate, spawnCheckRadius, blockLayers))
            {
                pos = candidate;
                return true;
            }
        }

        return false;
    }

    void CleanupDead()
    {
        for (int i = aliveAmmo.Count - 1; i >= 0; i--)
        {
            if (aliveAmmo[i] == null)
                aliveAmmo.RemoveAt(i);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!areaCollider) areaCollider = GetComponent<Collider2D>();
        if (!areaCollider) return;

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.25f);
        Gizmos.DrawCube(areaCollider.bounds.center, areaCollider.bounds.size);

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.7f);
        Gizmos.DrawWireCube(areaCollider.bounds.center, areaCollider.bounds.size);
    }
}
