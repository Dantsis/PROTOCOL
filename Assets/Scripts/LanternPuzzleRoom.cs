using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LanternPuzzleRoom : MonoBehaviour
{
    [Header("Puertas de la sala")]
    public List<Door> doors = new List<Door>();

    [Header("Faroles")]
    public GameObject lanternPrefab;
    public Sprite lanternOffSprite;
    public Sprite lanternOnSprite;

    [Tooltip("Cuántos faroles deben estar encendidos simultáneamente")]
    public int lanternCount = 3;

    [Header("Tiempo del puzzle")]
    [Tooltip("Duración (segundos) antes de que los faroles desaparezcan si fallás")]
    public float lanternLifetime = 2.5f;

    [Tooltip("Tiempo entre intentos")]
    public float respawnDelay = 1.0f;

    [Header("Zona de spawn (si se deja vacío usa el collider de la sala)")]
    public Collider2D roomArea;
    public float roomMargin = 0.5f;

    [Header("Prefab de bala del jugador (arrastrar aquí)")]
    public GameObject playerBulletPrefab;

    [Header("Efecto al completar puzzle")]
    [Tooltip("Cuánto dura el titileo de los faroles al resolver el puzzle")]
    public float solvedBlinkDuration = 1.5f;

    [Tooltip("Cada cuántos segundos cambian de sprite al titilar")]
    public float solvedBlinkInterval = 0.08f;

    // Estado interno
    private Collider2D roomTrigger;
    private readonly List<LanternTarget> currentLanterns = new List<LanternTarget>();
    private bool puzzleStarted = false;
    private bool puzzleSolved = false;
    private Coroutine puzzleRoutine;

    void Reset()
    {
        roomTrigger = GetComponent<Collider2D>();
        if (roomTrigger) roomTrigger.isTrigger = true;
        roomArea = roomTrigger;
    }

    void Awake()
    {
        roomTrigger = GetComponent<Collider2D>();
        if (roomTrigger && !roomTrigger.isTrigger)
            roomTrigger.isTrigger = true;

        if (!roomArea) roomArea = roomTrigger;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (puzzleStarted || puzzleSolved) return;

        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            StartPuzzle();
        }
    }

    void StartPuzzle()
    {
        puzzleStarted = true;

        // Cerrar + bloquear todas las puertas
        foreach (var d in doors)
        {
            if (d != null)
                d.Lock();
        }

        puzzleRoutine = StartCoroutine(PuzzleLoop());
    }

    IEnumerator PuzzleLoop()
    {
        while (!puzzleSolved)
        {
            SpawnLanterns();

            float t = 0f;
            bool success = false;

            while (t < lanternLifetime && !puzzleSolved)
            {
                if (AllLanternsLit())
                {
                    success = true;
                    break;
                }

                t += Time.deltaTime;
                yield return null;
            }

            if (success)
            {
                SolvePuzzle();
                yield break;
            }
            else
            {
                ClearLanterns();
                yield return new WaitForSeconds(respawnDelay);
            }
        }
    }

    void SpawnLanterns()
    {
        ClearLanterns();
        if (!roomArea) return;

        Bounds b = roomArea.bounds;

        for (int i = 0; i < lanternCount; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(b.min.x + roomMargin, b.max.x - roomMargin),
                Random.Range(b.min.y + roomMargin, b.max.y - roomMargin)
            );

            GameObject go = Instantiate(lanternPrefab, pos, Quaternion.identity);
            LanternTarget lt = go.GetComponent<LanternTarget>();

            if (lt != null)
            {
                lt.owner = this;
                lt.playerBulletPrefab = playerBulletPrefab;
                lt.SetSprites(lanternOffSprite, lanternOnSprite);
                lt.SetLit(false);
                currentLanterns.Add(lt);
            }
            else
            {
                Debug.LogWarning("LanternPrefab no tiene LanternTarget.", go);
            }
        }
    }

    void ClearLanterns()
    {
        foreach (var lt in currentLanterns)
        {
            if (lt != null)
                Destroy(lt.gameObject);
        }
        currentLanterns.Clear();
    }

    bool AllLanternsLit()
    {
        if (currentLanterns.Count == 0) return false;

        foreach (var lt in currentLanterns)
        {
            if (lt == null || !lt.IsLit)
                return false;
        }
        return true;
    }

    void SolvePuzzle()
    {
        if (puzzleSolved) return;
        puzzleSolved = true;

        if (puzzleRoutine != null)
            StopCoroutine(puzzleRoutine);

        StartCoroutine(SolvedSequence());
    }

    IEnumerator SolvedSequence()
    {
        // Aseguramos que todos estén encendidos y empezamos el titileo
        foreach (var lt in currentLanterns)
        {
            if (lt != null)
            {
                lt.SetLit(true);
                lt.PlaySolvedBlink(solvedBlinkDuration, solvedBlinkInterval);
            }
        }

        // Esperamos mientras titilan
        yield return new WaitForSeconds(solvedBlinkDuration);

        // Limpiamos faroles
        ClearLanterns();

        // Abrimos + desbloqueamos puertas
        foreach (var d in doors)
        {
            if (d != null)
            {
                d.Unlock();
                d.Open();
            }
        }

        // Desactivamos el trigger de la sala para que no se repita
        if (roomTrigger)
            roomTrigger.enabled = false;
    }

    public void NotifyLanternLitChanged()
    {
        if (puzzleStarted && !puzzleSolved && AllLanternsLit())
        {
            SolvePuzzle();
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!roomArea) roomArea = GetComponent<Collider2D>();
        if (!roomArea) return;

        var b = roomArea.bounds;
        Vector3 size = b.size - new Vector3(roomMargin * 2f, roomMargin * 2f, 0f);

        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawCube(b.center, size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(b.center, size);
    }
#endif
}

