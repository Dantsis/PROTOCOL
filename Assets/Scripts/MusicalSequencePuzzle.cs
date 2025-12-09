using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MusicalSequencePuzzle : MonoBehaviour
{
    [Header("Puertas de la sala")]
    public List<Door> doors = new List<Door>();

    [Header("Pads de SECUENCIA (los que solo muestran el patrón)")]
    public SpriteRenderer[] sequencePads = new SpriteRenderer[4];

    [Header("Pads de ENTRADA (donde se para el jugador)")]
    public MusicPadInput[] inputPads = new MusicPadInput[4];

    [Header("Indicadores de progreso (3 secuencias)")]
    public SpriteRenderer[] progressTiles = new SpriteRenderer[3];

    [Header("Sprites de pads")]
    public Sprite padIdleSprite;
    public Sprite padActiveSprite;

    [Header("Sprites de indicadores")]
    public Sprite progressNeutralSprite;
    public Sprite progressOkSprite;
    public Sprite progressFailSprite;

    [Header("Secuencias")]
    public int[] sequence1 = new int[4];
    public int[] sequence2 = new int[4];
    public int[] sequence3 = new int[4];

    [Header("Tiempos")]
    public float showStepTime = 0.4f;
    public float showStepDelay = 0.15f;
    public float retryDelay = 0.7f;

    // --- Estado interno ---
    Collider2D roomTrigger;
    List<int[]> sequences = new List<int[]>();
    int currentSequenceIndex = 0;
    List<int> currentInput = new List<int>();

    bool puzzleStarted = false;

    // IMPORTANTE: lo usa Eddie
    public bool puzzleSolved = false;

    bool showingSequence = false;
    bool acceptingInput = false;

    void Reset()
    {
        roomTrigger = GetComponent<Collider2D>();
        if (roomTrigger) roomTrigger.isTrigger = true;
    }

    void Awake()
    {
        roomTrigger = GetComponent<Collider2D>();
        if (roomTrigger && !roomTrigger.isTrigger)
            roomTrigger.isTrigger = true;

        sequences = new List<int[]> { sequence1, sequence2, sequence3 };

        for (int i = 0; i < inputPads.Length; i++)
        {
            if (inputPads[i] != null)
            {
                inputPads[i].puzzle = this;
                inputPads[i].padIndex = i;
            }
        }

        ResetAllVisuals();
    }

    void ResetAllVisuals()
    {
        foreach (var sr in sequencePads)
            if (sr != null && padIdleSprite != null)
                sr.sprite = padIdleSprite;

        foreach (var pad in inputPads)
            if (pad != null)
                pad.SetSprite(padIdleSprite);

        for (int i = 0; i < progressTiles.Length; i++)
            if (progressTiles[i] != null && progressNeutralSprite != null)
                progressTiles[i].sprite = progressNeutralSprite;
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
        currentSequenceIndex = 0;
        currentInput.Clear();
        ResetAllVisuals();

        foreach (var d in doors)
            if (d != null) d.Lock();

        StartCoroutine(ShowCurrentSequence());
    }

    IEnumerator ShowCurrentSequence()
    {
        acceptingInput = false;
        showingSequence = true;
        currentInput.Clear();

        foreach (var pad in inputPads)
            if (pad != null)
                pad.SetSprite(padIdleSprite);

        yield return new WaitForSeconds(0.3f);

        if (currentSequenceIndex < 0 || currentSequenceIndex >= sequences.Count)
        {
            showingSequence = false;
            yield break;
        }

        int[] seq = sequences[currentSequenceIndex];

        for (int i = 0; i < seq.Length; i++)
        {
            int padId = Mathf.Clamp(seq[i], 0, sequencePads.Length - 1);

            for (int j = 0; j < sequencePads.Length; j++)
            {
                if (sequencePads[j] != null && padIdleSprite != null)
                    sequencePads[j].sprite = padIdleSprite;
            }

            if (sequencePads[padId] != null && padActiveSprite != null)
                sequencePads[padId].sprite = padActiveSprite;

            yield return new WaitForSeconds(showStepTime);
        }

        for (int j = 0; j < sequencePads.Length; j++)
            if (sequencePads[j] != null && padIdleSprite != null)
                sequencePads[j].sprite = padIdleSprite;

        showingSequence = false;
        acceptingInput = true;
    }

    // llamado por MusicPadInput
    public void OnPadPressed(int padIndex, MusicPadInput pad)
    {
        if (!puzzleStarted || puzzleSolved || !acceptingInput) return;
        if (padIndex < 0 || padIndex >= 4) return;

        if (pad != null)
            pad.Flash(padActiveSprite, padIdleSprite, 0.15f);

        currentInput.Add(padIndex);

        if (currentInput.Count >= 4)
        {
            acceptingInput = false;
            StartCoroutine(EvaluateCurrentInput());
        }
    }

    IEnumerator EvaluateCurrentInput()
    {
        int[] seq = sequences[currentSequenceIndex];

        bool correct = true;
        for (int i = 0; i < seq.Length; i++)
        {
            if (currentInput[i] != seq[i])
            {
                correct = false;
                break;
            }
        }

        if (currentSequenceIndex < progressTiles.Length)
        {
            var sr = progressTiles[currentSequenceIndex];
            if (sr != null)
                sr.sprite = correct ? progressOkSprite : progressFailSprite;
        }

        if (correct)
        {
            yield return new WaitForSeconds(0.25f);

            currentSequenceIndex++;

            // --- CAMBIO IMPORTANTE ---
            if (currentSequenceIndex >= sequences.Count)
            {
                SolvePuzzle();   // ← Ahora esto marca puzzleSolved al toque
                yield break;
            }
            else
            {
                yield return new WaitForSeconds(0.35f);
                StartCoroutine(ShowCurrentSequence());
            }
        }
        else
        {
            yield return new WaitForSeconds(retryDelay);

            if (currentSequenceIndex < progressTiles.Length)
            {
                var sr = progressTiles[currentSequenceIndex];
                if (sr != null && progressNeutralSprite != null)
                    sr.sprite = progressNeutralSprite;
            }

            currentInput.Clear();
            StartCoroutine(ShowCurrentSequence());
        }
    }

    void SolvePuzzle()
    {
        if (puzzleSolved) return;

        puzzleSolved = true; // ← AHORA SE MARCA COMO RESUELTO

        if (roomTrigger)
            roomTrigger.enabled = false;

        // Ya no abre puertas aquí (Eddie lo maneja)
    }
}
