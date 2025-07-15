using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonController : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private Button startButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button speedButton;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private GameObject speedSliderGroup;
    private List<Button> buttons= new List<Button>();

    private void Awake()
    {
        buttons.Add(startButton);
        buttons.Add(pauseButton);
        buttons.Add(resetButton);
        buttons.Add(speedButton);
    }
    public void StartGame()
    {
        gameController.RunGame();
        if (gameController.isRunning)
        {
            startButton.gameObject.SetActive(false);
            pauseButton.gameObject.SetActive(true);
        }

    }
    public void PauseGame()
    {
        gameController.PauseGame();
        startButton.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(false);
    }
    public void ChangeSpeed()
    {
        gameController.ChangeSpeed((int)speedSlider.value);
    }
    public void ResetGame()
    {
        gameController.ResetGame();
        startButton.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(false);
    }

    public void ShowButtons()
    {
        foreach (Button button in buttons)
        {
            if (button == startButton && gameController.isRunning) continue; 
            if (button == pauseButton && !gameController.isRunning) continue;
            button.gameObject.SetActive(true);
        }
    }
    public void HideButtons()
    {
       foreach (Button button in buttons)
        {
            button.gameObject.SetActive(false);
        }
    }
    public void ShowSpeedSlider()
    {
        HideButtons();
        speedSliderGroup.SetActive(true);
    }
    public void HideSpeedSlider()
    {
        speedSliderGroup.SetActive(false);
        ShowButtons();
    }
}
