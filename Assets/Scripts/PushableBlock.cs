using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PushableBlock : MonoBehaviour
{
    [Header("Movimiento en grilla")]
    public float tileSize = 1f;
    public float moveTime = 0.15f;

    [Header("Colisión")]
    [Tooltip("Capas que bloquean a la caja (paredes, otras cajas, props sólidos...)")]
    public LayerMask blockingLayers;

    bool isMoving = false;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isMoving) return;

        // Solo reaccionamos si la otra cosa es el Player
        if (!collision.gameObject.CompareTag("Player")) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(x) < 0.1f && Mathf.Abs(y) < 0.1f)
            return;

        // Elegimos un eje dominante: o horizontal o vertical (no diagonal)
        Vector2 dir;
        if (Mathf.Abs(x) > Mathf.Abs(y))
            dir = new Vector2(Mathf.Sign(x), 0f);
        else
            dir = new Vector2(0f, Mathf.Sign(y));

        Vector2 targetPos = rb.position + dir * tileSize;

        // ¿Hay algo bloqueando en esa casilla?
        Collider2D hit = Physics2D.OverlapBox(targetPos, GetBoxSize(), 0f, blockingLayers);
        if (hit == null)
        {
            StartCoroutine(MoveTo(targetPos));
        }
    }

    Vector2 GetBoxSize()
    {
        var col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
            return box.size * 0.9f; // un poquito más pequeño para tolerancia
        if (col is CircleCollider2D circle)
            return Vector2.one * circle.radius * 2f * 0.9f;
        return Vector2.one * 0.9f;
    }

    IEnumerator MoveTo(Vector2 targetPos)
    {
        isMoving = true;

        Vector2 start = rb.position;
        float t = 0f;

        while (t < moveTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / moveTime);
            rb.MovePosition(Vector2.Lerp(start, targetPos, k));
            yield return null;
        }

        rb.MovePosition(targetPos);
        isMoving = false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, GetBoxSize());
    }
#endif
}
