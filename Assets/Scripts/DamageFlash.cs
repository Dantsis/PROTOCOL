using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    public float flashDuration = 0.1f;  // tiempo de cada flash
    public int flashCount = 3;          // cantidad de parpadeos

    private SpriteRenderer sr;
    private Color originalColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    public void PlayDamageFlash()
    {
        StopAllCoroutines();               // evita flashes solapados
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            sr.color = new Color(1f, 0.3f, 0.3f, 0.6f); // rojizo transparente
            yield return new WaitForSeconds(flashDuration);

            sr.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
    }
}
