using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonController : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private Button startButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resetButton;

    public void StartGame()
    {
        gameController.RunGame();
    }
    public void PauseGame()
    {
        gameController.PauseGame();
    }
    public void ResetGame()
    {
        gameController.ResetGame();
    }
}
