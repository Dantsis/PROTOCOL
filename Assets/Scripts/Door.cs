using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Door : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite openSprite;
    public Sprite closedSprite;

    [Header("Componentes")]
    public Collider2D solidCollider;
    public SpriteRenderer sr;

    [Header("Estado")]
    public bool startClosed = true;

    [Header("Comportamiento")]
    public float closeDelay = 0.35f;

    bool isOpen = false;
    bool isLocked = false;

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

    public void Lock()
    {
        isLocked = true;
        Close();
    }

    public void Unlock()
    {
        isLocked = false;
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

    // NUEVO — para que NPCDialogue pueda abrir puertas fácilmente
    public void OpenDoor()
    {
        Open();
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

