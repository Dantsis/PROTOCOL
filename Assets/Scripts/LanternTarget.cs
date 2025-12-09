using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class LanternTarget : MonoBehaviour
{
    [HideInInspector] public LanternPuzzleRoom owner;

    // Mantengo este campo porque LanternPuzzleRoom lo asigna
    [HideInInspector] public GameObject playerBulletPrefab;

    private SpriteRenderer sr;
    private Sprite offSprite;
    private Sprite onSprite;

    public bool IsLit { get; private set; }

    bool playerInRange = false;

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

        // YA NO USAMOS owner.NotifyLanternLitChanged(),
        // porque LanternPuzzleRoom ya no lo necesita.
    }


    void Update()
    {
        if (!playerInRange) return;
        if (IsLit) return;

        // Activación por tecla E
        if (Input.GetKeyDown(KeyCode.E))
        {
            SetLit(true);
        }
    }

    void UpdateVisual()
    {
        if (!sr) return;
        sr.sprite = IsLit ? onSprite : offSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Detectar jugador proximity
        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb != null && hb.health != null && hb.health.isPlayer)
        {
            playerInRange = true;
            return;
        }

        // Si no es jugador, tal vez es una bala del jugador.
        if (!IsLit && playerBulletPrefab != null)
        {
            // Comparación simple por nombre del prefab instanciado
            if (other.gameObject.name.Contains(playerBulletPrefab.name))
            {
                SetLit(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb != null && hb.health != null && hb.health.isPlayer)
        {
            playerInRange = false;
        }
    }

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

        IsLit = true;
        UpdateVisual();
    }
}
