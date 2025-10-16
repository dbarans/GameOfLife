using Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
      // === References ===
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private CellManager CellManager;
    [SerializeField] private UIButtonController uiButtonController;
    [SerializeField] private ButtonPanelSlider buttonPanelSlider;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private StatsDisplay gameStatsDisplay;
    [SerializeField] private CellRenderer cellRenderer;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private TouchHandler touchHandler;
    [SerializeField] private GameData gameData;
    private VibrationManager vibrationManager;
    private GenerationManager generationManager;
    private GameUIManager gameUIManager;



    // === Configuration ===
    private float genPerSec = 1f;
    private bool vibrateEveryGen = true;
    private bool isTutorial = false;

    // === Runtime state ===
    private bool _isRunning = false;
    private float timeSinceLastGen = 0f;
    private int isNextGenCalculated = 0;

    // === Debug ===
    private int generationsCount = 0;
    

    // === Synchronization ===
    private static readonly object nextGenLock = new object();

    // === Public properties ===
    public bool VibrateEveryGeneration
    {
        get => vibrateEveryGen;
        set => vibrateEveryGen = value;
    }

    public bool isRunning
    {
        get => _isRunning;
    }
    private void Awake()
    {

        vibrationManager = new VibrationManager();
        generationManager = new GenerationManager(CellManager, () => Interlocked.Exchange(ref isNextGenCalculated, 1), nextGenLock);
        gameUIManager = new GameUIManager(buttonPanel, mainCamera, uiButtonController);

        touchHandler.SetVibrationManager(vibrationManager);
        TextAnimator.Initialize(vibrationManager);

        uiButtonController.OnStartButtonClicked += HandleStartPauseToggle;
        uiButtonController.OnPauseButtonClicked += PauseGame;
        uiButtonController.OnResetButtonClicked += ResetGame;
        uiButtonController.OnSaveButtonClicked += SaveGame;
        uiButtonController.OnLoadButtonClicked += LoadGame;
        uiButtonController.OnSpeedChanged += ChangeSpeed;

        if (gameData.isTutorialOn)
        {
            tutorialManager.OnAllCellsDead += () =>
            {
                PauseGame();
                generationsCount = 0;
                tutorialManager.GenerationCount = generationsCount;
            };
            tutorialManager.OnRequiredCellsCount += RunGame;
            tutorialManager.OnBeforeFirstCellBirth += () =>
            {
                generationManager.AllowBirth = true;
                RunGame();
            };
            tutorialManager.OnIconGliderDrawn += () => { genPerSec *= 2; };
            tutorialManager.OnFirstGenerationDraw += () => { touchHandler.IsTutorialOn = false; };
            tutorialManager.OnTutorialCompleted += EndTutorial;

            StartTutorial();
        }
        else
        {
            buttonPanelSlider.SlideIn();
        }
    }


    private void Update()
    {
        if (_isRunning)
        {
            UpdateGeneration();
        }
    }
    private void UpdateGeneration()
    {
        //gameStatsDisplay.UpdateGenerationsPerSecondDisplay(); //Debug: display for generations per second
        timeSinceLastGen += Time.deltaTime;
        if (timeSinceLastGen >= 1f / genPerSec && isNextGenCalculated == 1)
        {
            IncrementGeneration();
            //gameStatsDisplay.IncrementGenerationCount(); //Debug: increment generation count for display
            timeSinceLastGen = 0f;
            Interlocked.Exchange(ref isNextGenCalculated, 0);
            CellManager.SwapGenerations();
            Task.Run(() => generationManager.GenerateNextGeneration());
            if (vibrateEveryGen && genPerSec <= 1f)
            {
                vibrationManager.VibrateOnGeneration();
            }
        }
    }
    private void IncrementGeneration()
    {
        generationsCount++;
        tutorialManager.GenerationCount = generationsCount;
    }
    private void HandleStartPauseToggle()
    {
        if (_isRunning)
        {
            PauseGame();
        }
        else
        {
            RunGame();
        }
    }

    private void StartTutorial()
    {
        isTutorial = true;
        vibrateEveryGen = true;
        tutorialManager.StartTutorial();
        touchHandler.IsTutorialOn = true;
        generationManager.AllowBirth = false;
        genPerSec = 1f;
    }
    private void EndTutorial()
    {
        isTutorial = false;
        touchHandler.IsTutorialOn = false;
        gameData.isTutorialOn = false;
    }
    public void RunGame()
    {
        if (CellManager.IsLivingCellsSetEmpty()) return;

        _isRunning = true;
        generationManager.GenerateNextGeneration();
        gameUIManager.SetRunningUI();
        gameUIManager.UpdateSaveLoadButtons(isRunning);
        uiButtonController.SetStartButtonState(UIButtonController.StartButtonState.Pause);
        uiButtonController.UpdateRunState(_isRunning);

    }
    public void PauseGame()
    {
        _isRunning = false;
        gameUIManager.SetPauseUI();
        uiButtonController.SetStartButtonState(UIButtonController.StartButtonState.Start);
        gameUIManager.UpdateSaveLoadButtons(isRunning);
        uiButtonController.UpdateRunState(_isRunning);
    }
    public void ResetGame()
    {
        PauseGame();
        CellManager.ClearGrid();
        Interlocked.Exchange(ref isNextGenCalculated, 0);
    }

    public void SaveGame()
    {
        if (_isRunning) return;
        Interlocked.Exchange(ref isNextGenCalculated, 0);
        CellManager.SaveGame();
    }
    public void LoadGame()
    {
        if (_isRunning) return;
        Interlocked.Exchange(ref isNextGenCalculated, 0);
        CellManager.LoadGame();
    }
    public void ChangeSpeed(int speed)
    {
        genPerSec = speed;
    }







}
