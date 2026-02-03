using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoadName;

    public void LoadGameScene()
    {
        Debug.Log($"Loading scene: {sceneToLoadName}"); 
        SceneManager.LoadScene(sceneToLoadName);
    }
}