using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class TouchHandler : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private CellGrid cellManager;
    [SerializeField] private GameController gameController;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;

    private Camera mainCamera;
    private bool firstClickState;
    private Vector2 prevTouchDelta;

    private enum TouchState { Idle, WaitingForSecondTouch, SingleTouch, MultiTouch }
    private TouchState currentState = TouchState.Idle;
    private float singleTouchTimer;
    private float singleTouchDelay = 0.1f;
    private Touch singleTouch;
    

    void Start()
    {
        mainCamera = Camera.main;
        mainCamera.nearClipPlane = 0.1f;

        if (tilemap == null)
            Debug.LogError("Tilemap not assigned to CellClickHandler!");

        if (cellManager == null)
            Debug.LogError("CellManager not assigned to CellClickHandler!");
    }

    void Update()
    {
        if (Input.touchCount > 0 && IsPointerOverUI(0))
        {
            currentState = TouchState.Idle;
            return;
        }
        switch (currentState)
        {
            case TouchState.Idle:
                if (Input.touchCount == 1)
                {
                    singleTouch = Input.GetTouch(0);
                    singleTouchTimer = Time.time;
                    currentState = TouchState.WaitingForSecondTouch;
                }
                else if (Input.touchCount == 2)
                {
                    currentState = TouchState.MultiTouch;
                    HandleTwoTouches();
                }
                break;
            case TouchState.WaitingForSecondTouch:
                if (Input.touchCount == 2)
                {
                    currentState = TouchState.MultiTouch;
                    HandleTwoTouches();
                }
                else if (Time.time - singleTouchTimer > singleTouchDelay)
                {
                    currentState = TouchState.SingleTouch;
                    HandleSingleTouch(singleTouch);
                }
                break;
            case TouchState.SingleTouch:
                if (Input.touchCount == 0)
                {
                    currentState = TouchState.Idle;
                }
                else
                {
                    HandleSingleTouch(Input.GetTouch(0));
                }
                break;
            case TouchState.MultiTouch:
                if (Input.touchCount < 1)
                {
                    currentState = TouchState.Idle;
                }
                else if (Input.touchCount != 1) 
                {
                    HandleTwoTouches();
                }
                break;
        }
    }

    void HandleSingleTouch(Touch touch)
    {
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
            prevTouchDelta = (touchZero.position + touchOne.position) / 2f;
            return;
        }

        if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
        {
            Vector2 currentTouchPos = (touchZero.position + touchOne.position) / 2f;
            Vector2 previousTouchPos = prevTouchDelta;

            float prevTouchDistance = Vector2.Distance(touchZeroPrevPos, touchOnePrevPos);
            float currentTouchDistance = Vector2.Distance(touchZero.position, touchOne.position);
            float distanceDelta = currentTouchDistance - prevTouchDistance;

            mainCamera.orthographicSize -= distanceDelta * zoomSpeed * mainCamera.orthographicSize * Time.deltaTime;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);

            Vector3 currentWorldPos = mainCamera.ScreenToWorldPoint(currentTouchPos);
            Vector3 onePixelRightWorldPos = mainCamera.ScreenToWorldPoint(currentTouchPos + Vector2.right);
            float worldUnitsPerPixel = (onePixelRightWorldPos - currentWorldPos).magnitude;
            Vector2 pixelDelta = currentTouchPos - previousTouchPos;
            Vector3 move = new Vector3(pixelDelta.x, pixelDelta.y, 0) * worldUnitsPerPixel;
            mainCamera.transform.position -= move;

            prevTouchDelta = currentTouchPos;
        }
    }

    void HandleTouch(Vector3Int cellPosition)
    {
        if (gameController.isRunning) return;

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
    private bool IsPointerOverUI(int fingerId)
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(fingerId);
        else
            return EventSystem.current.IsPointerOverGameObject();
    }

}