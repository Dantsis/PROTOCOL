using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PressurePlate : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite idleSprite;    // placa sin peso
    public Sprite pressedSprite; // placa presionada

    [HideInInspector] public PressurePlatePuzzle puzzle;

    SpriteRenderer sr;
    Collider2D col;

    // Para soportar más de un bloque encima a la vez
    readonly HashSet<PushableBlock> blocksOnTop = new HashSet<PushableBlock>();

    public bool IsPressed => blocksOnTop.Count > 0;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        UpdateVisual();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var block = other.GetComponent<PushableBlock>() ?? other.GetComponentInParent<PushableBlock>();
        if (block != null)
        {
            blocksOnTop.Add(block);
            UpdateVisual();
            NotifyPuzzle();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var block = other.GetComponent<PushableBlock>() ?? other.GetComponentInParent<PushableBlock>();
        if (block != null && blocksOnTop.Remove(block))
        {
            UpdateVisual();
            NotifyPuzzle();
        }
    }

    void UpdateVisual()
    {
        if (!sr) return;

        if (IsPressed && pressedSprite != null)
            sr.sprite = pressedSprite;
        else if (!IsPressed && idleSprite != null)
            sr.sprite = idleSprite;
    }

    void NotifyPuzzle()
    {
        if (puzzle != null)
            puzzle.OnPlateStateChanged();
    }
}
