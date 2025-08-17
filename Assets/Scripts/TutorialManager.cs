using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    // =================================================================================
    // ENUMS
    // =================================================================================
    public enum TutorialState
    {
        Start,
        AddCells,
        PanCamera,
        ZoomCamera,
        RemoveCells,
        PurposeMessage1,
        PurposeMessage2,
        Rules,
        ButtonsPanel,
        StartButton,
        PauseButton,
        RestartButton,
        End
    }

    public enum StatePhase
    {
        Start,
        Update,
        End
    }

    // =================================================================================
    // REFERENCES
    // =================================================================================
    [Header("References")]
    [SerializeField] private TouchHandler touchHandler;
    [SerializeField] private CellGrid cellGrid;
    [SerializeField] private PresentationFlowManager rulesPresentationManager;
    [SerializeField] private UIButtonController uiButtonController;
    [SerializeField] private ButtonPanelSlider buttonPanelSlider;
    [SerializeField] private TextMeshProUGUI messageBox;
    [SerializeField] private CanvasGroup messageBoxCanvasGroup;
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private Button buttonsPanelButtonON;
    [SerializeField] private Button buttonsPanelButtonOFF;
    [SerializeField] private GameObject rulesContainer;

    // =================================================================================
    // CONFIGURATION
    // =================================================================================
    [Header("Configuration")]
    [SerializeField] private int charsPerSecond = 10;
    private const int minCellsToAdd = 10;
    private const int minCameraPanDistance = 15;
    private const float minCameraZoom = 20f;

    // =================================================================================
    // MESSAGES
    // =================================================================================
    private const string startMessage = "Let's learn the controls!\nTap anywhere to start.";
    private const string addCellsMessage = "Tap or drag on the screen to create at least {0} living cells.\n{1}/{0}";
    private const string removeCellsMessage = "Remove all cells by tapping or dragging over them.";
    private const string panCameraMessage = "Pan the camera by dragging with two fingers.";
    private const string zoomCameraMessage = "Zoom the camera by pinching two fingers.";
    private const string buttonsPanelMessage = "Tap the button at the bottom to open the panel.";
    private const string pauseButtonMessage = "The Pause button stops the simulation. Click it to pause the evolution.";
    private const string startButtonMessage = "First, add some cells. Then, tap Start to begin the simulation and watch them evolve.\n You can't add cells while it's running.\n ";
    private const string tutorialEndMessage = "Tutorial completed.";
    private const string restartButtonMessage = "The Restart button resets the simulation. Click it to start over with a fresh grid.";
    private const string gamePurposeMessage1 = "It's a zero-player game with no set purpose...";
    private const string gamePurposeMessage2 = "... much like life itself";

    // =================================================================================
    // STATE
    // =================================================================================
    private bool startMessageDone = false;
    private bool isStartButtonClicked = false;
    private TutorialState tutorialState = TutorialState.Start;
    private StatePhase statePhase = StatePhase.Start;


