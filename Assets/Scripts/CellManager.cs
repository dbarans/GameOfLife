using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour
{
    private Dictionary<Vector3Int, bool> livingCells = new Dictionary<Vector3Int, bool>();
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
            livingCells[position] = true;
        else
            livingCells.Remove(position);

        stateChanged = true;
    }

    public bool IsCellAlive(Vector3Int position)
    {
        return livingCells.ContainsKey(position);
    }

    public List<Vector3Int> GetLivingCells()
    {
        return new List<Vector3Int>(livingCells.Keys);
    }
}
