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
            IReadOnlyCollection<Vector3Int> livingCells = cellManager.GetLivingCells();
            HashSet<Vector3Int> newGeneration = CalculateNextGeneration(livingCells);
            cellManager.UpdateNextGeneration(newGeneration);
            onGenerationCompleted?.Invoke();
        }
    }

    public HashSet<Vector3Int> CalculateNextGeneration(IReadOnlyCollection<Vector3Int> currentGen)
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
            int neighbors = CheckNeighbors(cell);
            bool isCellAlive = cellManager.IsCellAlive(cell);
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

    private int CheckNeighbors(Vector3Int cell)
    {
        int neighborsCount = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                Vector3Int neighbor = new Vector3Int(cell.x + x, cell.y + y, cell.z);
                if (cellManager.IsCellAlive(neighbor))
                {
                    neighborsCount++;
                }
            }
        }
        return neighborsCount;
    }
}
