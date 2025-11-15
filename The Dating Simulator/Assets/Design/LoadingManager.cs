using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    public GameObject stageLoadingPrefab;
    public GameObject miscLoadingPrefab;

    public Scene loadingscene;

    public GameObject currentLoadingScreen;

    public bool isStageLoading;

    public string NextStage;
    public string[] StageNameArray;

    public void StartLoading(string sceneToLoad, bool stageLoading)
    {
        isStageLoading = stageLoading;
        PlayerPrefs.SetString("NextScene", sceneToLoad);
    }
     
    public void Awake()
    {
        OnLoadingScreenLoaded();
        NextStage = PlayerPrefs.GetString("NextScene", NextStage);
        if (PlayerPrefs.GetInt("IsStageLoading") == 0)
        {
           StartLoading(NextStage, true);
        }
        else
        {
            StartLoading(NextStage, false);
        }

    }

    public void OnLoadingScreenLoaded()
    {
        // Read whether it's a stage or misc loading
        isStageLoading = PlayerPrefs.GetInt("IsStageLoading", 0) == 1; // Default to 0 (misc)

        if (isStageLoading)
        {
            currentLoadingScreen = Instantiate(stageLoadingPrefab);
            //PrefabUtility.InstantiatePrefab(stageLoadingPrefab);
            //Debug.LogError("trudinkles");
        }
        else if (!isStageLoading)
        {
            currentLoadingScreen = Instantiate(miscLoadingPrefab);
            //PrefabUtility.InstantiatePrefab(miscLoadingPrefab);
            //Debug.LogError("tunkinkles");
        }
        else
        {
            Debug.LogError("ERROR: Loading prefab is missing!");
        }
    }
}
