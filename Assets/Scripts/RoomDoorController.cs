using System.Collections.Generic;
using UnityEngine;

public class RoomDoorController : MonoBehaviour
{
    [Header("Doors de la sala")]
    public List<Door> doors = new List<Door>();

    [Header("Opciones")]
    public bool requireNPC = true;          // ¿Hace falta hablar con Eddie?
    public bool requireRoomClear = true;    // ¿Hace falta limpiar la sala?
    public bool requireLevelCompleted = true; // ¿Hace falta que el nivel propio esté completo?

    // Estados locales de esta habitación
    bool npcSpoke = false;
    bool roomCleared = false;

    // NUEVO: este es el “nivel completado” local
    [HideInInspector]
    public bool levelCompleted = false;

    //---------------------------------------------
    // LLAMADO DESDE NPC
    //---------------------------------------------
    public void MarkNPCSpoke()
    {
        npcSpoke = true;
        TryOpenDoors();
    }

    //---------------------------------------------
    // LLAMADO DESDE RoomWaveSpawner (combate)
    //---------------------------------------------
    public void MarkCombatCleared()
    {
        roomCleared = true;
        TryOpenDoors();
    }

    //---------------------------------------------
    // LLAMADO DESDE puzzles (opcional)
    //---------------------------------------------
    public void MarkPuzzleCleared()
    {
        roomCleared = true;
        TryOpenDoors();
    }

    //---------------------------------------------
    // LLAMADO DESDE RoomWaveSpawner al terminar TODAS las olas
    // o desde un evento externo si el nivel se completa por otra razón
    //---------------------------------------------
    public void MarkLevelCompleted()
    {
        levelCompleted = true;
        TryOpenDoors();
    }

    //---------------------------------------------
    // INTENTO DE ABRIR PUERTAS
    //---------------------------------------------
    void TryOpenDoors()
    {
        if (requireNPC && !npcSpoke) return;
        if (requireRoomClear && !roomCleared) return;
        if (requireLevelCompleted && !levelCompleted) return;

        foreach (var d in doors)
        {
            if (d != null)
                d.Open();
        }
    }
}
