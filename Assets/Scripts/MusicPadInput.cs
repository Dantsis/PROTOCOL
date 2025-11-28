using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class MusicPadInput : MonoBehaviour
{
    [HideInInspector] public MusicalSequencePuzzle puzzle;
    [HideInInspector] public int padIndex;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    public void SetSprite(Sprite s)
    {
        if (sr != null && s != null)
            sr.sprite = s;
    }

    public void Flash(Sprite onSprite, Sprite offSprite, float duration)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(FlashRoutine(onSprite, offSprite, duration));
    }

    IEnumerator FlashRoutine(Sprite onSprite, Sprite offSprite, float duration)
    {
        if (sr != null && onSprite != null)
            sr.sprite = onSprite;

        yield return new WaitForSeconds(duration);

        if (sr != null && offSprite != null)
            sr.sprite = offSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (puzzle == null) return;

        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            puzzle.OnPadPressed(padIndex, this);
        }
    }
}
