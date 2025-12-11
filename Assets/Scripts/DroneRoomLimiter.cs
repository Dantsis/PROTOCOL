using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DroneRoomLimiter : MonoBehaviour
{
    [Header("Asigná aquí el BoxCollider2D de la habitación")]
    public BoxCollider2D roomBounds;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (!roomBounds)
            Debug.LogError("DroneRoomLimiter: No asignaste un roomBounds (BoxCollider2D de la habitación).");
    }

    void FixedUpdate()
    {
        if (!roomBounds) return;

        Vector2 pos = rb.position;
        Bounds b = roomBounds.bounds;

        // Obtener velocidad actual
        Vector2 vel = rb.linearVelocity;

        // Bloquear movimiento si toca los límites
        if (pos.x <= b.min.x && vel.x < 0) vel.x = 0;
        if (pos.x >= b.max.x && vel.x > 0) vel.x = 0;
        if (pos.y <= b.min.y && vel.y < 0) vel.y = 0;
        if (pos.y >= b.max.y && vel.y > 0) vel.y = 0;

        rb.linearVelocity = vel;

        // Clampear la posición dentro de la habitación
        pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
        pos.y = Mathf.Clamp(pos.y, b.min.y, b.max.y);

        rb.position = pos;
    }

    void OnDrawGizmosSelected()
    {
        if (!roomBounds) return;

        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Gizmos.DrawCube(roomBounds.bounds.center, roomBounds.bounds.size);
    }
}
