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
    public void ResetGame()
    {
        gameController.ResetGame();
        startButton.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(false);
    }
}
