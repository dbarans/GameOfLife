using UnityEngine;
using UnityEngine.SceneManagement;

public class BootSceneManager : MonoBehaviour
{
    private const string TUTORIAL_COMPLETED_KEY = "HasCompletedTutorial";
    private const string TUTORIAL_SCENE_NAME = "Tutorial";
    private const string MAIN_MENU_SCENE_NAME = "Game"; //temporary, change to "MainMenu" when implemented

    void Start()
    {
        bool hasCompletedTutorial = PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1;

        if (hasCompletedTutorial)
        {
            Debug.Log("Tutorial completed. Loading main menu scene.");
            SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
        }
        else
        {
            Debug.Log("Tutorial not completed. Loading tutorial scene.");
            SceneManager.LoadScene(TUTORIAL_SCENE_NAME);
        }
    }

    public void ResetTutorialCompletionState()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_COMPLETED_KEY);
        PlayerPrefs.Save();
        Debug.Log("Tutorial completion state reset.");
    }
}