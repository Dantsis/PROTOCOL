using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class SpikeTrap : MonoBehaviour
{
    [Header("Sprites (0=Arriba, 1=Intermedio, 2=Transición, 3=Abajo)")]
    public Sprite[] sprites = new Sprite[4];

    [Header("Tiempos (segundos)")]
    public float upHold = 1.2f;
    public float downHold = 1.2f;
    public float transitionStep = 0.12f;

    [Header("Daño")]
    public int damage = 1;
    public bool hurtOnTransitions = false;

    [Header("Estado inicial")]
    public bool startUp = false;

    [Header("Delay inicial")]
    public float startDelay = 0f;   // nuevo campo para desfasar trampas

    private SpriteRenderer sr;
    private Collider2D hitbox;
    private bool dangerous = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<Collider2D>();
        if (hitbox) hitbox.isTrigger = true;
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Cycle());
    }

    IEnumerator Cycle()
    {
        // 🔹 delay inicial para desincronizar varias trampas
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        // Estado inicial
        if (startUp) SetFrame(0, true);
        else SetFrame(3, false);

        yield return new WaitForSeconds(0.01f);

        while (true)
        {
            yield return StartCoroutine(GoUp());
            yield return new WaitForSeconds(upHold);

            yield return StartCoroutine(GoDown());
            yield return new WaitForSeconds(downHold);
        }
    }

    IEnumerator GoUp()
    {
        SetFrame(2, hurtOnTransitions); yield return new WaitForSeconds(transitionStep);
        SetFrame(1, hurtOnTransitions); yield return new WaitForSeconds(transitionStep);
        SetFrame(0, true); yield return null;
    }

    IEnumerator GoDown()
    {
        SetFrame(1, hurtOnTransitions); yield return new WaitForSeconds(transitionStep);
        SetFrame(2, hurtOnTransitions); yield return new WaitForSeconds(transitionStep);
        SetFrame(3, false); yield return null;
    }

    void SetFrame(int index, bool isDangerous)
    {
        if (sr && sprites != null && index >= 0 && index < sprites.Length && sprites[index] != null)
            sr.sprite = sprites[index];

        dangerous = isDangerous;
        if (hitbox) hitbox.enabled = isDangerous;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!dangerous) return;

        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb != null && hb.health != null)
            hb.health.TakeDamage(damage, Vector2.zero, Vector2.up);
    }

}



