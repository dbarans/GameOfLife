using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour
{
    private HashSet<Vector3Int> livingCells = new HashSet<Vector3Int>();
    private bool stateChanged = false;

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
}
