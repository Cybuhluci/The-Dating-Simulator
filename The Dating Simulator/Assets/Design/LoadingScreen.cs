using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{ 
    public string StageName1;
    public string StageName2;
    public TMP_Text StageName1txt;
    public TMP_Text StageName2txt;

    public GameObject spinnyLoadingThing; // The spinning object
    public float lengthOfLoading = 3f;
    public float spinSpeed = 200f; // Rotation speed in degrees per second

    private string levelToLoad;
    private string previousScene;
    public bool over;
    public int endCount;
    public bool stageLoaded;

    public Animator Anim;
    public Camera Cam;

    void Start()
    {
        if (PlayerPrefs.GetInt("IsStageLoading") == 1)
        {
            Anim.SetBool("loaded", false);
        }

        StageName1txt.text = StageName1;
        StageName2txt.text = StageName2;

        levelToLoad = PlayerPrefs.GetString("NextScene", "DefaultScene");
        previousScene = PlayerPrefs.GetString("PreviousScene", "DefaultScene");

        if (levelToLoad == "DefaultScene")
        {
            Debug.LogError("ERROR: No scene name found! Did you set 'NextScene' before loading?");
            return;
        }

        Debug.Log($"Loading screen active. Loading: {levelToLoad}, Unloading: {previousScene}");
        StartCoroutine(LoadLevelAsync());
    }

    private IEnumerator LoadLevelAsync()
    {
        SceneManager.UnloadSceneAsync(previousScene);

        yield return new WaitForSeconds(lengthOfLoading); // Simulate loading delay

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelToLoad, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f) // Wait until almost ready
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
        stageLoaded = true;
    }

    void Update()
    {
        if (spinnyLoadingThing != null)
        {
            spinnyLoadingThing.transform.Rotate(Vector3.forward, -spinSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (stageLoaded)
        {
            over = true;
        }

        if (over)
        {
            endCount++;

            if (endCount == 10)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(levelToLoad));
            }
            else if (endCount == 30)
            {
                if (PlayerPrefs.GetInt("IsStageLoading") == 1)
                {
                    Anim.SetBool("loaded", true);
                }
            }
            else if (endCount > 60)
            {

                Debug.Log("CLOSED LOADING SCREEN");
                SceneManager.UnloadSceneAsync("loadingScene");
            }
        }
    }
}
