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

    // === Runtime state ===
    private bool _isRunning = false;
    private float nextGenerationDisplayTime = 0f;
    private float generationIntervalSec => genPerSec <= 0 ? 1f : (1f / genPerSec);
    private Thread calculationThread = null;
    private bool shouldStopCalculationThread = false;
    private readonly object calculationThreadLock = new object();

    // === Debug ===
    private int generationsCount = 0;
    private int calculatedGenerationsCount = 0;


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
        generationManager = new GenerationManager(CellManager, null, nextGenLock);
        gameUIManager = new GameUIManager(buttonPanel, mainCamera, uiButtonController);

        touchHandler.SetVibrationManager(vibrationManager);
        TextAnimator.Initialize(vibrationManager);

        uiButtonController.OnStartButtonClicked += HandleStartPauseToggle;
        uiButtonController.OnPauseButtonClicked += PauseGame;
        uiButtonController.OnResetButtonClicked += ResetGame;
        uiButtonController.OnSaveButtonClicked += SaveGame;
        uiButtonController.OnLoadButtonClicked += LoadGame;
        uiButtonController.OnSpeedChanged += ChangeSpeed;
        uiButtonController.OnPatternBookButtonClicked += OpenPatternBook;
        uiButtonController.OnSlideInButtonClicked += SlideOutButtonsPanel;
        
        // Subscribe to pattern selection
        if (uiButtonController.patternListManager != null)
        {
            uiButtonController.patternListManager.OnPatternSelected += LoadPattern;
        }

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
        float now = Time.unscaledTime;
        int safety = 8;
        while (now >= nextGenerationDisplayTime && safety-- > 0)
        {
            int desiredGenNumber = generationsCount + 1;
            if (CellManager.TryDisplayGeneration(desiredGenNumber))
            {
                IncrementGeneration();
                nextGenerationDisplayTime += generationIntervalSec;

                if (vibrateEveryGen && genPerSec <= 1f)
                {
                    vibrationManager.VibrateOnGeneration();
                }
            }
            else
            {
                break;
            }
        }
        if (gameStatsDisplay != null)
        {
            int calculatedCount = Interlocked.Exchange(ref calculatedGenerationsCount, 0);
            if (calculatedCount > 0)
            {
                gameStatsDisplay.AddCalculatedGenerations(calculatedCount);
            }
            gameStatsDisplay.UpdateGenerationsPerSecondDisplay();
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
        vibrateEveryGen = true;
        tutorialManager.StartTutorial();
        touchHandler.IsTutorialOn = true;
        generationManager.AllowBirth = false;
        genPerSec = 1f;
    }
    private void EndTutorial()
    {
        touchHandler.IsTutorialOn = false;
        gameData.isTutorialOn = false;
    }
    public void RunGame()
    {
        if (CellManager.IsLivingCellsSetEmpty()) return;

        _isRunning = true;
        
        CellManager.ResetGenerationBuffer();
        generationsCount = 0;
        tutorialManager.GenerationCount = generationsCount;
        nextGenerationDisplayTime = Time.unscaledTime + generationIntervalSec;
        
        StartCalculationThread();
        
        gameUIManager.SetRunningUI();
        gameUIManager.UpdateButtonsInteractivity(isRunning);
        uiButtonController.SetStartButtonState(UIButtonController.StartButtonState.Pause);
        uiButtonController.UpdateRunState(_isRunning);
    }
    
    private void StartCalculationThread()
    {
        lock (calculationThreadLock)
        {
            if (calculationThread != null && calculationThread.IsAlive)
            {
                return; 
            }
            
            shouldStopCalculationThread = false;
            calculationThread = new Thread(CalculationThreadLoop)
            {
                IsBackground = true,
                Name = "GenerationCalculationThread"
            };
            calculationThread.Start();
        }
    }
    
    private void StopCalculationThread()
    {
        lock (calculationThreadLock)
        {
            shouldStopCalculationThread = true;
            if (calculationThread != null)
            {
                calculationThread.Join(1000); 
                calculationThread = null;
            }
        }
    }
    
    private void CalculationThreadLoop()
    {
        while (!shouldStopCalculationThread)
        {
            if (!_isRunning)
            {
                Thread.Sleep(10); 
                continue;
            }
            
            if (!CellManager.TryGetNextGenerationToCalculate(out int nextIndex, out int nextGenerationNumber))
            {
                Thread.Sleep(5);
                continue;
            }
            
            HashSet<Vector3Int> baseGen = CellManager.GetBaseGenerationForCalculation(nextIndex);
            
            HashSet<Vector3Int> newGeneration = generationManager.CalculateNextGeneration(baseGen);
            
            CellManager.SaveCalculatedGeneration(nextIndex, nextGenerationNumber, newGeneration);
            
            Interlocked.Increment(ref calculatedGenerationsCount);
            
        }
    }
    public void PauseGame()
    {
        _isRunning = false;
        StopCalculationThread();
        gameUIManager.SetPauseUI();
        uiButtonController.SetStartButtonState(UIButtonController.StartButtonState.Start);
        gameUIManager.UpdateButtonsInteractivity(isRunning);
        uiButtonController.UpdateRunState(_isRunning);
    }
    public void ResetGame()
    {
        PauseGame();
        CellManager.ClearGrid();
    }

    public void SaveGame()
    {
        if (_isRunning) return;
        CellManager.SaveGame();
    }
    public void LoadGame()
    {
        if (_isRunning) return;
        CellManager.LoadGame();
    }
    public void ChangeSpeed(int speed)
    {
        genPerSec = speed;
        if (_isRunning) nextGenerationDisplayTime = Time.unscaledTime + generationIntervalSec;
    }
    public void OpenPatternBook()
    {
        PauseGame();
        buttonPanelSlider.SlideToPatternsBook();
        uiButtonController.ShowPatternsBook();
    }
    public void SlideOutButtonsPanel()
    {
        buttonPanelSlider.SlideOut();
        if (buttonPanelSlider.GetSlideState() == ButtonPanelSlider.SlideState.Extended) return;
        uiButtonController.HidePatternsBook();
    }

    public void LoadPattern(PatternData patternData)
    {
        PauseGame();
        try
        {
            if (patternData != null)
            {
                PatternDataConverter converter = new PatternDataConverter();
                HashSet<Vector3Int> patternCells = converter.ConvertPattern(patternData);
                CellManager.SetPattern(patternCells);
                Debug.Log($"Loaded pattern: {patternData.Name} (ID: {patternData.Id}) with {patternCells.Count} cells");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading pattern: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        StopCalculationThread();
    }
}