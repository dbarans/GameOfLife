using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GenerationManager
{
    private readonly CellManager cellManager;
    private readonly object nextGenLock;
    private readonly Action onGenerationCompleted;
    private bool _allowBirth = true;

    public bool AllowBirth
    {
        get => _allowBirth;
        set => _allowBirth = value;
    }

    public GenerationManager(CellManager cellManager, Action onGenerationCompleted, object nextGenLock)
    {
        this.cellManager = cellManager;
        this.onGenerationCompleted = onGenerationCompleted;
        this.nextGenLock = nextGenLock;
    }

    public void GenerateNextGeneration()
    {
        lock (nextGenLock)
        {
            HashSet<Vector3Int> livingCellsCopy = cellManager.GetLivingCells();
            HashSet<Vector3Int> newGeneration = CalculateNextGeneration(livingCellsCopy);
            cellManager.UpdateNextGeneration(newGeneration);
            onGenerationCompleted?.Invoke();
        }
    }

    public HashSet<Vector3Int> CalculateNextGeneration(HashSet<Vector3Int> currentGen)
    {
        HashSet<Vector3Int> newGeneration = new HashSet<Vector3Int>();
        HashSet<Vector3Int> cellsToCheck = new HashSet<Vector3Int>(currentGen);

        foreach (Vector3Int cell in currentGen)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector3Int neighbor = new Vector3Int(cell.x + x, cell.y + y, cell.z);
                    cellsToCheck.Add(neighbor);
                }
            }
        }

        foreach (Vector3Int cell in cellsToCheck)
        {
            int neighbors = CheckNeighbors(cell, currentGen);
            bool isCellAlive = currentGen.Contains(cell);
            
            if (isCellAlive)
            {
                if (neighbors == 2 || neighbors == 3)
                {
                    newGeneration.Add(cell);
                }
            }
            else
            {
                if (neighbors == 3 && _allowBirth)
                {
                    newGeneration.Add(cell);
                }
            }
        }

        return newGeneration;
    }

    private int CheckNeighbors(Vector3Int cell, HashSet<Vector3Int> currentGen)
    {
        int neighborsCount = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                Vector3Int neighbor = new Vector3Int(cell.x + x, cell.y + y, cell.z);
                if (currentGen.Contains(neighbor))
                {
                    neighborsCount++;
                }
            }
        }
        return neighborsCount;
    }
}
