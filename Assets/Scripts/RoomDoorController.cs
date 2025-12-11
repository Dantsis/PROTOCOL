using System.Collections.Generic;
using UnityEngine;

public class RoomDoorController : MonoBehaviour
{
    [Header("Doors de la sala")]
    public List<Door> doors = new List<Door>();

    [Header("Opciones")]
    public bool requireNPC = true;        // necesita hablar con Eddie?
    public bool requireRoomClear = true;  // necesita completar contenido?

    bool npcSpoke = false;
    bool roomCleared = false;

    //---------------------------------------------
    // LLAMADO DESDE NPC (o desde un handler que conecte el NPC a esta habitación)
    //---------------------------------------------
    public void MarkNPCSpoke()
    {
        npcSpoke = true;
        TryOpenDoors();
    }

    //---------------------------------------------
    // LLAMADO DESDE RoomWaveSpawner (COMBATE)
    //---------------------------------------------
    public void MarkCombatCleared()
    {
        roomCleared = true;
        TryOpenDoors();
    }

    //---------------------------------------------
    // LLAMADO DESDE MusicalSequencePuzzle / LanternPuzzle
    //---------------------------------------------
    public void MarkPuzzleCleared()
    {
        roomCleared = true;
        TryOpenDoors();
    }

    //---------------------------------------------
    // INTENTO DE ABRIR PUERTAS
    //---------------------------------------------
    void TryOpenDoors()
    {
        if (requireNPC && !npcSpoke) return;
        if (requireRoomClear && !roomCleared) return;

        foreach (var d in doors)
        {
            if (d != null)
                d.Open();
        }
    }
}