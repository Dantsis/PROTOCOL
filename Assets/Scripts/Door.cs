using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Door : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite openSprite;
    public Sprite closedSprite;

    [Header("Componentes")]
    [Tooltip("Collider que bloquea el paso cuando la puerta está cerrada (NO Trigger)")]
    public Collider2D solidCollider;
    [Tooltip("SpriteRenderer de la hoja de puerta")]
    public SpriteRenderer sr; // si no lo asignás, se toma el del mismo GO

    [Header("Estado")]
    [Tooltip("Arranca cerrada por defecto")]
    public bool startClosed = true;

    [Header("Comportamiento")]
    [Tooltip("Retardo antes de intentar cerrar tras salir del área de proximidad")]
    public float closeDelay = 0.35f;

    // estado interno
    bool isOpen = false;
    bool isLocked = false; // lo maneja RoomController

    public bool IsLocked => isLocked;
    public float CloseDelay => closeDelay;

    void Reset()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        ApplyState(startClosed ? false : true, force: true);
    }

    // --- API pública para RoomController ---
    public void Lock()
    {
        isLocked = true;
        Close();
    }
    public void Unlock()
    {
        isLocked = false;
        // no abrimos automáticamente: el controlador decide
    }
    public void Open()
    {
        if (isLocked) return;
        ApplyState(true);
    }
    public void Close()
    {
        ApplyState(false);
    }

    void ApplyState(bool open, bool force = false)
    {
        if (!force && isOpen == open) return;
        isOpen = open;

        if (solidCollider) solidCollider.enabled = !open;

        if (sr)
        {
            if (open && openSprite) sr.sprite = openSprite;
            else if (!open && closedSprite) sr.sprite = closedSprite;
        }
    }
}