private void Start()
    {
        if (PlayerPrefsManager.HasCompletedTutorial)
        {
            Debug.Log("Tutorial already completed. Skipping tutorial.");
            buttonPanelSlider.SlideIn();
            gameObject.SetActive(false);
            return;
        }
    }
    private void Update()
    {
        switch (tutorialState)
        {
            case TutorialState.Start:
                HandleStartState();
                break;
            case TutorialState.AddCells:
                HandleAddCellsState();
                break;
            case TutorialState.RemoveCells:
                HandleRemoveCellsState();
                break;
            case TutorialState.PanCamera:
                HandlePanCameraState();
                break;
            case TutorialState.ZoomCamera:
                HandleZoomCameraState();
                break;
            case TutorialState.PurposeMessage1:
                HandlePurposeMessage1();
                break;
            case TutorialState.PurposeMessage2:
                HandlePurposeMessage2();
                break;
            case TutorialState.Rules:
                HandleRulesState();
                break;
            case TutorialState.ButtonsPanel:
                HandleButtonsPanelState();
                break;
            case TutorialState.StartButton:
                HandleStartButtonState();
                break;
            case TutorialState.PauseButton:
                HandlePauseButtonState();
                break;
            case TutorialState.RestartButton:
                HandleRestartButtonState();
                break;
            case TutorialState.End:
                HandleEndState();
                break;
        }
    }

    private void ShowMessage(string text, Action onAnimationComplete)
    {
        messageBox.text = text;
        Tween messageTween = TextAnimator.AnimateTextByCharactersPerSecond(messageBox, charsPerSecond);
        messageTween.OnComplete(() =>
        {
            onAnimationComplete.Invoke();   
        });
    }
    private void HideMessage(Action onFadeOutComplete)
    {
        if (messageBoxCanvasGroup != null)
        {
            messageBoxCanvasGroup.DOFade(0, 0.5f)
                .OnComplete(() =>
                {
                    messageBox.text = "";
                    messageBoxCanvasGroup.alpha = 1; 
                    onFadeOutComplete.Invoke();
                });
        }
    }
    private void HandleStartState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(startMessage, () => { startMessageDone = true; });
                buttonsPanelButtonON.gameObject.SetActive(false);
                
                uiButtonController.SetAllButtonsInteractable(false);
                uiButtonController.SetButtonInteractable(UIButtonController.ButtonType.Start, true);
                statePhase = StatePhase.Update;
                touchHandler.SetCanAddCells(false);
                touchHandler.SetCanRemoveCells(false);
                touchHandler.SetCanPanCamera(false);
                touchHandler.SetCanZoomCamera(false);
                break;
            case StatePhase.Update:
                if (startMessageDone && Input.GetMouseButtonDown(0))
                {
                    statePhase = StatePhase.End;
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.AddCells;
                        statePhase = StatePhase.Start;
                    });
                }
                break;

            
        }
        
    }

    private void HandleAddCellsState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(addCellsMessage.Replace("{0}", minCellsToAdd.ToString()), () => { touchHandler.SetCanAddCells(true); });
                statePhase = StatePhase.Update;
                break;

            case StatePhase.Update:
                messageBox.text = addCellsMessage.Replace("{0}", minCellsToAdd.ToString()).Replace("{1}", (touchHandler.GetCellsAddedCount()).ToString());
                if (touchHandler.GetCellsAddedCount() >= minCellsToAdd)
                { 
                    statePhase = StatePhase.End;
                    touchHandler.SetCanAddCells(false);
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.PanCamera;
                        statePhase = StatePhase.Start;
                    });
                    
                }
                break;
        }
    }



    private void HandlePanCameraState()
    {
       switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(panCameraMessage, () => { touchHandler.SetCanPanCamera(true); });
                statePhase = StatePhase.Update;
                break;

            case StatePhase.Update:
                if (Mathf.Abs(touchHandler.GetCurrentPanAmount()) >= minCameraPanDistance)
                {
                    statePhase = StatePhase.End;
                    touchHandler.SetCanPanCamera(false);
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.ZoomCamera;
                        statePhase = StatePhase.Start;
                    });
                }
                break;
        }
    }

    private void HandleZoomCameraState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(zoomCameraMessage, () => { touchHandler.SetCanZoomCamera(true); });
                statePhase = StatePhase.Update;
                break;

            case StatePhase.Update:
                if (touchHandler.GetCurrentZoomAmount() >= minCameraZoom)
                {
                    statePhase = StatePhase.End;
                    touchHandler.SetCanZoomCamera(false);
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.RemoveCells;
                        statePhase = StatePhase.Start;
                    });
                }
                break;
        }
    }
    private void HandleRemoveCellsState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(removeCellsMessage, () => { 
                    touchHandler.SetCanRemoveCells(true);
                    touchHandler.SetCanPanCamera(true);
                    touchHandler.SetCanZoomCamera(true);                
                });
                statePhase = StatePhase.Update;
                break;

            case StatePhase.Update:
                if (cellGrid.IsLivingCellsSetEmpty())
                {
                    statePhase = StatePhase.End;
                    touchHandler.SetCanRemoveCells(false);
                    touchHandler.SetCanPanCamera(false);
                    touchHandler.SetCanZoomCamera(false);
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.ButtonsPanel;
                        statePhase = StatePhase.Start;
                    });
                }
                break;
        }
    }


    private void HandleButtonsPanelState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(buttonsPanelMessage, () => { 
                    buttonsPanelButtonON.gameObject.SetActive(true);

                });
                statePhase = StatePhase.Update;
                break;

            case StatePhase.Update:
                if (buttonPanelSlider.IsExtended())
                {
                    statePhase = StatePhase.End;
                    buttonsPanelButtonOFF.gameObject.SetActive(false);
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.PurposeMessage1;
                        statePhase = StatePhase.Start;
                    });
                }
                break;
        }
    }

    private void HandlePurposeMessage1()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(gamePurposeMessage1, () => { statePhase = StatePhase.End; });
                statePhase = StatePhase.Update;
                break;
            case StatePhase.End:
                if (Input.GetMouseButtonDown(0))
                {
                    statePhase = StatePhase.End;
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.PurposeMessage2;
                        statePhase = StatePhase.Start;
                    });
                }
                break;
        }
    }
    private void HandlePurposeMessage2()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(gamePurposeMessage2, () => { statePhase = StatePhase.End; });
                statePhase = StatePhase.Update;
                break;
            case StatePhase.End:
                if (Input.GetMouseButtonDown(0))
                {
                    statePhase = StatePhase.End;
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.StartButton;
                        statePhase = StatePhase.Start;
                    });
                }
                break;
        }
    }


    private void HandleRulesState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:

                rulesContainer.SetActive(true);
                rulesPresentationManager.StartPresentation(() => {
                    statePhase = StatePhase.Update;
                });
                
                statePhase = StatePhase.Update;
                break;

            case StatePhase.Update:
                if (rulesPresentationManager.IsPresentationFinished() && Input.GetMouseButtonDown(0))
                {
                    statePhase = StatePhase.End;
                    rulesContainer.SetActive(false);
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.StartButton;
                        statePhase = StatePhase.Start;
                    });
                }
                break;
        }
    }
    private void HandleStartButtonState()
    {
        
        switch (statePhase)
        {
            case (StatePhase.Start):
                ShowMessage(startButtonMessage, () =>
                {
                    touchHandler.SetCanAddCells(true);
                    touchHandler.SetCanRemoveCells(true);
                    touchHandler.SetCanPanCamera(true);
                    touchHandler.SetCanZoomCamera(true);
                });
                uiButtonController.OnStartButtonClicked += OnStartButtonHandled;
                statePhase = StatePhase.Update;
                break;
            case (StatePhase.Update):
                if (isStartButtonClicked)
                { 
                    statePhase = StatePhase.End;
                    HideMessage(() =>
                    {
                        uiButtonController.OnStartButtonClicked -= OnStartButtonHandled;
                        tutorialState = TutorialState.End;
                        statePhase = StatePhase.Start;
                    });
                }
                break;
        }
    }

    private void HandlePauseButtonState()
    {

    }

    private void HandleRestartButtonState()
    {
        throw new NotImplementedException();
    }

    private void HandleEndState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(tutorialEndMessage, () => { StartCoroutine(DelayHideMessage(1f, () => { })); });
                buttonsPanelButtonOFF.gameObject.SetActive(true);
                uiButtonController.SetAllButtonsInteractable(true);
                uiButtonController.SetButtonInteractable(UIButtonController.ButtonType.Save, false);
                uiButtonController.SetButtonInteractable(UIButtonController.ButtonType.Load, false);
                statePhase = StatePhase.End;
                PlayerPrefsManager.HasCompletedTutorial = true;

                break;
        }
    }

    private IEnumerator DelayHideMessage(float delay, Action onComplete = null)
    {
        yield return new WaitForSeconds(delay);
        HideMessage(onComplete);
    }
    private void OnStartButtonHandled()
    {
        if (!cellGrid.IsLivingCellsSetEmpty())
        {
            isStartButtonClicked = true;
        }
    }
}