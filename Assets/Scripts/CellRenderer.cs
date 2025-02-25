using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellRenderer : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile cellTile;
    public CellManager cellManager;

    void Update()
    {
        if (cellManager.HasStateChanged())
        {
            UpdateView();
            cellManager.ResetStateChange();
        }
    }

    void UpdateView()
    {
        tilemap.ClearAllTiles();
        foreach (Vector3Int pos in cellManager.GetLivingCells())
        {
            tilemap.SetTile(pos, cellTile);
        }
    }
}
