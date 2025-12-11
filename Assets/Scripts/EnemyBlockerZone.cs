using UnityEngine;

public class EnemyBlockerZone : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var ai = other.GetComponent<DroneMechAI>();
            if (ai != null)
            {
                ai.SetDesiredVelocity(Vector2.zero);

                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
