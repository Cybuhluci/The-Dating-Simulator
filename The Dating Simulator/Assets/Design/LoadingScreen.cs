using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour // THIS SCRIPT MIGHT BE REMOVED IN FUTURE, STAGES ARE NOT PART OF THE GAME ANYMORE
{ 
    public string StageName1 = "Placeholder";
    public string StageName2 = "Placeholder";
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

        string nextScene = PlayerPrefs.GetString("NextScene");

        levelToLoad = PlayerPrefs.GetString("NextScene", "DefaultScene");
        previousScene = PlayerPrefs.GetString("PreviousScene", "DefaultScene");

        // Split the scene name into two display strings and assign to the UI texts
        if (!string.IsNullOrEmpty(levelToLoad))
        {
            var names = SplitSceneName(levelToLoad);
            StageName1 = names.Item1;
            StageName2 = names.Item2;

            if (StageName1txt != null) StageName1txt.text = StageName1;
            if (StageName2txt != null) StageName2txt.text = StageName2;
        }
        else
        {
            if (StageName1txt != null) StageName1txt.text = StageName1;
            if (StageName2txt != null) StageName2txt.text = StageName2;
        }

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

    private (string, string) SplitSceneName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return ("", "");

        var name = sceneName;

        var camelMatch = Regex.Match(name.Substring(1), "[A-Z]");
        if (camelMatch.Success)
        {
            int pos = camelMatch.Index + 1;
            var a = name.Substring(0, pos);
            var b = name.Substring(pos);
            return (ToDisplay(a), ToDisplay(b));
        }

        int mid = Mathf.Clamp(name.Length / 2, 1, name.Length - 1);
        var left = name.Substring(0, mid);
        var right = name.Substring(mid);
        return (ToDisplay(left), ToDisplay(right));
    }

    private string ToDisplay(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        var s = Regex.Replace(input, @"[_\-.]+", " ");
        s = Regex.Replace(s, "(?<=[a-z0-9])(?=[A-Z])", " ");
        return s.Trim();
    }
}
