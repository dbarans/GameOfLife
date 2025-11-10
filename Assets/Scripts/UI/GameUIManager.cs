using UnityEngine;
using UnityEngine.UI;

public class GameUIManager
{
    private readonly GameObject buttonPanel;
    private readonly Image buttonPanelImage;
    private readonly Camera mainCamera;
    private readonly UIButtonController uiButtonController;

    private readonly Color pauseBackgroundColor;
    private readonly Color runningBackgroundColor = Color.black;

    public GameUIManager(GameObject buttonPanel, Camera mainCamera, UIButtonController uiButtonController)
    {
        this.buttonPanel = buttonPanel;
        this.mainCamera = mainCamera;
        this.uiButtonController = uiButtonController;

        buttonPanelImage = buttonPanel.GetComponent<Image>();
        pauseBackgroundColor = mainCamera.backgroundColor;
    }

    public void SetPauseUI()
    {
        mainCamera.backgroundColor = pauseBackgroundColor;
        buttonPanelImage.color = pauseBackgroundColor;
        uiButtonController.UpdateButtonsInteractivity(false);
    }

    public void SetRunningUI()
    {
        mainCamera.backgroundColor = runningBackgroundColor;
        buttonPanelImage.color = runningBackgroundColor;
        uiButtonController.UpdateButtonsInteractivity(true);
    }

    public void UpdateButtonsInteractivity(bool isGameRunning)
    {
        uiButtonController.UpdateButtonsInteractivity(isGameRunning);
    }
}
