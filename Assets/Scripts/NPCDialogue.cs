using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCDialogue : MonoBehaviour
{
    // Estado global que otras salas consultan
    public static bool HasTalkedToEddie = false;
    public static event Action OnEddieTalked;

    [Header("UI")]
    public GameObject interactPrompt;
    public DialogueBox dialogueBox;

    [Header("Texto de este NPC")]
    [TextArea] public string[] dialogueLines;
    public KeyCode interactKey = KeyCode.E;

    [Header("Opciones")]
    [Tooltip("Lista de puertas que se abrirán al terminar el diálogo.")]
    public List<Door> doorsToOpen = new List<Door>();

    [Tooltip("Si es el Eddie final, cargará la escena indicada en 'finalSceneName'. Si está vacío, usa 'continuara'.")]
    public bool isFinalEddie = false;

    [Tooltip("Nombre de la escena a cargar (llenado automáticamente por el editor si usás el Scene Asset picker).")]
    public string finalSceneName = "continuara";

    // estado local
    bool playerInRange = false;
    bool dialogueRunning = false;

    // instancia-local para saber si este NPC ya habló
    public bool HasTalked { get; private set; } = false;

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

            if (dialogueBox != null)
            {
                dialogueBox.Open(dialogueLines, () =>
                {
                    OnDialogueComplete();
                });
            }
            else
            {
                OnDialogueComplete();
            }
        }
    }

    void OnDialogueComplete()
    {
        dialogueRunning = false;
        HasTalked = true;

        // Avisar global que Eddie habló
        if (!HasTalkedToEddie)
        {
            HasTalkedToEddie = true;
            OnEddieTalked?.Invoke();
        }

        // Abrir TODAS las puertas asignadas
        foreach (var d in doorsToOpen)
        {
            if (d != null)
                d.Open();
        }

        // Eddie final: cargar escena elegida (si corresponde)
        if (isFinalEddie)
        {
            string sceneToLoad = string.IsNullOrEmpty(finalSceneName) ? "continuara" : finalSceneName;

            // Asegurarse que la escena esté en Build Settings (o fallará)
            // Podés verificar en editor antes de build.
            try
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            catch (Exception ex)
            {
                Debug.LogError($"NPCDialogue: no se pudo cargar la escena '{sceneToLoad}'. ¿Está en Build Settings? Excepción: {ex.Message}");
            }
            return;
        }

        if (playerInRange && interactPrompt)
            interactPrompt.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            playerInRange = true;
            if (!dialogueRunning && interactPrompt)
                interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            playerInRange = false;
            if (interactPrompt)
                interactPrompt.SetActive(false);
        }
    }
}
