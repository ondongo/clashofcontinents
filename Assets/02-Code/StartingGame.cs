using UnityEngine;
using UnityEngine.SceneManagement;
public class StartingGame : MonoBehaviour
{
    public string sceneName;


    void OnClick()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
