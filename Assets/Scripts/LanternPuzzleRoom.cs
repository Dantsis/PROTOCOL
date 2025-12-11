using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LanternPuzzleRoom : MonoBehaviour
{
    public RoomDoorController doorController;

    [Header("Puertas de la sala")]
    public List<Door> doors = new List<Door>();

    [Header("Requisito Eddie")]
    public bool requireEddieToOpenDoors = true;

    bool roomCleared = false;
    bool waitingForEddie = false;
    bool eddieHasBeenTalkedAfterPuzzle = false;

    [Header("Faroles")]
    public GameObject lanternPrefab;
    public Sprite lanternOffSprite;
    public Sprite lanternOnSprite;
    public int lanternCount = 3;

    [Header("Tiempo del puzzle")]
    public float lanternLifetime = 2.5f;
    public float respawnDelay = 1.0f;

    [Header("Zona de spawn")]
    public Collider2D roomArea;
    public float roomMargin = 0.5f;

    [Header("Prefab bala jugador")]
    public GameObject playerBulletPrefab;

    [Header("Efecto completado")]
    public float solvedBlinkDuration = 1.5f;
    public float solvedBlinkInterval = 0.08f;

    Collider2D roomTrigger;
    readonly List<LanternTarget> currentLanterns = new List<LanternTarget>();
    bool puzzleStarted = false;
    public bool puzzleSolved = false;

    Coroutine puzzleRoutine;

    void Awake()
    {
        roomTrigger = GetComponent<Collider2D>();
        if (roomTrigger && !roomTrigger.isTrigger)
            roomTrigger.isTrigger = true;

        if (!roomArea) roomArea = roomTrigger;
    }

    void OnEnable()
    {
        NPCDialogue.OnEddieTalked += OnEddieTalked;
    }

    void OnDisable()
    {
        NPCDialogue.OnEddieTalked -= OnEddieTalked;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (puzzleStarted || puzzleSolved) return;

        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
            StartPuzzle();
    }

    void StartPuzzle()
    {
        puzzleStarted = true;

        foreach (var d in doors)
            if (d != null) d.Lock();

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

            ClearLanterns();
            yield return new WaitForSeconds(respawnDelay);
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
            if (lt == null || !lt.IsLit) return false;

        return true;
    }

    void SolvePuzzle()
    {
        if (puzzleSolved) return;
        puzzleSolved = true;
        roomCleared = true;

        if (puzzleRoutine != null)
            StopCoroutine(puzzleRoutine);

        StartCoroutine(SolvedSequence());
    }

    IEnumerator SolvedSequence()
    {
        foreach (var lt in currentLanterns)
        {
            if (lt != null)
            {
                lt.SetLit(true);
                lt.PlaySolvedBlink(solvedBlinkDuration, solvedBlinkInterval);
            }
        }

        yield return new WaitForSeconds(solvedBlinkDuration);

        ClearLanterns();

        if (requireEddieToOpenDoors)
        {
            if (eddieHasBeenTalkedAfterPuzzle)
            {
                TriggerDoorController();
            }
            else
            {
                waitingForEddie = true;
            }
        }
        else
        {
            TriggerDoorController();
        }

        if (roomTrigger)
            roomTrigger.enabled = false;
    }

    void TriggerDoorController()
    {
        if (doorController != null)
            doorController.MarkPuzzleCleared();
    }

    public void NotifyLanternLitChanged()
    {
        if (puzzleStarted && !puzzleSolved && AllLanternsLit())
        {
            SolvePuzzle();
        }
    }

    void OnEddieTalked()
    {
        if (!requireEddieToOpenDoors) return;
        if (!roomCleared) return;

        eddieHasBeenTalkedAfterPuzzle = true;

        if (waitingForEddie)
        {
            waitingForEddie = false;
            TriggerDoorController();
        }
    }
}