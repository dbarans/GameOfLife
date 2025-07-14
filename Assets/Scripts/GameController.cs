using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class GameController : MonoBehaviour
{
    [SerializeField] private float genPerSec = 5f;
    private bool _isRunning = false;
    private float timeSinceLastGen = 0;
    private int isNextGenCalculated = 0;
    [SerializeField] private CellGrid CellGrid;
    [SerializeField] private Camera mainCamera;
    private Color cameraBackgroundColor;

    private static readonly object nextGenLock = new object();

    public bool isRunning
    {
        get { return _isRunning; }
    }

    private void Start()
    {
        cameraBackgroundColor = mainCamera.backgroundColor;
    }

    private void Update()
    {
        if (_isRunning)
        {
            UpdateGeneration();
        }
    }
    private void GenerateNextGeneration() 
    {
        lock (nextGenLock)
        {
            IReadOnlyCollection<Vector3Int> livingCells = CellGrid.GetLivingCells();
            HashSet<Vector3Int> newGeneration = CalculateNextGeneration(livingCells);
            CellGrid.UpdateNextGeneration(newGeneration);
            Interlocked.Exchange(ref isNextGenCalculated, 1);
        }
    }
    private void UpdateGeneration()
    {
        timeSinceLastGen += Time.deltaTime;
        if (timeSinceLastGen >= 1f / genPerSec && isNextGenCalculated == 1)
        {
            timeSinceLastGen = 0f;
            Interlocked.Exchange(ref isNextGenCalculated, 1);
            CellGrid.SwapGenerations();
            Task.Run(() => GenerateNextGeneration()); 
        }
    }
    private HashSet<Vector3Int> CalculateNextGeneration(IReadOnlyCollection<Vector3Int> genIn)
    {
        HashSet<Vector3Int> genOut = new HashSet<Vector3Int>();
        HashSet<Vector3Int> cellsToCheck = new HashSet<Vector3Int>(genIn);
        foreach (Vector3Int cell in genIn)
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
            bool isCellAlive = CellGrid.IsCellAlive(cell);
            if (isCellAlive)
            {
                if (neighbors == 2 || neighbors == 3)
                {
                    genOut.Add(cell);
                }
            }
            else
            {
                if (neighbors == 3)
                {
                    genOut.Add(cell);
                }
            }
        }

        return genOut;
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
                bool isNeighbourAlive = CellGrid.IsCellAlive(neighbor);
                if (isNeighbourAlive) 
                {
                    neighborsCount++;
                }
            }
        }


            return neighborsCount;
    }

   public void RunGame()
    {
        if (CellGrid.IsLivingCellsSetEmpty()) return;

        GenerateNextGeneration();
        mainCamera.backgroundColor = Color.black;
        _isRunning = true;

    }
    public void PauseGame()
    {
        mainCamera.backgroundColor = cameraBackgroundColor;
        _isRunning = false;
    }
    public void ResetGame()
    {
        mainCamera.backgroundColor = cameraBackgroundColor;
        _isRunning = false;
        CellGrid.ClearGrid();
        Interlocked.Exchange(ref isNextGenCalculated, 0);
    }
    

 
}
