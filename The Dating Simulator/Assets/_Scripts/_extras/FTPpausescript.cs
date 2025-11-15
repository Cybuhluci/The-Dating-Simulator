using Luci;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FTPpausescript : MonoBehaviour
{
    MinaAttributes attributes;

    public bool inPauseScreen;

    public InputActionAsset UIasset;
    private InputAction UIgoback;
    private InputAction UIstart;
    private InputAction UImenuleft, UImenuright;

    public GameObject playerHUD;
    public GameObject pausemenumain, pausemenubackground;
    public GameObject pauseingame;
    public GameObject submenuScreen; // New Submenu screen
    public GameObject collectiblesScreen, affinityScreen, optionsScreen, statusScreen;

    // DevMode stage selection
    public GameObject stageselect;
    public GameObject[] stageScreens;
    private int currentStageIndex = 0;
    private bool devModeEnabled = false;

    void Start()
    {
        attributes = MinaAttributes.Instance;

        UIgoback = UIasset.FindActionMap("UI").FindAction("Cancel");
        UIstart = UIasset.FindActionMap("UI").FindAction("Start");
        UImenuleft = UIasset.FindActionMap("UI").FindAction("Click");
        UImenuright = UIasset.FindActionMap("UI").FindAction("RightClick");

        UIgoback.Enable();
        UIstart.Enable();
        UImenuleft.Enable();
        UImenuright.Enable();

        Cursor.lockState = CursorLockMode.Confined;

        //devModeEnabled = PlayerPrefs.GetInt("DevMode", 0) == 1;
    }

    void Update()
    {
        if (UIstart.WasPerformedThisFrame())
        {
            TogglePause();
        }

        if (UIgoback.WasPerformedThisFrame())
        {
            HandleBack();
        }
    }

    void TogglePause()
    {
        if (attributes.InResults) return;

        bool isGamePaused = pauseingame.activeSelf;

        submenuScreen.SetActive(false);
        collectiblesScreen.SetActive(false);
        affinityScreen.SetActive(false);
        optionsScreen.SetActive(false);
        stageselect.SetActive(false);

        inPauseScreen = isGamePaused;

        if (inPauseScreen == true)
        {
            Time.timeScale = 0f;
            MinaAttributes.Instance.PlayerDisabled = true;
            MinaAttributes.Instance.CameraDisabled = true;
            pauseingame.SetActive(false);
            playerHUD.SetActive(false);
            pausemenubackground.SetActive(true);
            pausemenumain.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            MinaAttributes.Instance.PlayerDisabled = false;
            MinaAttributes.Instance.CameraDisabled = false;
            pausemenubackground.SetActive(false);
            pausemenumain.SetActive(false);
            pauseingame.SetActive(true);
            playerHUD.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void HandleBack()
    {
        if (collectiblesScreen.activeSelf || affinityScreen.activeSelf || optionsScreen.activeSelf)
        {
            // If in a sub-screen, return to the submenu
            CloseToSubmenu();
        }
        else if (statusScreen.activeSelf)
        {
            closeStatus();
        }
        else if (submenuScreen.activeSelf)
        {
            // If already in the submenu, go back to the main pause menu
            CloseSubmenu();
        }
        else if (stageselect.activeSelf)
        {
            stageselect.SetActive(false);
            pausemenumain.SetActive(true);
        }
        else if (pausemenumain.activeSelf)
        {
            TogglePause();
        }
    }

    void CloseToSubmenu()
    {
        collectiblesScreen.SetActive(false);
        affinityScreen.SetActive(false);
        optionsScreen.SetActive(false);
        submenuScreen.SetActive(true);
    }

    public void continueButton()
    {
        TogglePause();
    }

    public void returnToMainMenu()
    {
        Time.timeScale = 1f;
        LoadMiscScene("mainmenu");
    }

    public void openSubmenu()
    {
        pausemenumain.SetActive(false);
        submenuScreen.SetActive(true);
    }

    public void CloseSubmenu()
    {
        submenuScreen.SetActive(false);
        collectiblesScreen.SetActive(false);
        affinityScreen.SetActive(false);
        optionsScreen.SetActive(false);
        pausemenumain.SetActive(true);
    }

    public void openCollectibles()
    {
        submenuScreen.SetActive(false);
        collectiblesScreen.SetActive(true);
    }
    
    public void openStatus()
    {
        submenuScreen.SetActive(false);
        pausemenubackground.SetActive(false);
        statusScreen.SetActive(true);
    }

    public void closeStatus()
    {
        statusScreen.SetActive(false);
        submenuScreen.SetActive(true);
        pausemenubackground.SetActive(true);
    }

    public void openAffinity()
    {
        submenuScreen.SetActive(false);
        affinityScreen.SetActive(true);
    }

    public void openOptions()
    {
        submenuScreen.SetActive(false);
        optionsScreen.SetActive(true);
    }

    public void openStageSelect()
    {
        if (devModeEnabled)
        {
            pausemenumain.SetActive(false);
            stageselect.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Stage Select is DevMode only!");
        }
    }

    public void ChangeStageScreen(int direction)
    {
        if (!stageselect.activeSelf) return;

        stageScreens[currentStageIndex].SetActive(false);
        currentStageIndex = (currentStageIndex + direction + stageScreens.Length) % stageScreens.Length;
        stageScreens[currentStageIndex].SetActive(true);
    }

    public void LoadMiscScene(string sceneName)
    {
        Time.timeScale = 1f;
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
}
