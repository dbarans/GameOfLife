using UnityEngine;
using UnityEngine.SceneManagement;

public class BootSceneManager : MonoBehaviour
{
    private const string TUTORIAL_COMPLETED_KEY = "HasCompletedTutorial";
    private const string FIRST_TIME_KEY = "FirstTime";
    private const string TUTORIAL_SCENE_NAME = "Tutorial";
    private const string GAME_SCENE_NAME = "Game"; 
    private const string MAIN_MENU_SCENE_NAME = "MainMenu";
    private const string WELCOME_SCENE_NAME = "Welcome";

    void Start()
    {
        bool hasCompletedTutorial = PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1;
        bool firstTime = PlayerPrefs.GetInt(FIRST_TIME_KEY, 1) == 1;

        if (firstTime)
        {
            PlayerPrefs.SetInt(FIRST_TIME_KEY, 0);
            PlayerPrefs.Save();
            Debug.Log("First time player detected. Loading tutorial scene.");
            SceneManager.LoadScene(WELCOME_SCENE_NAME);
            return;
        }
        else
        {
            SceneManager.LoadScene(GAME_SCENE_NAME);
        }

        /*if (hasCompletedTutorial)
        {
            Debug.Log("Tutorial completed. Loading main menu scene.");
            SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
        }
        else
        {
            Debug.Log("Tutorial not completed. Loading tutorial scene.");
            SceneManager.LoadScene(TUTORIAL_SCENE_NAME);
        }*/
    }

    public void ResetTutorialCompletionState()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_COMPLETED_KEY);
        PlayerPrefs.Save();
        Debug.Log("Tutorial completion state reset.");
    }
}