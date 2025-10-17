using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class RoomWaveSpawner : MonoBehaviour
{
    [Header("Puertas de la sala")]
    public List<Door> doors = new List<Door>();

    [Header("Enemigo a spawnear")]
    public GameObject dronePrefab;

    [Header("Oleadas")]
    [Tooltip("Cuántos drones aparecen al iniciar el encuentro")]
    public int initialCount = 2;

    [Tooltip("Tamaño de cada nueva oleada")]
    public int waveSize = 3;

    [Tooltip("Cantidad TOTAL máxima (inicial + oleadas)")]
    public int maxTotal = 8;

    [Tooltip("Espera antes de cada nueva oleada")]
    public float respawnDelay = 1.0f;

    [Header("Spawneo aleatorio dentro del collider de la sala")]
    [Tooltip("Margen interior para no pegarse a las paredes")]
    public float roomMargin = 0.5f;

    [Tooltip("Radio para chequear si el lugar está libre")]
    public float spawnCheckRadius = 0.3f;

    [Tooltip("Capas que bloquean el spawn (Walls, Props sólidos, etc.)")]
    public LayerMask blockLayers;

    [Tooltip("Intentos máximos para encontrar una posición libre por enemigo")]
    public int maxAttemptsPerSpawn = 25;

    [Header("Flash de aviso (opcional)")]
    [Tooltip("Sprite blanco (o similar) para parpadear en el punto de spawn")]
    public Sprite flashSprite;
    public Color flashColor = Color.white;
    public float flashScale = 1f;
    public int flashCount = 3;
    public float flashInterval = 0.08f;
    public float flashYOffset = 0f;

    // Estado interno
    bool encounterStarted = false;
    bool roomCleared = false;

    int totalSpawned = 0;      // instanciados en total
    int pendingSpawns = 0;     // flashes en progreso que todavía no instanciaron
    readonly List<GameObject> alive = new List<GameObject>();

    Collider2D roomTrigger; // IsTrigger = true

    void Reset()
    {
        roomTrigger = GetComponent<Collider2D>();
        if (roomTrigger) roomTrigger.isTrigger = true;
    }

    void Awake()
    {
        roomTrigger = GetComponent<Collider2D>();
        if (roomTrigger && !roomTrigger.isTrigger) roomTrigger.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (roomCleared || encounterStarted) return;

        // Detectar jugador (usando Hurtbox -> Health.isPlayer)
        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            StartEncounter();
        }
    }

    void StartEncounter()
    {
        encounterStarted = true;

        // Cerrar + bloquear puertas
        foreach (var d in doors)
        {
            if (!d) continue;
            d.Lock();
        }

        // Oleada inicial
        int toSpawn = Mathf.Min(initialCount, maxTotal - totalSpawned - pendingSpawns);
        StartCoroutine(SpawnWave(toSpawn));

        // Comenzar loop
        StartCoroutine(ManageWavesLoop());
    }

    IEnumerator ManageWavesLoop()
    {
        while (true)
        {
            CleanupDead();

            // ¿Se spawneó todo y no queda nadie vivo ni pendiente? -> sala limpia
            if (totalSpawned >= maxTotal && alive.Count == 0 && pendingSpawns == 0)
            {
                RoomCleared();
                yield break;
            }

            // Si no hay vivos y aún faltan por spawnear, lanzar nueva oleada
            if (alive.Count == 0 && (totalSpawned + pendingSpawns) < maxTotal)
            {
                yield return new WaitForSeconds(respawnDelay);

                int remain = maxTotal - (totalSpawned + pendingSpawns);
                int count = Mathf.Min(waveSize, remain);

                yield return SpawnWave(count);
            }

            yield return null;
        }
    }

    IEnumerator SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if ((totalSpawned + pendingSpawns) >= maxTotal) break;

            // Buscar posición aleatoria válida
            Vector2 pos;
            if (!TryFindSpawnPosition(out pos))
                continue; // si no se encuentra lugar, salteamos este intento

            // Lanzar coroutine de flash + spawn
            pendingSpawns++;
            StartCoroutine(FlashAndSpawn(pos));
        }
        yield return null;
    }

    bool TryFindSpawnPosition(out Vector2 pos)
    {
        pos = Vector2.zero;
        if (!roomTrigger) return false;

        var b = roomTrigger.bounds;

        for (int attempt = 0; attempt < maxAttemptsPerSpawn; attempt++)
        {
            Vector2 candidate = new Vector2(
                UnityEngine.Random.Range(b.min.x + roomMargin, b.max.x - roomMargin),
                UnityEngine.Random.Range(b.min.y + roomMargin, b.max.y - roomMargin)
            );

            // Chequear si la zona está libre
            var hit = Physics2D.OverlapCircle(candidate, spawnCheckRadius, blockLayers);
            if (hit == null)
            {
                pos = candidate;
                return true;
            }
        }
        return false; // no se encontró lugar libre tras N intentos
    }

    IEnumerator FlashAndSpawn(Vector2 pos)
    {
        GameObject flash = null;
        SpriteRenderer fr = null;

        if (flashSprite != null)
        {
            flash = new GameObject("SpawnFlash");
            fr = flash.AddComponent<SpriteRenderer>();
            fr.sprite = flashSprite;
            fr.color = flashColor;
            fr.sortingOrder = 9999; // delante de todo
            flash.transform.position = (Vector3)pos + Vector3.up * flashYOffset;
            flash.transform.localScale = Vector3.one * flashScale;

            for (int i = 0; i < flashCount; i++)
            {
                fr.enabled = true; yield return new WaitForSeconds(flashInterval);
                fr.enabled = false; yield return new WaitForSeconds(flashInterval);
            }
        }
        else
        {
            // Sin sprite de flash, igual damos timing antes del spawn
            yield return new WaitForSeconds(flashCount * flashInterval * 2f);
        }

        if (flash) Destroy(flash);

        // Instanciar enemigo
        if (dronePrefab != null)
        {
            var go = Instantiate(dronePrefab, pos, Quaternion.identity);
            alive.Add(go);
            totalSpawned++;
        }

        pendingSpawns = Mathf.Max(0, pendingSpawns - 1);
    }

    void CleanupDead()
    {
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            if (alive[i] == null)
            {
                alive.RemoveAt(i);
            }
            else
            {
                var hp = alive[i].GetComponent<Health>() ?? alive[i].GetComponentInChildren<Health>();
                if (hp != null && !hp.IsAlive)
                {
                    // si tu Health destruye el GO al morir, se limpiará en el próximo ciclo
                }
            }
        }
    }

    void RoomCleared()
    {
        roomCleared = true;
        encounterStarted = false;

        // Abrir + desbloquear puertas
        foreach (var d in doors)
        {
            if (!d) continue;
            d.Unlock();
            d.Open();
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!roomTrigger) roomTrigger = GetComponent<Collider2D>();
        if (!roomTrigger) return;

        var b = roomTrigger.bounds;
        Vector3 size = b.size - new Vector3(roomMargin * 2f, roomMargin * 2f, 0f);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.15f);
        Gizmos.DrawCube(b.center, size);
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 1f);
        Gizmos.DrawWireCube(b.center, size);
    }
#endif
}




