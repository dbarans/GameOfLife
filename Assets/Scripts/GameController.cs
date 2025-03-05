using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class GameController : MonoBehaviour
{
    private float genPerSec = 5f;
    private float timeSinceLastGen = 0;
    private int isNextGenCalculated = 0;
    [SerializeField] private CellGrid CellGrid;

    private static readonly object nextGenLock = new object();

    private void Start()
    {
        Task.Run(() => GenerateNextGeneration());
    }
    private void Update()
    {
        UpdateGeneration();
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
        //logic to calculate new generation
        return genOut;
    }
}
