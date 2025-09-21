using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueBox : MonoBehaviour
{
    [Header("Refs")]
    public TextMeshProUGUI dialogueText;

    [Header("Speed")]
    public float typeSpeed = 0.04f;

    // estado interno
    string[] lines;
    int index;
    Coroutine typing;
    System.Action onClosed;

    void Awake()
    {
        // Por si el objeto quedó activo en el editor: ocultarlo al iniciar la escena
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (dialogueText) dialogueText.text = "";
    }

    /// <summary>
    /// Abre la caja con estas líneas. Se puede pasar un callback opcional al cerrar.
    /// </summary>
    public void Open(string[] newLines, System.Action onClosedCallback = null)
    {
        lines = newLines;
        onClosed = onClosedCallback;
        index = 0;

        gameObject.SetActive(true);
        ShowLine();
    }

    void ShowLine()
    {
        if (typing != null) StopCoroutine(typing);
        typing = StartCoroutine(Type(lines[index]));
    }

    IEnumerator Type(string line)
    {
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Si todavía se está tipeando, completar
            if (typing != null && dialogueText.text != lines[index])
            {
                StopCoroutine(typing);
                dialogueText.text = lines[index];
            }
            else
            {
                // Siguiente línea o cerrar
                if (index < lines.Length - 1)
                {
                    index++;
                    ShowLine();
                }
                else
                {
                    Close();
                }
            }
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
        onClosed?.Invoke();
        onClosed = null;
    }
}
