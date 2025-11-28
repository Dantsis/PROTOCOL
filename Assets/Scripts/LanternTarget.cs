using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class LanternTarget : MonoBehaviour
{
    [HideInInspector] public LanternPuzzleRoom owner;
    [HideInInspector] public GameObject playerBulletPrefab;

    private SpriteRenderer sr;
    private Sprite offSprite;
    private Sprite onSprite;

    public bool IsLit { get; private set; }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    public void SetSprites(Sprite off, Sprite on)
    {
        offSprite = off;
        onSprite = on;
        UpdateVisual();
    }

    public void SetLit(bool lit)
    {
        IsLit = lit;
        UpdateVisual();

        if (IsLit && owner != null)
            owner.NotifyLanternLitChanged();
    }

    void UpdateVisual()
    {
        if (!sr) return;
        sr.sprite = IsLit ? onSprite : offSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsLit || owner == null || playerBulletPrefab == null)
            return;

        // Comprobación simple por nombre del prefab (sirve si las balas son instancias de ese prefab)
        if (other.gameObject.name.Contains(playerBulletPrefab.name))
        {
            SetLit(true);
        }
    }

    // --- Titileo al resolver el puzzle ---
    public void PlaySolvedBlink(float duration, float interval)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(BlinkRoutine(duration, interval));
    }

    IEnumerator BlinkRoutine(float duration, float interval)
    {
        float t = 0f;
        bool useOn = true;

        while (t < duration)
        {
            useOn = !useOn;

            if (sr != null)
                sr.sprite = useOn ? onSprite : offSprite;

            yield return new WaitForSeconds(interval);
            t += interval;
        }

        // Al final lo dejamos encendido
        IsLit = true;
        UpdateVisual();
    }
}

