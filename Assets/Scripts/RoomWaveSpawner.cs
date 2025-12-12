using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class RoomWaveSpawner : MonoBehaviour
{

    public RoomDoorController doorController;

    public List<Transform> spawnPoints = new List<Transform>();

    public GameObject dronePrefab;
    public int initialCount = 2;
    public int waveSize = 3;
    public int maxTotal = 8;
    public float respawnDelay = 1f;

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
        if (roomTrigger && !roomTrigger.isTrigger) roomTrigger.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (encounterStarted || roomCleared) return;

        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
            StartEncounter();
    }

    void StartEncounter()
    {
        encounterStarted = true;

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

        yield return new WaitForSeconds(0.1f);

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
                Random.Range(b.min.x + roomMargin, b.max.x - roomMargin),
                Random.Range(b.min.y + roomMargin, b.max.y - roomMargin)
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
            if (alive[i] == null) alive.RemoveAt(i);
    }

    void RoomCleared()
    {
        roomCleared = true;

        if (doorController != null)
        {
            doorController.MarkCombatCleared();
            doorController.levelCompleted = true;    // ← AGREGADO
        }

        if (roomTrigger)
            roomTrigger.enabled = false;
    }

}