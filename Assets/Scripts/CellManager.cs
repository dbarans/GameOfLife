using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellManager : MonoBehaviour
{
    private HashSet<Vector3Int> livingCells = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> nextGeneration = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> savedCells = new HashSet<Vector3Int>();
    private bool stateChanged = false;

    private static readonly object gridLock = new object();

    private void Start()
    {
        //CreateTutorialGeneration();
    }


    public bool HasStateChanged()
    {
        return stateChanged;
    }

    public void ResetStateChange()
    {
        stateChanged = false;
    }

    public void SetCellState(Vector3Int position, bool isAlive)
    {
        if (isAlive)
            livingCells.Add(position);
        else
            livingCells.Remove(position);

        stateChanged = true;
    }

    public bool IsCellAlive(Vector3Int position)
    {
        return livingCells.Contains(position);
    }

    public IReadOnlyCollection<Vector3Int> GetLivingCells()
    {
        return livingCells;
    }
    public void UpdateNextGeneration(IReadOnlyCollection<Vector3Int> newGeneration)
    {
        lock (gridLock)
        {
            nextGeneration.Clear();

            foreach (Vector3Int cell in newGeneration)
            {
                nextGeneration.Add(cell);
            }
            stateChanged = true;
        }
    }
    public void SwapGenerations()
    {
        lock (gridLock)
        {
            (livingCells, nextGeneration) = (nextGeneration, livingCells);
        }
    }
    public void CreateRandomGeneration()
    {
        int width = 100;
        int height = 100;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (UnityEngine.Random.value > 0.5f)
                {
                    livingCells.Add(new Vector3Int(x, y, 0));
                }
            }
        }
    }
    public void CreateTutorialGeneration()
    {
        int width = 1;
        int height = 1;
        for (int x = -2; x < width+1; x++)
        {
            for (int y = -2; y < height+1; y++)
            {
                if (UnityEngine.Random.value > 0.2f)
                {
                    livingCells.Add(new Vector3Int(x, y, 0));
                }
            }
        }
    }
    public void ClearGrid()
    {
        lock (gridLock)
        {
            livingCells.Clear();
            nextGeneration.Clear();
            stateChanged = true;
        }
    }
    public bool IsLivingCellsSetEmpty()
    {
        return livingCells.Count == 0;
    }    
    public void SaveGame()
    {
        lock (gridLock)
        {
            savedCells = new HashSet<Vector3Int>(livingCells);
        }
    }
    public void LoadGame()
    {
        lock (gridLock)
        {
            livingCells = new HashSet<Vector3Int>(savedCells);
            stateChanged = true;
        }
    }

    public void SetPattern(HashSet<Vector3Int> patternCells)
    {
        lock (gridLock)
        {
            livingCells.Clear();
            nextGeneration.Clear();
            
            foreach (Vector3Int cell in patternCells)
            {
                livingCells.Add(cell);
            }
            
            stateChanged = true;
        }
    }
}
