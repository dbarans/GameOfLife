using UnityEngine;
using UnityEngine.SceneManagement;

public class BootSceneManager : MonoBehaviour
{
    private const string GAME_SCENE_NAME = "Game"; 
    private const string MAIN_MENU_SCENE_NAME = "MainMenu";
    private const string WELCOME_SCENE_NAME = "Welcome";

    void Start()
    {
        if (PlayerPrefsManager.IsFirstTime)
        {
            PlayerPrefsManager.IsFirstTime = false;
            PlayerPrefs.Save();
            Debug.Log("First time player detected. Loading tutorial scene.");
            SceneManager.LoadScene(WELCOME_SCENE_NAME);
            return;
        }
        else
        {
            SceneManager.LoadScene(GAME_SCENE_NAME);
        }
    }
}