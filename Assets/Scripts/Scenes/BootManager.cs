using UnityEngine;
using UnityEngine.SceneManagement;

public class BootSceneManager : MonoBehaviour
{
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
        SceneManager.LoadScene(SceneNames.Game);
    }
}