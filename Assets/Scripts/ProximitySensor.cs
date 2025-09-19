using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class ProximitySensor : MonoBehaviour
{
    [Tooltip("Referencia a la puerta (padre)")]
    public Door door;

    [Header("Detección de jugador")]
    public LayerMask playerLayers;               // capa del Player/Hurtbox
    [Tooltip("Caja extra donde NO se cierra, centrada en el umbral")]
    public Vector2 extraCheckSize = new Vector2(1.2f, 1.2f);

    bool playerInside = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!door) return;

        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            playerInside = true;
            door.Open();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!door) return;

        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            playerInside = false;
            StartCoroutine(TryCloseAfterDelay());
        }
    }

    IEnumerator TryCloseAfterDelay()
    {
        // espera el delay configurado en la puerta
        yield return new WaitForSeconds(door.CloseDelay);

        if (door.IsLocked) yield break;   // si la sala la bloqueó, no cerrar acá
        if (playerInside) yield break;    // si el jugador volvió a entrar, no cerrar

        // chequeo extra: si el jugador sigue pegado al umbral, no cerrar
        var hit = Physics2D.OverlapBox(transform.position, extraCheckSize, 0f, playerLayers);
        if (hit != null) yield break;

        door.Close();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0,1,1,0.2f);
        Gizmos.DrawCube(transform.position, new Vector3(extraCheckSize.x, extraCheckSize.y, 0));
        Gizmos.color = new Color(0,1,1,1);
        Gizmos.DrawWireCube(transform.position, new Vector3(extraCheckSize.x, extraCheckSize.y, 0));
    }
#endif
}
