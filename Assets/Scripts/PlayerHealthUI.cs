using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Health playerHealth;   // referencia al Health del jugador
    public Slider slider;         // referencia al Slider de la UI

    void Start()
    {
        if (playerHealth == null)
        {
            // Busca automáticamente un Health marcado como player
            var allHealth = FindObjectsOfType<Health>();
            foreach (var h in allHealth)
            {
                if (h.isPlayer)
                {
                    playerHealth = h;
                    break;
                }
            }
        }

        if (slider == null)
            slider = GetComponent<Slider>();

        if (playerHealth != null && slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = playerHealth.maxHealth;
            slider.value = playerHealth.currentHealth;
        }
    }

    void Update()
    {
        if (playerHealth != null && slider != null)
        {
            slider.value = playerHealth.currentHealth;
        }
    }
}
