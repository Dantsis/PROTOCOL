using UnityEngine;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactPrompt;  // TMP "Presione E..." (en el Canvas)
    public DialogueBox dialogueBox;    // referencia al DialoguePanel (con el script arriba)

    [Header("Texto de este NPC")]
    [TextArea] public string[] dialogueLines;
    public KeyCode interactKey = KeyCode.E;

    // estado
    bool playerInRange = false;
    bool dialogueRunning = false;

    void Start()
    {
        if (interactPrompt) interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange || dialogueRunning) return;

        if (Input.GetKeyDown(interactKey))
        {
            dialogueRunning = true;
            if (interactPrompt) interactPrompt.SetActive(false);

            if (dialogueBox)
            {
                // Abre y cuando termina vuelve a habilitar interacción
                dialogueBox.Open(dialogueLines, () =>
                {
                    dialogueRunning = false;
                    if (playerInRange && interactPrompt) interactPrompt.SetActive(true);
                });
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            playerInRange = true;
            if (!dialogueRunning && interactPrompt) interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            playerInRange = false;
            if (interactPrompt) interactPrompt.SetActive(false);
        }
    }
}

