using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DevelopersHub.ClashOfWhatecer;

public class StartingGame : MonoBehaviour
{
    public string sceneName;
    private Button btn;
    private GameObject textStartButton;   // <- ajout

    void Start()
    {
        if (SoundManager.instanse != null && SoundManager.instanse.mainMusic != null)
            SoundManager.instanse.PlayMusic(SoundManager.instanse.mainMusic);

        btn = GameObject.Find("StartButton").GetComponent<Button>();
        btn.onClick.AddListener(OnClick);

        // on récupère l'enfant qui contient le texte
        textStartButton = GameObject.Find("TextStartButton");
    }

    public void OnClick()
    {
        // On désactive complètement l'objet du bouton (plus visible, plus cliquable)
        if (btn != null)
            btn.gameObject.SetActive(false);

        if (SoundManager.instanse != null && SoundManager.instanse.buttonClickSound != null)
            SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);

        Loading.Open();
        StartCoroutine(LoadSceneCoroutine());
    }

    private System.Collections.IEnumerator LoadSceneCoroutine()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        // 1) On attend que la scène soit quasiment prête
        while (!async.isDone && async.progress < 0.9f)
            yield return null;

        // 2) On garde le loading affiché pendant un temps minimum
        float waitTime = 2f;  // durée du loading visible
        float t = 0f;
        while (t < waitTime)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // 3) On ferme le loading et on active la nouvelle scène
        Loading.Close();
        async.allowSceneActivation = true;
    }
}