using UnityEngine;

public class DoorDetectorComponent : MonoBehaviour
{
    [Tooltip("Referencia a la puerta que debe cerrarse cuando el jugador pasa.")]
    public Door door;

    private void Awake()
    {
        // Si no se asigna en el inspector, intenta buscar una puerta en los padres
        if (door == null)
            door = GetComponentInParent<Door>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (door == null) return;

        // Asegurarse de que detecta al jugador correctamente
        if (other.CompareTag("Player"))
        {
            door.Close();   // cerrar la puerta
        }
    }
}
