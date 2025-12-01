using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneSpawnFlash : MonoBehaviour
{
    [Header("Parpadeo de aparición")]
    public int blinkCount = 4;
    public float blinkInterval = 0.08f;

    private SpriteRenderer[] spriteRenderers;
    private List<Color> originalColors = new List<Color>();

    void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        originalColors.Clear();
        foreach (var sr in spriteRenderers)
        {
            originalColors.Add(sr.color);
        }
    }

    /// <summary>
    /// Llamado por el spawner. Hace parpadear al dron antes de activarse.
    /// </summary>
    public IEnumerator PlaySpawnFlashRoutine()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            yield break;

        for (int i = 0; i < blinkCount; i++)
        {
            // "Blanco fantasma": mantiene blanco pero baja mucho la alpha
            for (int j = 0; j < spriteRenderers.Length; j++)
            {
                Color c = originalColors[j];
                // casi invisible pero con un leve glow blanco
                spriteRenderers[j].color = new Color(1f, 1f, 1f, 0.05f);
            }

            yield return new WaitForSeconds(blinkInterval);

            // Volver a color original
            for (int j = 0; j < spriteRenderers.Length; j++)
            {
                spriteRenderers[j].color = originalColors[j];
            }

            yield return new WaitForSeconds(blinkInterval);
        }

        // Aseguramos que termine en el color original
        for (int j = 0; j < spriteRenderers.Length; j++)
        {
            spriteRenderers[j].color = originalColors[j];
        }
    }
}

