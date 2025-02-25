using UnityEngine;
using UnityEngine.Tilemaps;

public class CellClickHandler : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private CellManager cellManager;

    private Camera mainCamera;
    private bool firstClickState;

    void Start()
    {
        mainCamera = Camera.main;

        if (tilemap == null)
            Debug.LogError("Tilemap not assigned to CellClickHandler!");

        if (cellManager == null)
            Debug.LogError("CellManager not assigned to CellClickHandler!");
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                Vector3Int cellPosition = GetCellFromTouch(touch.position);

                if (touch.phase == TouchPhase.Began)
                    firstClickState = cellManager.IsCellAlive(cellPosition);

                HandleTouch(cellPosition);
            }
        }
    }

    void HandleTouch(Vector3Int cellPosition)
    {
        bool isAlive = cellManager.IsCellAlive(cellPosition);
        if (firstClickState == isAlive)
        {
            cellManager.SetCellState(cellPosition, !isAlive);
        }
    }

    Vector3Int GetCellFromTouch(Vector3 touchPosition)
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(touchPosition);
        worldPosition.z = 0;
        return tilemap.WorldToCell(worldPosition);
    }
}
