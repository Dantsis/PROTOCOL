using UnityEngine;

[RequireComponent(typeof(DroneMechAI))]
public class MovementLimiter2D : MonoBehaviour
{
    [Header("Límite de movimiento")]
    public bool limitMovement = true;

    [Tooltip("Collider2D que define el área donde el drone puede moverse")]
    public Collider2D allowedArea;

    DroneMechAI ai;
    Rigidbody2D rb;

    void Awake()
    {
        ai = GetComponent<DroneMechAI>();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!limitMovement || allowedArea == null) return;

        // si el drone está fuera del área, lo empujamos adentro
        if (!IsInside(allowedArea, transform.position))
        {
            // lo hacemos frenar
            ai.SetDesiredVelocity(Vector2.zero);

            // opcional: lo movemos adentro del collider
            Vector2 closest = allowedArea.ClosestPoint(transform.position);
            rb.MovePosition(closest);
        }
    }

    bool IsInside(Collider2D col, Vector2 point)
    {
        // Para triggers
        if (col.isTrigger)
            return col.OverlapPoint(point);

        // Para colliders sólidos
        return col.bounds.Contains(point);
    }
}
