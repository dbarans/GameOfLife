using UnityEngine;
using UnityEngine.Tilemaps;

public class CellClickHandler : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private CellManager cellManager;

    private Camera mainCamera;
    private bool firstClickState;
    private Vector2 prevTouchDelta; 

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
        if (Input.touchCount == 1)
        {
            HandleSingleTouch();
        }
        else if (Input.touchCount == 2)
        {
            HandleTwoTouches();
        }
        else
        {
            prevTouchDelta = Vector2.zero;
        }
    }

    void HandleSingleTouch()
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
    void HandleTwoTouches()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
        {
            prevTouchDelta = (touchZeroPrevPos + touchOnePrevPos) / 2f; 
            return; 
        }

        if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
        {
            Vector2 currentTouchPos = (touchZero.position + touchOne.position) / 2f;
            Vector2 previousTouchPos = (touchZeroPrevPos + touchOnePrevPos) / 2f;
            Vector3 currentWorldPos = mainCamera.ScreenToWorldPoint(currentTouchPos);
            Vector3 onePixelRightWorldPos = mainCamera.ScreenToWorldPoint(currentTouchPos + Vector2.right);
            float worldUnitsPerPixel = (onePixelRightWorldPos - currentWorldPos).magnitude;
            Vector2 pixelDelta = currentTouchPos - previousTouchPos;
            Vector3 move = new Vector3(pixelDelta.x, pixelDelta.y, 0) * worldUnitsPerPixel;

            mainCamera.transform.position -= move; 
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