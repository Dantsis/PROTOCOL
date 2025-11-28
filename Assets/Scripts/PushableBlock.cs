using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PushableBlock : MonoBehaviour
{
    [Header("Agarre")]
    [Tooltip("Offset relativo al jugador cuando está agarrada la caja.")]
    public Vector2 holdOffset = new Vector2(0f, -0.25f);

    [Header("UI (opcional)")]
    [Tooltip("Canvas / icono 'Presione E' asociado a esta caja.")]
    public GameObject interactPrompt;

    // --- Estado interno ---
    Transform player;      // referencia al jugador cerca
    bool playerInRange;    // está dentro del trigger
    bool isGrabbed;        // actualmente agarrada

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Queremos un rigidbody "quieto" que no se mueva solo
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;   // no hay fuerzas, ni deslizamiento
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void Update()
    {
        // Agarrar / soltar con E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleGrab();
        }

        // Si está agarrada, pegada al jugador con offset
        if (isGrabbed && player != null)
        {
            Vector3 target = player.position + (Vector3)holdOffset;
            transform.position = target;
        }
    }

    void ToggleGrab()
    {
        if (player == null) return;

        isGrabbed = !isGrabbed;

        if (isGrabbed)
        {
            // La caja pasa a ser hija del jugador (más prolijo)
            transform.SetParent(player);
            transform.localPosition = holdOffset;

            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }
        else
        {
            // La caja se queda donde está y deja de seguir
            transform.SetParent(null);

            if (playerInRange && interactPrompt != null)
                interactPrompt.SetActive(true);
        }
    }

    // Zona de interacción: usá un collider grande con IsTrigger = true
    void OnTriggerEnter2D(Collider2D other)
    {
        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            player = hb.transform;
            playerInRange = true;

            if (!isGrabbed && interactPrompt != null)
                interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            playerInRange = false;

            if (!isGrabbed && interactPrompt != null)
                interactPrompt.SetActive(false);

            // Si se aleja mientras la caja está agarrada, la soltamos
            if (isGrabbed)
            {
                isGrabbed = false;
                transform.SetParent(null);
            }
        }
    }
}
