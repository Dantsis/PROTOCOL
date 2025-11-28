using System.Collections.Generic;
using UnityEngine;

public class PressurePlatePuzzle : MonoBehaviour
{
    [Header("Placas de presión de esta sala")]
    public List<PressurePlate> plates = new List<PressurePlate>();

    [Header("Puertas controladas")]
    public List<Door> doors = new List<Door>();

    [Header("Opcional: mantener puertas cerradas mientras no estén todas presionadas")]
    public bool closeWhenNotSolved = true;

    bool solved = false;

    void Awake()
    {
        // Enlazamos cada placa con este puzzle
        foreach (var p in plates)
        {
            if (p != null)
                p.puzzle = this;
        }
    }

    public void OnPlateStateChanged()
    {
        if (solved) return;

        // ¿Están TODAS presionadas?
        bool allPressed = true;
        foreach (var p in plates)
        {
            if (p == null) continue;
            if (!p.IsPressed)
            {
                allPressed = false;
                break;
            }
        }

        if (allPressed)
        {
            SolvePuzzle();
        }
        else if (closeWhenNotSolved)
        {
            CloseDoors();
        }
    }

    void SolvePuzzle()
    {
        solved = true;
        OpenDoors();
    }

    void OpenDoors()
    {
        foreach (var d in doors)
        {
            if (d == null) continue;
            d.Unlock();
            d.Open();
        }
    }

    void CloseDoors()
    {
        foreach (var d in doors)
        {
            if (d == null) continue;
            d.Lock();
        }
    }
}
