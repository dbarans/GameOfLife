using UnityEngine;
using UnityEngine.Tilemaps;

public class CellClickHandler : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile cellTile;

    private Camera mainCamera;
    private TileBase firstClickedTile;

    void Start()
    {
        mainCamera = Camera.main;

        if (tilemap == null)
        {
            Debug.LogError("Tilemap not assigned to CellClickHandler!");
        }
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(touch.position);
                worldPosition.z = 0;
                Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

                firstClickedTile = tilemap.GetTile(cellPosition);
                HandleTouch(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                HandleTouch(touch.position);
            }
        }
    }

    void HandleTouch(Vector3 touchPosition)
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(touchPosition);
        worldPosition.z = 0;
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        TileBase clickedTile = tilemap.GetTile(cellPosition);

        if (firstClickedTile == clickedTile)
        {
            tilemap.SetTile(cellPosition, clickedTile == null ? cellTile : null);
        }
    }
}