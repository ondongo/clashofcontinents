using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ClashOfContinents;

public class StartingGame : MonoBehaviour
{
    public string sceneName;
    private Button btn;
    private GameObject textStartButton;

    void Start()
    {
        if (SoundManager.instanse != null && SoundManager.instanse.mainMusic != null)
            SoundManager.instanse.PlayMusic(SoundManager.instanse.mainMusic);

        btn = GameObject.Find("StartButton").GetComponent<Button>();
        btn.onClick.AddListener(OnClick);

        textStartButton = GameObject.Find("TextStartButton");
    }

    public void OnClick()
    {
        if (btn != null)
            btn.gameObject.SetActive(false);

        if (SoundManager.instanse != null && SoundManager.instanse.buttonClickSound != null)
            SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);

        StartCoroutine(LoadSceneCoroutine());
    }

    private System.Collections.IEnumerator LoadSceneCoroutine()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (!async.isDone && async.progress < 0.9f)
            yield return null;

        float waitTime = 1f;
        float t = 0f;
        while (t < waitTime)
        {
            t += Time.deltaTime;
            yield return null;
        }
        async.allowSceneActivation = true;
    }
}