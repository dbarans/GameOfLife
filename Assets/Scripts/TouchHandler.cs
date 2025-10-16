using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class TouchHandler : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private CellManager cellManager;
    [SerializeField] private GameController gameController;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 500f;
    private VibrationManager vibrationManager;

    private Camera mainCamera;
    private bool firstClickState;
    private Vector2 prevTouchDelta;

    private enum TouchState { Idle, WaitingForSecondTouch, SingleTouch, MultiTouch }
    private TouchState currentState = TouchState.Idle;
    private float singleTouchTimer;
    private float singleTouchDelay = 0.1f;
    private Touch singleTouch;

    private bool canAddCells = true;
    private bool canRemoveCells = true;
    private bool canPanCamera = true;
    private bool canZoomCamera = true;

    private int cellsAddedCount = 0;
    private float currentZoomAmount = 0f;
    private float currentPanAmount = 0f;

    private bool _isTutorialOn = false;
    public bool IsTutorialOn
    {
        get => _isTutorialOn;
        set => _isTutorialOn = value;
    }


    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Make sure your camera has the 'MainCamera' tag.");
        }
        else
        {
            mainCamera.nearClipPlane = 0.1f;
        }

        if (tilemap == null)
            Debug.LogError("Tilemap not assigned to TouchHandler!");

        if (cellManager == null)
            Debug.LogError("CellManager not assigned to TouchHandler!");

        ResetInteractionCounts();

    }

    private void Update()
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

    private void HandleSingleTouch(Touch touch)
    {
        if (!canAddCells && !canRemoveCells) return;

        if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
        {
            Vector3Int cellPosition = GetCellFromTouch(touch.position);

            if (touch.phase == TouchPhase.Began)
                firstClickState = cellManager.IsCellAlive(cellPosition);

            HandleCellAction(cellPosition, canAddCells, canRemoveCells);
        }
    }

    private void HandleTwoTouches()
    {
        if (!canPanCamera && !canZoomCamera) return;

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

            if (canZoomCamera)
            {
                float prevTouchDistance = Vector2.Distance(touchZeroPrevPos, touchOnePrevPos);
                float currentTouchDistance = Vector2.Distance(touchZero.position, touchOne.position);
                float distanceDelta = currentTouchDistance - prevTouchDistance;

                float originalSize = mainCamera.orthographicSize;
                mainCamera.orthographicSize -= distanceDelta * zoomSpeed * mainCamera.orthographicSize * Time.deltaTime;
                mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);

                currentZoomAmount += Mathf.Abs(mainCamera.orthographicSize - originalSize);
            }

            if (canPanCamera)
            {
                Vector3 currentWorldPos = mainCamera.ScreenToWorldPoint(currentTouchPos);
                Vector3 onePixelRightWorldPos = mainCamera.ScreenToWorldPoint(currentTouchPos + Vector2.right);
                float worldUnitsPerPixel = (onePixelRightWorldPos - currentWorldPos).magnitude;
                Vector2 pixelDelta = currentTouchPos - previousTouchPos;
                Vector3 move = new Vector3(pixelDelta.x, pixelDelta.y, 0) * worldUnitsPerPixel;

                mainCamera.transform.position -= move;

                currentPanAmount += move.magnitude;
            }

            prevTouchDelta = currentTouchPos;
        }
    }

    private void HandleCellAction(Vector3Int cellPosition, bool allowAdd, bool allowRemove)
    {
        if (gameController.isRunning) return;

        bool isCurrentCellAlive = cellManager.IsCellAlive(cellPosition);

        if (allowAdd && !isCurrentCellAlive && firstClickState == false)
        {
            if (_isTutorialOn && !CanAddCellHereDuringTutorial(cellPosition)) return;

            cellManager.SetCellState(cellPosition, true);
            vibrationManager.VibrateOnTouch();
        }
        else if (allowRemove && isCurrentCellAlive && firstClickState == true)
        {
            cellManager.SetCellState(cellPosition, false);
            vibrationManager.VibrateOnTouch();
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
        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }
    #region Tutorial Methods
    public void SetCanAddCells(bool enable)
    {
        canAddCells = enable;
    }

    public void SetCanRemoveCells(bool enable)
    {
        canRemoveCells = enable;
    }

    public void SetCanPanCamera(bool enable)
    {
        canPanCamera = enable;
    }

    public void SetCanZoomCamera(bool enable)
    {
        canZoomCamera = enable;
    }

    public void DisableAllInteractions()
    {
        canAddCells = false;
        canRemoveCells = false;
        canPanCamera = false;
        canZoomCamera = false;
    }

    public int GetCellsAddedCount()
    {
        return cellsAddedCount;
    }

    public float GetCurrentZoomAmount()
    {
        return currentZoomAmount;
    }

    public float GetCurrentPanAmount()
    {
        return currentPanAmount;
    }
    public float GetCurrentOrthographicSize()
    {
        return mainCamera.orthographicSize;
    }

    public void ResetInteractionCounts()
    {
        cellsAddedCount = 0;
        currentZoomAmount = 0f;
        currentPanAmount = 0f;
    }
    #endregion

    internal void SetVibrationManager(VibrationManager vibrationManager)
    {
        this.vibrationManager = vibrationManager;
    }

    private bool CanAddCellHereDuringTutorial(Vector3Int cellPosition)
    {
        return (cellPosition.x >= -2 && cellPosition.x <= 1 && cellPosition.y >= -2 && cellPosition.y <= 1);
    }
}