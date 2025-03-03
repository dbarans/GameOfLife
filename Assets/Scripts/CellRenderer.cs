using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;


public class CellRenderer : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tile cellTile;
    [SerializeField] private CellManager cellManager;
    [SerializeField] private Camera mainCamera;
    private Vector2 cellSize;
    private int tilesWide;
    private int tilesHigh;
    private int leftmostTile;
    private int rightmostTile;
    private int bottommostTile;
    private int topmostTile;
    private int previousleftmostTile;
    private int previousrightmostTile;
    private int previousbottommostTile;
    private int previoustopmostTile;
    private Vector3 lastCameraPosition;
    private bool needUpdate = false;


    private float lastOrthographicSize; 

    private void Start()
    {
        lastOrthographicSize = mainCamera.orthographicSize;
        UpdateTileDimensions();
        UpdateTilemapBounds();
    }

    void Update()
    {
        // Size of the camera has changed
        if (!Mathf.Approximately(lastOrthographicSize, mainCamera.orthographicSize))
        {
            lastOrthographicSize = mainCamera.orthographicSize;
            UpdateTileDimensions();
        }
        // Camera has moved
        Vector3 currentCameraPosition = mainCamera.transform.position;
        if (lastCameraPosition != currentCameraPosition)
        {
            lastCameraPosition = currentCameraPosition;
            UpdateTilemapBounds();
        }
        // State of the cells has changed
        if (cellManager.HasStateChanged())
        {
            needUpdate = true;
            cellManager.ResetStateChange();
        }
        if (needUpdate)
        {
            UpdateView();
            needUpdate = false;
        }
    }

    private void UpdateView()
    {
        tilemap.ClearAllTiles();
        IReadOnlyCollection<Vector3Int> livingCells = cellManager.GetLivingCells();
        int maxTilesOnScreen = tilesWide * tilesHigh;
        if (livingCells.Count() < maxTilesOnScreen)
        {
            foreach (Vector3Int pos in livingCells)
            {
                tilemap.SetTile(pos, cellTile);
            }
        }
        else
        {
            for (int x = leftmostTile; x <= rightmostTile; x++)
            {
                for (int y = bottommostTile; y <= topmostTile; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (livingCells.Contains(pos))
                    {
                        tilemap.SetTile(pos, cellTile);
                    }
                }
            }

        }
    }
    private void UpdateTilemapBounds()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 cellSize = tilemap.cellSize;

        float halfWidthInTiles = tilesWide / 2f;
        float halfHeightInTiles = tilesHigh / 2f;

        leftmostTile = Mathf.FloorToInt(cameraPosition.x / cellSize.x - halfWidthInTiles);
        rightmostTile = Mathf.FloorToInt(cameraPosition.x / cellSize.x + halfWidthInTiles);
        bottommostTile = Mathf.FloorToInt(cameraPosition.y / cellSize.y - halfHeightInTiles);
        topmostTile = Mathf.FloorToInt(cameraPosition.y / cellSize.y + halfHeightInTiles);

        if (leftmostTile != previousleftmostTile || rightmostTile != previousrightmostTile || bottommostTile != previousbottommostTile || topmostTile != previoustopmostTile)
        {
            previousleftmostTile = leftmostTile;
            previousrightmostTile = rightmostTile;
            previousbottommostTile = bottommostTile;
            previoustopmostTile = topmostTile;
            needUpdate = true;
        }
    }

    private void UpdateTileDimensions()
    {
        int previousTilesWide = tilesWide;
        int previousTilesHigh = tilesHigh;
        float worldHeight = mainCamera.orthographicSize * 2;
        float worldWidth = worldHeight * mainCamera.aspect;

        cellSize = tilemap.cellSize;

        float rawTilesWide = worldWidth / cellSize.x;
        float rawTilesHigh = worldHeight / cellSize.y;

        tilesWide = Mathf.CeilToInt(rawTilesWide);
        tilesHigh = Mathf.CeilToInt(rawTilesHigh);

        if (rawTilesWide > tilesWide - 0.5f)
        {
            tilesWide++;
        }
        if (rawTilesHigh > tilesHigh - 0.5f)
        {
            tilesHigh++;
        }
        tilesHigh++; 
        tilesWide++;
        if (tilesWide != previousTilesWide || tilesHigh != previousTilesHigh)
        {
            UpdateTilemapBounds();
        }
    }
   


}
