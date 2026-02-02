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

    private const int BUFFER_SIZE = 128;
    private List<HashSet<Vector3Int>> generationBuffer = new List<HashSet<Vector3Int>>();
    private int displayedGenerationIndex = 0;
    private int calculatedGenerationIndex = 0;
    private int latestReadyGenerationIndex = -1;

    private int displayedGenerationNumber = 0;
    private int calculatedGenerationNumber = 0;
    private readonly int[] bufferGenerationNumbers = new int[BUFFER_SIZE];

    // Maximum number of generations that can be calculated ahead of what is currently displayed.
    private const int MAX_GENERATIONS_AHEAD = 100;

    private static readonly object gridLock = new object();

    private void Start()
    {
        //CreateTutorialGeneration();
        InitializeGenerationBuffer();
    }

    private void InitializeGenerationBuffer()
    {
        generationBuffer.Clear();
        for (int i = 0; i < BUFFER_SIZE; i++)
        {
            generationBuffer.Add(new HashSet<Vector3Int>());
            bufferGenerationNumbers[i] = -1;
        }
        displayedGenerationIndex = 0;
        calculatedGenerationIndex = 0;
        latestReadyGenerationIndex = -1;
        displayedGenerationNumber = 0;
        calculatedGenerationNumber = 0;
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
        lock (gridLock)
        {
            return livingCells.Contains(position);
        }
    }

    public HashSet<Vector3Int> GetLivingCells()
    {
        lock (gridLock)
        {
            // Return a copy to avoid race conditions
            return new HashSet<Vector3Int>(livingCells);
        }
    }

    // === Generation buffer methods ===


    public bool TryGetNextGenerationToCalculate(out int bufferIndex, out int generationNumber)
    {
        lock (gridLock)
        {
            // Do not allow calculations to go too far ahead of what is currently displayed.
            if (calculatedGenerationNumber - displayedGenerationNumber >= MAX_GENERATIONS_AHEAD)
            {
                bufferIndex = -1;
                generationNumber = -1;
                return false;
            }

            int nextIndex = (calculatedGenerationIndex + 1) % BUFFER_SIZE;

            bufferIndex = nextIndex;
            generationNumber = calculatedGenerationNumber + 1;
            return true;
        }
    }

    /// <summary>
    /// Saves a calculated generation to the buffer
    /// </summary>
    public bool SaveCalculatedGeneration(int bufferIndex, int generationNumber, HashSet<Vector3Int> newGeneration)
    {
        lock (gridLock)
        {
            if (bufferIndex < 0 || bufferIndex >= BUFFER_SIZE)
                return false;

            generationBuffer[bufferIndex].Clear();
            foreach (Vector3Int cell in newGeneration)
            {
                generationBuffer[bufferIndex].Add(cell);
            }

            bufferGenerationNumbers[bufferIndex] = generationNumber;

            // Update the latest ready generation (only if it is not displayed)
            if (bufferIndex != displayedGenerationIndex)
            {
                if (latestReadyGenerationIndex == -1)
                {
                    latestReadyGenerationIndex = bufferIndex;
                }
                else
                {
                    // Set the newest ready generation (handle wrap-around in circular buffer)
                    int distanceToDisplayed =
                        (bufferIndex - displayedGenerationIndex + BUFFER_SIZE) % BUFFER_SIZE;

                    int distanceLatestToDisplayed =
                        (latestReadyGenerationIndex - displayedGenerationIndex + BUFFER_SIZE) % BUFFER_SIZE;

                    if (distanceToDisplayed > distanceLatestToDisplayed)
                    {
                        latestReadyGenerationIndex = bufferIndex;
                    }
                }
            }

            calculatedGenerationIndex = bufferIndex;
            calculatedGenerationNumber = generationNumber;
            return true;
        }
    }


    public bool TryDisplayGeneration(int desiredGenerationNumber)
    {
        lock (gridLock)
        {
            if (desiredGenerationNumber <= displayedGenerationNumber)
                return false;

            int foundIndex = -1;
            for (int i = 0; i < BUFFER_SIZE; i++)
            {
                if (bufferGenerationNumbers[i] == desiredGenerationNumber)
                {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex == -1)
                return false; 

            livingCells.Clear();
            foreach (Vector3Int cell in generationBuffer[foundIndex])
            {
                livingCells.Add(cell);
            }

            displayedGenerationIndex = foundIndex;
            displayedGenerationNumber = desiredGenerationNumber;

            if (latestReadyGenerationIndex == foundIndex) latestReadyGenerationIndex = -1;

            stateChanged = true;
            return true;
        }
    }

    /// <summary>
    /// Returns the index of the latest ready generation to display,
    /// or -1 if none exists
    /// </summary>
    public int GetLatestReadyGenerationIndex()
    {
        lock (gridLock)
        {
            return latestReadyGenerationIndex;
        }
    }

    /// <summary>
    /// Switches the displayed generation to the latest ready one (skip-to-latest)
    /// Returns true if the switch was successful
    /// </summary>
    public bool SwitchToLatestGeneration()
    {
        lock (gridLock)
        {
            if (latestReadyGenerationIndex == -1 ||
                latestReadyGenerationIndex == displayedGenerationIndex)
                return false;

            // Copy the latest generation into livingCells
            livingCells.Clear();
            foreach (Vector3Int cell in generationBuffer[latestReadyGenerationIndex])
            {
                livingCells.Add(cell);
            }

            displayedGenerationIndex = latestReadyGenerationIndex;
            latestReadyGenerationIndex = -1; // Reset; will be set on next save
            stateChanged = true;
            return true;
        }
    }

    /// <summary>
    /// Returns the base generation used to calculate the next generation
    /// Returns a COPY to avoid race conditions
    /// </summary>
    public HashSet<Vector3Int> GetBaseGenerationForCalculation(int targetIndex)
    {
        lock (gridLock)
        {
            // We calculate targetIndex, so the base is targetIndex - 1
            int baseIndex = (targetIndex - 1 + BUFFER_SIZE) % BUFFER_SIZE;

            if (baseIndex == displayedGenerationIndex)
            {
                // Base is the displayed generation (livingCells) – return a copy
                return new HashSet<Vector3Int>(livingCells);
            }

            // Check if the buffer generation exists and is not empty
            if (generationBuffer[baseIndex].Count > 0)
            {
                // Return a copy from the buffer
                return new HashSet<Vector3Int>(generationBuffer[baseIndex]);
            }

            // Fallback – if the buffer is empty (e.g. at the beginning),
            // use livingCells and return a copy
            // This can only happen when targetIndex == 1 and Gen0 is in livingCells
            return new HashSet<Vector3Int>(livingCells);
        }
    }

    /// <summary>
    /// Resets the buffer when starting a new game
    /// </summary>
    public void ResetGenerationBuffer()
    {
        lock (gridLock)
        {
            // Clear all buffer slots
            for (int i = 0; i < BUFFER_SIZE; i++)
            {
                generationBuffer[i].Clear();
                bufferGenerationNumbers[i] = -1;
            }

            // Copy livingCells into the first buffer slot (Gen0)
            foreach (Vector3Int cell in livingCells)
            {
                generationBuffer[0].Add(cell);
            }

            displayedGenerationIndex = 0;   // Display Gen0 (livingCells)
            calculatedGenerationIndex = 0;  // Last calculated is Gen0, next will be Gen1
            latestReadyGenerationIndex = -1; // No ready generations

            displayedGenerationNumber = 0;
            calculatedGenerationNumber = 0;
            bufferGenerationNumbers[0] = 0;
        }
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
        for (int x = -2; x < width + 1; x++)
        {
            for (int y = -2; y < height + 1; y++)
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
            InitializeGenerationBuffer();
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
            ResetGenerationBuffer();
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

            ResetGenerationBuffer();
            stateChanged = true;
        }
    }
}
