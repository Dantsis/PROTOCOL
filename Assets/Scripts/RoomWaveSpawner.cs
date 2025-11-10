using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class RoomWaveSpawner : MonoBehaviour
{
    [Header("Puertas de la sala")]
    public List<Door> doors = new List<Door>();

    [Header("Spawn points (opcionales)")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Enemigos")]
    public GameObject dronePrefab;
    public int initialCount = 2;
    public int waveSize = 3;
    public int maxTotal = 8;
    public float respawnDelay = 1f;

    [Header("Sprites de aparición")]
    public Sprite flashSprite;
    public Sprite normalSprite;
    public int blinkCount = 4;
    public float blinkInterval = 0.08f;

    [Header("Random dentro del collider (si no hay spawnPoints)")]
    public float roomMargin = 0.5f;
    public float spawnCheckRadius = 0.3f;
    public LayerMask blockLayers;
    public int maxAttemptsPerSpawn = 25;

    private bool encounterStarted = false;
    private bool roomCleared = false;
    private int totalSpawned = 0;
    private int pendingSpawns = 0;
    private readonly List<GameObject> alive = new List<GameObject>();
    private Collider2D roomTrigger;

    void Awake()
    {
        roomTrigger = GetComponent<Collider2D>();
        if (roomTrigger && !roomTrigger.isTrigger)
            roomTrigger.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (roomCleared || encounterStarted) return;

        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
            StartEncounter();
    }

    void StartEncounter()
    {
        encounterStarted = true;
        foreach (var d in doors) if (d) d.Lock();

        int toSpawn = Mathf.Min(initialCount, maxTotal - totalSpawned);
        StartCoroutine(SpawnWave(toSpawn));
        StartCoroutine(ManageWavesLoop());
    }

    IEnumerator ManageWavesLoop()
    {
        while (true)
        {
            CleanupDead();

            if (totalSpawned >= maxTotal && alive.Count == 0 && pendingSpawns == 0)
            {
                RoomCleared();
                yield break;
            }

            if (alive.Count == 0 && (totalSpawned + pendingSpawns) < maxTotal)
            {
                yield return new WaitForSeconds(respawnDelay);
                int remain = maxTotal - (totalSpawned + pendingSpawns);
                int count = Mathf.Min(waveSize, remain);
                StartCoroutine(SpawnWave(count));
            }

            yield return null;
        }
    }

    IEnumerator SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if ((totalSpawned + pendingSpawns) >= maxTotal) break;

            Vector2 pos;
            if (!TryFindSpawnPosition(out pos)) continue;

            pendingSpawns++;
            StartCoroutine(SpawnDrone(pos));
        }
        yield return null;
    }

    IEnumerator SpawnDrone(Vector2 pos)
    {
        var go = Instantiate(dronePrefab, pos, Quaternion.identity);
        alive.Add(go);
        totalSpawned++;

        var hp = go.GetComponent<Health>() ?? go.GetComponentInChildren<Health>();
        if (hp) hp.SetInvulnerable(true);

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        var behaviours = go.GetComponentsInChildren<MonoBehaviour>();
        foreach (var b in behaviours)
        {
            if (b != this && !(b is SpriteRenderer) && !(b is Health))
                b.enabled = false;
        }

        // --- Nuevo parpadeo entre sprites ---
        var sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr)
        {
            for (int i = 0; i < blinkCount; i++)
            {
                sr.sprite = flashSprite;
                yield return new WaitForSeconds(blinkInterval);
                sr.sprite = normalSprite;
                yield return new WaitForSeconds(blinkInterval);
            }
        }

        // Reactivar dron
        if (rb) rb.simulated = true;
        if (hp) hp.SetInvulnerable(false);

        foreach (var b in behaviours)
        {
            if (b != this && !(b is SpriteRenderer) && !(b is Health))
                b.enabled = true;
        }

        pendingSpawns = Mathf.Max(0, pendingSpawns - 1);
    }

    bool TryFindSpawnPosition(out Vector2 pos)
    {
        pos = Vector2.zero;
        if (!roomTrigger) return false;

        var b = roomTrigger.bounds;
        for (int i = 0; i < maxAttemptsPerSpawn; i++)
        {
            Vector2 candidate = new Vector2(
                UnityEngine.Random.Range(b.min.x + roomMargin, b.max.x - roomMargin),
                UnityEngine.Random.Range(b.min.y + roomMargin, b.max.y - roomMargin)
            );

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
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            if (alive[i] == null)
                alive.RemoveAt(i);
        }
    }

    void RoomCleared()
    {
        roomCleared = true;
        encounterStarted = false;
        foreach (var d in doors) if (d) { d.Unlock(); d.Open(); }
    }
}



