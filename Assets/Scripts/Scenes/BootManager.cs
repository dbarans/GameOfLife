using UnityEngine;
using UnityEngine.SceneManagement;

public class BootSceneManager : MonoBehaviour
{
    private const string GAME_SCENE_NAME = "Game"; 
    private const string MAIN_MENU_SCENE_NAME = "MainMenu";
    private const string WELCOME_SCENE_NAME = "Welcome";

    [SerializeField] private GameData gameData;

    void Start()
    {
        if (PlayerPrefsManager.IsFirstTime)
        {
            PlayerPrefsManager.IsFirstTime = false;
            PlayerPrefs.Save();
            gameData.isTutorialOn = true;
        }
        else
        {
            gameData.isTutorialOn = false;
        }
        SceneManager.LoadScene(GAME_SCENE_NAME);
    }
}