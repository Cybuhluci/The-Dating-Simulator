using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class mainmenu : MonoBehaviour
{
    public InputActionAsset UIasset;
    private InputAction UIgoback;
    private InputAction UItitleanykey;
    public GameObject wholeofthemenu;

    public GameObject stageselect, mainmenuscreen, optionsscreen;
    public GameObject titlescreen;

    // Stage Selection Screens
    public GameObject[] stageScreens;
    private int currentStageIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        UIgoback = UIasset.FindActionMap("UI").FindAction("Cancel");
        UItitleanykey = UIasset.FindActionMap("UI").FindAction("Start");

        UIgoback.Enable();
        UItitleanykey.Enable();

        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        if (UIgoback.WasPressedThisFrame())
        {
            if (mainmenuscreen.activeSelf)
            {
                optionsscreen.SetActive(false);
                stageselect.SetActive(false);
                mainmenuscreen.SetActive(true);
                wholeofthemenu.SetActive(false);
                titlescreen.SetActive(true);
            }
            else if (stageselect.activeSelf)
            {
                stageselect.SetActive(false);
                mainmenuscreen.SetActive(true);
            }
            else if (optionsscreen.activeSelf)
            {
                optionsscreen.SetActive(false);
                mainmenuscreen.SetActive(true);
            }
        }
        
        if (UItitleanykey.WasPressedThisFrame())
        {
            if (titlescreen.activeSelf)
            {
                titlescreen.SetActive(false);
                wholeofthemenu.SetActive(true);
            }
        }

        if (stageselect.activeSelf)
        {
            if (UIasset.FindActionMap("UI").FindAction("StageLeft").WasPressedThisFrame())
            {
                ChangeStageScreen(-1);
            }
            else if (UIasset.FindActionMap("UI").FindAction("StageRight").WasPressedThisFrame())
            {
                ChangeStageScreen(1);
            }
        }
    }

    void ChangeStageScreen(int direction)
    {
        stageScreens[currentStageIndex].SetActive(false);
        currentStageIndex = (currentStageIndex + direction + stageScreens.Length) % stageScreens.Length;
        stageScreens[currentStageIndex].SetActive(true);
    }

    public void playgame(string SceneName)
    {
        LoadStage(SceneName);
    }

    public void LoadStage(string stageName)
    {
        if (string.IsNullOrEmpty(stageName))
        {
            Debug.LogError("ERROR: Scene name is empty or null!");
            return;
        }

        // Save scene names
        PlayerPrefs.SetString("NextScene", stageName);
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("IsStageLoading", 1); // 1 = Stage
        PlayerPrefs.Save(); // Ensure data is written

        Debug.Log($"Loading screen opened. Next Scene: {stageName}, Previous Scene: {SceneManager.GetActiveScene().name}");

        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }

    public void LoadMiscScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ERROR: Scene name is empty or null!");
            return;
        }

        // Save scene names
        PlayerPrefs.SetString("NextScene", sceneName);
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("IsStageLoading", 0); // 0 = Misc
        PlayerPrefs.Save(); // Ensure data is written

        Debug.Log($"Loading screen opened. Next Scene: {sceneName}, Previous Scene: {SceneManager.GetActiveScene().name}");

        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }

    public void openstageselect()
    {
        mainmenuscreen.SetActive(false);
        stageselect.SetActive(true);
    }

    public void openoptionsscreen()
    {
        mainmenuscreen.SetActive(false);
        optionsscreen.SetActive(true);
    }

    public void closeoptionsscreen()
    {
        optionsscreen.SetActive(false);
        mainmenuscreen.SetActive(true);
    }

    public void quittomenu()
    {
        SceneManager.LoadScene("mainmenu");
    }

    public void quittodesktop()
    {
        Application.Quit();
#if unityeditor
        EditorApplication.isPlaying = false; // Only works in editor
#endif
    }
}
