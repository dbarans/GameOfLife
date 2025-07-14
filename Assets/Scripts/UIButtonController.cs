using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonController : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private Button startButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resetButton;

    [SerializeField] private Image startImage;

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
