using DG.Tweening;
using System;
using System.Collections;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    // =================================================================================
    // ENUMS
    // =================================================================================
    public enum TutorialState
    {
        Start,
        TouchScreen,
        FirstLiveCell,
        Reproduction,
        
        FirstCellDeath,
        AddMoreToSurvive,
        TryAgainSurvive,
        FirstCellBirth,
        AddTooManyCells,
        ZoomOutCamera,
        Overpopulation,
        AfterOverpopulation,


        DrawIcon,

        AddCells, // OLD TUTORIAL
        PanCamera,
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
    [SerializeField] private CellManager cellGrid;
    [SerializeField] private PresentationFlowManager rulesPresentationManager;
    [SerializeField] private UIButtonController uiButtonController;
    [SerializeField] private ButtonPanelSlider buttonPanelSlider;
    [SerializeField] private TextMeshProUGUI upperMessageBox;
    [SerializeField] private TextMeshProUGUI middleMessageBox;
    [SerializeField] private TextMeshProUGUI lowerMessageBox;
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private Button buttonsPanelButtonON;
    [SerializeField] private Button buttonsPanelButtonOFF;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject rulesContainer;
    [SerializeField] private GameObject tutorialSpace;

    // =================================================================================
    // CONFIGURATION
    // =================================================================================
    [Header("Configuration")]
    [SerializeField] private int charsPerSecond = 10;
    private const int minCellsToAdd = 16;
    private const int minCameraPanDistance = 15;
    private const float minCameraZoom = 20f;

    // =================================================================================
    // MESSAGES
    // =================================================================================
    private const string startMessage = "Let's learn the controls!\nTap anywhere to start";
    private const string touchScreenMessage = "Tap the center of the screen";
    private const string firstLiveCellMessage = "This is a live cell ";
    private const string firstCellDeathMessage = "Loneliness kills";
    private const string addMoreToSurviveMessage = "Add more cells to keep them alive";
    private const string tryAgainSurviveMessage = "Try again\nThey are too far apart";
    private const string wellDoneSurviveMessage = "Well done";
    private const string firstCellBirthMessage = "A cell comes to life when enough neighbors are around";
    private const string addTooManyCellsMessage = "Add more cells\n{1}/{0} ";
    private const string overpopulationMessage = "Overpopulation causes death";
    private const string drawIconMessage = "Draw the glider from game icon";
    private const string addCellsMessage = "Tap or drag on the screen to create at least {0} living cells\n{1}/{0}";
    private const string removeCellsMessage = "Remove all cells by tapping or dragging over them";
    private const string panCameraMessage = "Pan the camera by dragging with two fingers";
    private const string zoomCameraMessage = "Zoom out the camera by pinching two fingers";
    private const string buttonsPanelMessage = "Tap the button at the bottom to open the panel";
    private const string pauseButtonMessage = "The Pause button stops the simulation. Click it to pause the evolution";
    private const string startButtonMessage = "First, add some cells. Then, tap Play to begin the simulation and watch them evolve\n You can't add cells while it's running\n ";
    private const string tutorialEndMessage = "Tutorial completed.";
    private const string restartButtonMessage = "The Restart button resets the simulation. Click it to start over with a fresh grid";
    private const string gamePurposeMessage1 = "It's a zero-player game with no set purpose...";
    private const string gamePurposeMessage2 = "... much like life itself";

    // =================================================================================
    // STATE
    // =================================================================================
    private bool isStartButtonClicked = false;
    private bool isTutorialOn = false;
    private TutorialState tutorialState = TutorialState.End;
    private StatePhase statePhase = StatePhase.End;

    private Action _standaloneRulesOnClosed;
    private bool _standaloneRulesWaitingForClick;
    [SerializeField] private float standaloneRulesReenableInteractionDelay = 0.4f;

    private int _generationCount = 0;

    public int GenerationCount
    {
        set
        {
            _generationCount = value;
        }
    }

    public Action OnAllCellsDead { get; set; }
    public Action OnRequiredCellsCount { get; set; }
    public Action OnBeforeFirstCellBirth { get; set; }
    public Action OnTutorialCompleted { get; set; }
    public Action OnIconGliderDrawn { get; set; }
    public Action OnFirstGenerationDraw { get; set; }
    public Action OnSkipTutorial { get; set; }
    public Action OnPressStartButtoState { get; set; }

    private void Start()
    {
        
        //if (PlayerPrefsManager.HasCompletedTutorial)
        //{
        //    Debug.Log("Tutorial already completed. Skipping tutorial.");
        //    buttonPanelSlider.SlideIn();
        //    gameObject.SetActive(false);
        //    return;
        //}
    }
    private void Update()
    {
        if (_standaloneRulesOnClosed != null && rulesContainer != null && rulesContainer.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (_standaloneRulesWaitingForClick)
            {
                _standaloneRulesWaitingForClick = false;
                DoStandaloneRulesCleanup();
            }
            else
            {
                rulesPresentationManager?.StopPresentation();
            }
            return;
        }

        if (!isTutorialOn) return;

        switch (tutorialState)
        {
            case TutorialState.Start:
                HandleStartState();
                break;
            case TutorialState.TouchScreen:
                HandleTouchScreen();
                break;
            case TutorialState.FirstLiveCell:
                HandleFirstLiveCellState();
                break;
            case TutorialState.FirstCellDeath:
                HandleFirstCellDeathState();
                break;
            case TutorialState.AddMoreToSurvive:
                HandleAddMoreToSurviveState();
                break;
            case TutorialState.TryAgainSurvive:
                HandleTryAgainToSurviveState();
                break;
            case TutorialState.FirstCellBirth:
                HandleFirstCellBirthState();
                break;
            case TutorialState.AddTooManyCells:
                HandleAddTooManyCellsState();
                break;
            case TutorialState.ZoomOutCamera:
                HandleZoomOutCameraState();
                break;
            case TutorialState.Overpopulation:
                HandleOverpopulationState();
                break;
            case TutorialState.AfterOverpopulation:
                HandleAfterOverpopulationState();
                break;
            case TutorialState.DrawIcon:
                HandleDrawIconState();
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

    private void ShowMessage(string text, Action onAnimationComplete, TextMeshProUGUI messageBox)
    {
        messageBox.text = text;
        Tween messageTween = TextAnimator.AnimateTextByCharactersPerSecond(messageBox, charsPerSecond);
        messageTween.OnComplete(() =>
        {
            onAnimationComplete.Invoke();   
        });
    }
    private void HideMessage(Action onFadeOutComplete, TextMeshProUGUI messageBox)
    {
        CanvasGroup messageBoxCanvasGroup = messageBox.gameObject.GetComponent<CanvasGroup>();
        if (messageBoxCanvasGroup != null)
        {
            messageBoxCanvasGroup.DOFade(0, 0.3f)
                .OnComplete(() =>
                {
                    messageBox.text = "";
                    messageBoxCanvasGroup.alpha = 1;
                    onFadeOutComplete.Invoke();
                });
        }
        else
        {
            Debug.LogWarning("HideMessage: CanvasGroup component not found on the message box parent.");
            messageBox.text = "";
            onFadeOutComplete.Invoke();
        }
    }
    private void HandleStartState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                buttonsPanelButtonON.gameObject.SetActive(false);
                uiButtonController.SetAllButtonsInteractable(false);
                touchHandler.SetCanAddCells(false);
                touchHandler.SetCanRemoveCells(false);
                touchHandler.SetCanPanCamera(false);
                touchHandler.SetCanZoomCamera(false);
                tutorialState = TutorialState.TouchScreen;
                statePhase = StatePhase.Start;
                break;   
        }    
    }
    private void HandleTouchScreen()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                StartCoroutine(WaitAndProceed(1f, () => { 
                ShowMessage(touchScreenMessage, () => {
                    touchHandler.SetCanAddCells(true);
                }, upperMessageBox);
                }));
                statePhase = StatePhase.Update;

                break;
            case StatePhase.Update:
                if (cellGrid.GetLivingCells().Count >= 1)
                {
                    touchHandler.SetCanAddCells(false);
                    statePhase = StatePhase.End;
                    HideMessage(() => 
                    {
                        tutorialState = TutorialState.FirstLiveCell;
                        statePhase = StatePhase.Start;
                    }, upperMessageBox);
                }
                break;
            
        }

    }
    private void HandleFirstLiveCellState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                StartCoroutine(WaitAndProceed(0.2f, () => {  
                ShowMessage(firstLiveCellMessage, () => {
                    StartCoroutine(WaitAndProceed(0.8f, () => { OnRequiredCellsCount?.Invoke(); }));
                }, upperMessageBox);
                }));
                statePhase = StatePhase.Update;
                break;
            case StatePhase.Update:
                if (cellGrid.IsLivingCellsSetEmpty())
                {
                    tutorialState = TutorialState.FirstCellDeath;
                    statePhase = StatePhase.Start;
                    OnAllCellsDead?.Invoke();
                    upperMessageBox.text = firstLiveCellMessage.Replace(" is ", " was ");
                }
                break;

        }
    }
    private void HandleFirstCellDeathState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                StartCoroutine(WaitAndProceed(1.2f, () => {  
                ShowMessage(firstCellDeathMessage, () => { StartCoroutine(WaitAndProceed(2.5f, () => {
                    statePhase = StatePhase.End; 

                })); }, lowerMessageBox);
                }));
                statePhase = StatePhase.Update;
                break;
            case StatePhase.End:
                statePhase = StatePhase.Update;
                HideMessage(() =>
                {
                    tutorialState = TutorialState.AddMoreToSurvive;
                    statePhase = StatePhase.Start;
                }, lowerMessageBox);
                HideMessage(() => { }, upperMessageBox);
                break;

        }
    }
    private void HandleAddMoreToSurviveState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                StartCoroutine(WaitAndProceed(0.5f, () => { 
                ShowMessage(addMoreToSurviveMessage, () => 
                {
                    touchHandler.SetCanAddCells(true);
                    touchHandler.SetCanRemoveCells(true);
                    skipButton.gameObject.SetActive(true);
                }, upperMessageBox);
                }));
                statePhase = StatePhase.Update;
                break;
            case StatePhase.Update:
                if (cellGrid.GetLivingCells().Count == 3)
                {
                    touchHandler.SetCanAddCells(false);
                    touchHandler.SetCanRemoveCells(false);
                    statePhase = StatePhase.End;
                    HideMessage(() =>
                    {
                        OnRequiredCellsCount?.Invoke();
                        tutorialState = TutorialState.TryAgainSurvive;
                        statePhase = StatePhase.Start;
                    }, upperMessageBox);
                }
                break;
        }
    }
    private void HandleTryAgainToSurviveState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                statePhase = StatePhase.Update;
                break;
            case StatePhase.Update:
                if (cellGrid.IsLivingCellsSetEmpty())
                {
                    ShowStateTransition(tryAgainSurviveMessage, TutorialState.AddMoreToSurvive, lowerMessageBox, 0.5f, 3f);
                }
                else if (cellGrid.GetLivingCells().Count >= 3 && _generationCount >= 2)
                {
                    ShowStateTransition(wellDoneSurviveMessage, TutorialState.FirstCellBirth, lowerMessageBox, 0.5f, 1.5f);
                }
                break;
        }
    }
    private void HandleFirstCellBirthState()
    {
       switch (statePhase)
        {
            case StatePhase.Start:
                OnBeforeFirstCellBirth?.Invoke();
                statePhase = StatePhase.Update;
                break;
            case StatePhase.Update:
                if (cellGrid.GetLivingCells().Count >= 4 && _generationCount >= 1)
                {
                    OnAllCellsDead?.Invoke();
                    statePhase = StatePhase.End;
                    StartCoroutine(WaitAndProceed(0.8f, () =>
                    {
                        ShowMessage(firstCellBirthMessage, () =>
                        {
                           tutorialState = TutorialState.AddTooManyCells;
                           statePhase = StatePhase.Start;
                        }, lowerMessageBox);
                    }));
                }
                break;
        }
    }
    private void HandleAddTooManyCellsState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                StartCoroutine(WaitAndProceed(1.5f, () =>
                {
                    ShowMessage(addTooManyCellsMessage.Replace("{0}",minCellsToAdd.ToString()).Replace("{1}",cellGrid.GetLivingCells().Count.ToString()), () => 
                    { 
                        touchHandler.SetCanAddCells(true);
                        touchHandler.SetCanRemoveCells(true);
                        statePhase = StatePhase.Update;
                    }, upperMessageBox);
                }));
                statePhase = StatePhase.End;
                break;
            case StatePhase.Update:
                upperMessageBox.text = addTooManyCellsMessage.Replace("{0}", minCellsToAdd.ToString()).Replace("{1}", (cellGrid.GetLivingCells().Count.ToString()).ToString());
                if (cellGrid.GetLivingCells().Count >= minCellsToAdd)
                {
                    touchHandler.SetCanAddCells(false);
                    touchHandler.SetCanRemoveCells(false);
                    tutorialSpace.SetActive(false);
                    statePhase = StatePhase.End;
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.ZoomOutCamera;
                        statePhase = StatePhase.Start;
                    }, upperMessageBox);
                    HideMessage(() => { }, lowerMessageBox);
                }
                break;
        }
    }
    private void HandleZoomOutCameraState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(zoomCameraMessage, () => { touchHandler.SetCanZoomCamera(true); }, upperMessageBox);
                statePhase = StatePhase.Update;
                break;

            case StatePhase.Update:
                if (touchHandler.GetCurrentOrthographicSize() >= 8f)
                {
                    
                    statePhase = StatePhase.End;
                    touchHandler.SetCanZoomCamera(false);
                    HideMessage(() =>
                    {
                        OnRequiredCellsCount?.Invoke();
                        tutorialState = TutorialState.Overpopulation;
                        statePhase = StatePhase.Start;
                    }, upperMessageBox);
                }
                break;
        }
    }
    private void HandleOverpopulationState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                statePhase = StatePhase.Update;
                break;
            case StatePhase.Update:
                if (_generationCount >= 1)
                {
                    ShowStateTransition(overpopulationMessage, TutorialState.AfterOverpopulation, lowerMessageBox, 0.5f, 1.5f);
                }
                break;
        }
    }
    private void HandleAfterOverpopulationState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                OnRequiredCellsCount?.Invoke();
                statePhase = StatePhase.Update;
                break;
            case StatePhase.Update:
                if (cellGrid.IsLivingCellsSetEmpty())
                {
                    statePhase = StatePhase.End;
                    OnAllCellsDead?.Invoke();
                    StartCoroutine(WaitAndProceed(0.5f, () =>
                    {                     
                        tutorialState = TutorialState.Rules;
                        statePhase = StatePhase.Start;
                    }));
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
                    }, upperMessageBox);
                }
                break;
        }
    }

    public void ShowRulesStandalone(Action<Action> onOpen, Action onClosed)
    {
        if (isTutorialOn) return;
        if (rulesContainer == null || rulesPresentationManager == null) return;

        _standaloneRulesOnClosed = onClosed;
        _standaloneRulesWaitingForClick = false;
        onOpen?.Invoke(() =>
        {
            if (touchHandler != null)
            {
                touchHandler.SetCanAddCells(false);
                touchHandler.SetCanRemoveCells(false);
            }
            if (cellGrid != null) cellGrid.HideGrid();
            rulesContainer.SetActive(true);
            rulesPresentationManager.StartPresentation(
                () => { _standaloneRulesWaitingForClick = true; },
                DoStandaloneRulesCleanup);
        });
    }

    private void DoStandaloneRulesCleanup()
    {
        _standaloneRulesWaitingForClick = false;
        if (rulesContainer != null) rulesContainer.SetActive(false);
        if (cellGrid != null) cellGrid.RestoreFromHide();
        StartCoroutine(ReenableCellInteractionNextFrame());
        _standaloneRulesOnClosed?.Invoke();
        _standaloneRulesOnClosed = null;
    }

    private IEnumerator ReenableCellInteractionNextFrame()
    {
        yield return new WaitForSeconds(standaloneRulesReenableInteractionDelay);
        if (touchHandler != null)
        {
            touchHandler.SetCanAddCells(true);
            touchHandler.SetCanRemoveCells(true);
        }
    }

    public void CloseStandaloneRules()
    {
        if (_standaloneRulesOnClosed == null) return;
        rulesPresentationManager?.StopPresentation();
    }

    private void HandleDrawIconState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(drawIconMessage, () => 
                { 
                    touchHandler.SetCanAddCells(true);
                    touchHandler.SetCanRemoveCells(true);
                    statePhase = StatePhase.Update;
                }, upperMessageBox);
                statePhase = StatePhase.Update;
                break;
            case StatePhase.Update:
                if (cellGrid.GetLivingCells().Count >= 5)
                {
                    touchHandler.SetCanAddCells(false);
                    touchHandler.SetCanRemoveCells(false);
                    statePhase = StatePhase.End;
                    HideMessage(() =>
                    {
                        OnRequiredCellsCount?.Invoke();
                        OnIconGliderDrawn?.Invoke();
                        tutorialState = TutorialState.AddCells;
                        statePhase = StatePhase.Start;
                    }, upperMessageBox);
                }
                break;
        }
    }
    private void HandleAddCellsState()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(addCellsMessage.Replace("{0}", minCellsToAdd.ToString()), () => { touchHandler.SetCanAddCells(true); }, upperMessageBox);
                statePhase = StatePhase.Update;
                break;

            case StatePhase.Update:
                upperMessageBox.text = addCellsMessage.Replace("{0}", minCellsToAdd.ToString()).Replace("{1}", (cellGrid.GetLivingCells().Count).ToString());
                if (touchHandler.GetCellsAddedCount() >= minCellsToAdd)
                { 
                    statePhase = StatePhase.End;
                    touchHandler.SetCanAddCells(false);
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.PanCamera;
                        statePhase = StatePhase.Start;
                    }, upperMessageBox);

                }
                break;
        }
    }



    private void HandlePanCameraState()
    {
       switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(panCameraMessage, () => { touchHandler.SetCanPanCamera(true); }, upperMessageBox);
                statePhase = StatePhase.Update;
                break;

            case StatePhase.Update:
                if (Mathf.Abs(touchHandler.GetCurrentPanAmount()) >= minCameraPanDistance)
                {
                    statePhase = StatePhase.End;
                    touchHandler.SetCanPanCamera(false);
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.ZoomOutCamera;
                        statePhase = StatePhase.Start;
                    }, upperMessageBox);
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
                }, upperMessageBox);
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
                    }, upperMessageBox);
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

                }, upperMessageBox);
                statePhase = StatePhase.Update;
                break;

            case StatePhase.Update:
                if (!buttonPanelSlider.IsHidden())
                {
                    statePhase = StatePhase.End;
                    buttonsPanelButtonOFF.gameObject.SetActive(false);
                    HideMessage(() =>
                    {
                        tutorialState = TutorialState.StartButton;
                        statePhase = StatePhase.Start;
                    }, upperMessageBox);
                }
                break;
        }
    }

    private void HandlePurposeMessage1()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(gamePurposeMessage1, () => {
                    StartCoroutine(WaitAndProceed(1.8f, () => { statePhase = StatePhase.End; }));
                     }, upperMessageBox);
                statePhase = StatePhase.Update;
                break;
            case StatePhase.End:
                statePhase = StatePhase.Update;
                HideMessage(() =>
                {
                    tutorialState = TutorialState.PurposeMessage2;
                    statePhase = StatePhase.Start;
                }, upperMessageBox);
                break;
        }
    }
    private void HandlePurposeMessage2()
    {
        switch (statePhase)
        {
            case StatePhase.Start:
                ShowMessage(gamePurposeMessage2, () => {
                    StartCoroutine(WaitAndProceed(2.5f, () => { statePhase = StatePhase.End; }));
                }, upperMessageBox);
                statePhase = StatePhase.Update;
                break;
            case StatePhase.End:
                statePhase = StatePhase.Update;
                HideMessage(() =>
                {
                    tutorialState = TutorialState.End;
                    statePhase = StatePhase.Start;
                }, upperMessageBox);
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
                    OnPressStartButtoState?.Invoke();
                    uiButtonController.SetButtonInteractable(UIButtonController.ButtonType.Start, true);
                    touchHandler.SetCanAddCells(true);
                    touchHandler.SetCanRemoveCells(true);
                    touchHandler.SetCanPanCamera(true);
                    touchHandler.SetCanZoomCamera(true);
                    OnFirstGenerationDraw?.Invoke();
                }, upperMessageBox);
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
                        tutorialState = TutorialState.PurposeMessage1;
                        statePhase = StatePhase.Start;
                    }, upperMessageBox);
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
                ShowMessage(tutorialEndMessage, () => { StartCoroutine(WaitAndProceed(1f, () => { HideMessage(() => { },upperMessageBox); })); }, upperMessageBox);
                //buttonsPanelButtonOFF.gameObject.SetActive(true);
                uiButtonController.SetAllButtonsInteractable(true);
                uiButtonController.SetButtonInteractable(UIButtonController.ButtonType.Save, false);
                uiButtonController.SetButtonInteractable(UIButtonController.ButtonType.Load, false);
                statePhase = StatePhase.End;
                isTutorialOn = false;
                PlayerPrefsManager.HasCompletedTutorial = true;
                skipButton.gameObject.SetActive(false);
                OnTutorialCompleted?.Invoke();
                break;
        }
    }

    private IEnumerator WaitAndProceed(float waitTime, Action onComplete = null)
    {
        yield return new WaitForSeconds(waitTime);
        onComplete?.Invoke();
    }
    private void OnStartButtonHandled()
    {
        if (!cellGrid.IsLivingCellsSetEmpty())
        {
            isStartButtonClicked = true;
        }
    }
    public void StartTutorial()
    {
        isTutorialOn = true;
        tutorialSpace.SetActive(true);
        tutorialState = TutorialState.Start;
        statePhase = StatePhase.Start;
    }
    private void ShowStateTransition(string message, TutorialState nextState, TextMeshProUGUI messageBox, float waitBeforeMsg = 1f, float waitAfterMsg = 2f)
    {
        OnAllCellsDead?.Invoke();
        statePhase = StatePhase.End;
        StartCoroutine(WaitAndProceed(waitBeforeMsg, () =>
        {
            ShowMessage(message, () =>
            {
                StartCoroutine(WaitAndProceed(waitAfterMsg, () =>
                {
                    HideMessage(() =>
                    {
                        tutorialState = nextState;
                        statePhase = StatePhase.Start;
                    }, messageBox);
                }));
            }, messageBox);
        }));
    }
    public void SkipTutorial()
    {
        OnSkipTutorial?.Invoke();
    }

}