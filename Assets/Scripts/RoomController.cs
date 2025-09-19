using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class RoomController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Puertas de esta sala que se controlan en conjunto")]
    public List<Door> doors = new List<Door>();

    [Tooltip("Enemigos con Health dentro de la sala (asignar manualmente o auto-descubrir)")]
    public List<Health> enemies = new List<Health>();

    [Header("Opcional: auto-descubrir enemigos por capa")]
    public bool autoFindEnemiesOnStart = false;
    public LayerMask enemyLayers;
    public Vector2 boundsPadding = new Vector2(0.1f, 0.1f); // margen para el overlap

    // Estado
    bool encounterStarted = false;
    bool roomCleared = false;

    Collider2D roomTrigger; // IsTrigger = true

    void Reset()
    {
        roomTrigger = GetComponent<Collider2D>();
        if (roomTrigger) roomTrigger.isTrigger = true;
    }

    void Awake()
    {
        roomTrigger = GetComponent<Collider2D>();
        if (roomTrigger && !roomTrigger.isTrigger) roomTrigger.isTrigger = true;
    }

    void Start()
    {
        if (autoFindEnemiesOnStart)
            AutoCollectEnemiesInBounds();
    }

    void AutoCollectEnemiesInBounds()
    {
        // Intentamos usar bounds del collider
        var b = roomTrigger.bounds;
        b.Expand(new Vector3(boundsPadding.x, boundsPadding.y, 0f));

        // Overlap en área (simplificado con Physics2D.OverlapBoxAll)
        var size = new Vector2(b.size.x, b.size.y);
        var center = (Vector2)b.center;
        var hits = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayers);

        enemies.Clear();
        foreach (var h in hits)
        {
            var hp = h.GetComponent<Health>() ?? h.GetComponentInParent<Health>();
            if (hp != null && !hp.isPlayer && !enemies.Contains(hp))
                enemies.Add(hp);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (roomCleared || encounterStarted) return;

        // ¿Entró el jugador?
        var hb = other.GetComponent<Hurtbox>() ?? other.GetComponentInParent<Hurtbox>();
        if (hb && hb.health && hb.health.isPlayer)
        {
            StartEncounter();
        }
    }

    void StartEncounter()
    {
        encounterStarted = true;

        // Cerrar y bloquear todas las puertas
        foreach (var d in doors)
        {
            if (!d) continue;
            d.Lock();  // cierra + bloquea
        }

        // Comenzar a monitorear enemigos
        InvokeRepeating(nameof(CheckEnemiesCleared), 0.2f, 0.2f);
    }

    void CheckEnemiesCleared()
    {
        // Limpiar referencias nulas por si algo fue destruido
        enemies.RemoveAll(e => e == null);

        bool anyAlive = false;
        foreach (var e in enemies)
        {
            if (e != null && e.IsAlive)
            {
                anyAlive = true;
                break;
            }
        }

        if (!anyAlive)
        {
            RoomCleared();
        }
    }

    void RoomCleared()
    {
        CancelInvoke(nameof(CheckEnemiesCleared));
        roomCleared = true;
        encounterStarted = false;

        // Desbloquear y abrir puertas
        foreach (var d in doors)
        {
            if (!d) continue;
            d.Unlock();
            d.Open();
        }
    }

#if UNITY_EDITOR
    // Gizmo del área de detección de enemigos auto
    void OnDrawGizmosSelected()
    {
        if (!roomTrigger) roomTrigger = GetComponent<Collider2D>();
        if (!roomTrigger) return;

        var b = roomTrigger.bounds;
        b.Expand(new Vector3(boundsPadding.x, boundsPadding.y, 0f));

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawCube(b.center, b.size);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireCube(b.center, b.size);
    }
#endif
}
