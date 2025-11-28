using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MusicalSequencePuzzle : MonoBehaviour
{
    [Header("Puertas de la sala")]
    public List<Door> doors = new List<Door>();

    [Header("Pads de SECUENCIA (los que solo muestran el patrón)")]
    [Tooltip("4 pads que se prenden en orden para mostrar la secuencia")]
    public SpriteRenderer[] sequencePads = new SpriteRenderer[4];

    [Header("Pads de ENTRADA (donde se para el jugador)")]
    [Tooltip("4 pads con MusicPadInput (el jugador pisa para copiar la secuencia)")]
    public MusicPadInput[] inputPads = new MusicPadInput[4];

    [Header("Indicadores de progreso (3 secuencias)")]
    [Tooltip("Sprites en el piso que muestran si cada secuencia está bien/mal")]
    public SpriteRenderer[] progressTiles = new SpriteRenderer[3];

    [Header("Sprites de pads")]
    public Sprite padIdleSprite;   // gris o apagado
    public Sprite padActiveSprite; // rojo (cuando se enciende)

    [Header("Sprites de indicadores de progreso")]
    public Sprite progressNeutralSprite; // estado neutro
    public Sprite progressOkSprite;      // verde
    public Sprite progressFailSprite;    // rojo

    [Header("Secuencias (usar valores 0,1,2,3)")]
    [Tooltip("Primera secuencia de 4 pasos (0..3 para cada pad)")]
    public int[] sequence1 = new int[4];
    [Tooltip("Segunda secuencia")]
    public int[] sequence2 = new int[4];
    [Tooltip("Tercera secuencia")]
    public int[] sequence3 = new int[4];

    [Header("Tiempos")]
    [Tooltip("Tiempo que cada pad de la secuencia queda prendido")]
    public float showStepTime = 0.4f;

    [Tooltip("Tiempo entre pasos de la secuencia")]
    public float showStepDelay = 0.15f;

    [Tooltip("Tiempo antes de volver a mostrar la secuencia si el jugador falla")]
    public float retryDelay = 0.7f;

    // --- Estado interno ---
    Collider2D roomTrigger;
    List<int[]> sequences = new List<int[]>();
    int currentSequenceIndex = 0;
    List<int> currentInput = new List<int>();

    bool puzzleStarted = false;
    bool puzzleSolved = false;
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

        // Asignar este puzzle a los pads de entrada
        for (int i = 0; i < inputPads.Length; i++)
        {
            if (inputPads[i] != null)
            {
                inputPads[i].puzzle = this;
                inputPads[i].padIndex = i;
            }
        }

        // Estado inicial de sprites
        ResetAllVisuals();
    }

    void ResetAllVisuals()
    {
        // Pads de secuencia
        foreach (var sr in sequencePads)
        {
            if (sr != null && padIdleSprite != null)
                sr.sprite = padIdleSprite;
        }

        // Pads de entrada
        foreach (var pad in inputPads)
        {
            if (pad != null)
                pad.SetSprite(padIdleSprite);
        }

        // Indicadores de progreso
        for (int i = 0; i < progressTiles.Length; i++)
        {
            if (progressTiles[i] != null && progressNeutralSprite != null)
                progressTiles[i].sprite = progressNeutralSprite;
        }
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
        currentSequenceIndex = 0;
        currentInput.Clear();
        ResetAllVisuals();

        // Cerrar + bloquear puertas
        foreach (var d in doors)
        {
            if (d != null)
                d.Lock();
        }

        StartCoroutine(ShowCurrentSequence());
    }

    IEnumerator ShowCurrentSequence()
    {
        acceptingInput = false;
        showingSequence = true;
        currentInput.Clear();

        // Reset visual de pads de entrada
        foreach (var pad in inputPads)
        {
            if (pad != null)
                pad.SetSprite(padIdleSprite);
        }

        // Seguridad
        if (currentSequenceIndex < 0 || currentSequenceIndex >= sequences.Count)
            yield break;

        int[] seq = sequences[currentSequenceIndex];

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < seq.Length; i++)
        {
            int padId = Mathf.Clamp(seq[i], 0, sequencePads.Length - 1);

            // Todos idle
            for (int j = 0; j < sequencePads.Length; j++)
            {
                if (sequencePads[j] != null && padIdleSprite != null)
                    sequencePads[j].sprite = padIdleSprite;
            }

            // Encendemos el pad correspondiente
            if (sequencePads[padId] != null && padActiveSprite != null)
                sequencePads[padId].sprite = padActiveSprite;

            yield return new WaitForSeconds(showStepTime);
        }

        // Volvemos a estado idle en los pads de secuencia
        for (int j = 0; j < sequencePads.Length; j++)
        {
            if (sequencePads[j] != null && padIdleSprite != null)
                sequencePads[j].sprite = padIdleSprite;
        }

        showingSequence = false;
        acceptingInput = true;
    }

    // Llamado por los MusicPadInput cuando el jugador pisa un pad
    public void OnPadPressed(int padIndex, MusicPadInput pad)
    {
        if (!puzzleStarted || puzzleSolved || !acceptingInput) return;
        if (padIndex < 0 || padIndex >= 4) return;

        // Feedback visual en el pad de entrada
        if (pad != null)
            pad.Flash(padActiveSprite, padIdleSprite, 0.15f);

        currentInput.Add(padIndex);

        // Si ya metió los 4 pasos, evaluamos
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

        // Indicador de esta secuencia (0,1,2)
        if (currentSequenceIndex < progressTiles.Length)
        {
            var sr = progressTiles[currentSequenceIndex];
            if (sr != null)
            {
                if (correct && progressOkSprite != null)
                    sr.sprite = progressOkSprite;
                else if (!correct && progressFailSprite != null)
                    sr.sprite = progressFailSprite;
            }
        }

        if (correct)
        {
            // Guardamos este tile en verde para siempre
            yield return new WaitForSeconds(0.25f);

            currentSequenceIndex++;

            // ¿Completó las 3 secuencias?
            if (currentSequenceIndex >= sequences.Count)
            {
                SolvePuzzle();
                yield break;
            }
            else
            {
                // Pasar a la siguiente secuencia
                yield return new WaitForSeconds(0.35f);
                StartCoroutine(ShowCurrentSequence());
            }
        }
        else
        {
            // Secuencia incorrecta: esperar, volver a neutral y repetir misma secuencia
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
        puzzleSolved = true;
        acceptingInput = false;

        // Abrir + desbloquear puertas
        foreach (var d in doors)
        {
            if (d != null)
            {
                d.Unlock();
                d.Open();
            }
        }

        if (roomTrigger)
            roomTrigger.enabled = false;
    }
}
