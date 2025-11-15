using System.Collections;
using Unity.Cinemachine;
using Luci;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ResultsScreen : MonoBehaviour
{
    // Systems
    MinaAttributes attributes = MinaAttributes.Instance;

    [SerializeField] PlayerInput input;

    // UI + Camera
    public GameObject PlayerHUD;
    public GameObject ResultsUI;
    public GameObject ExpScreenUI;
    public CinemachineCamera ResultsCam;

    // Text Elements
    public TMP_Text RESULTSTime;
    public TMP_Text RESULTSScore_Rings;
    public TMP_Text RESULTSScore_Tricks;
    public TMP_Text RESULTSScore_Enemies;
    public TMP_Text RESULTSBonus;
    public TMP_Text RESULTSTotalScore;
    public TMP_Text RESULTSEXPBefore;
    public TMP_Text RESULTSEXPNewAdd;

    // Rank Display
    public GameObject[] RANKS;

    // Final Stats
    public int RANK;
    public float totalScore;

    // Bonuses
    public float timeBonus, ringBonus, trickScore, enemyBonus;
    public float ringPercentBonus, deathPenalty, bonusScore;

    // EXP
    public int expGained;
    private int startingExp;
    private float expAnimDuration = 1.5f; // seconds
    private Coroutine expAnimCoroutine;

    // Internal screen state
    private bool showingExpScreen = false;
    private bool readyForInput = false;

    void Start()
    {
        attributes = MinaAttributes.Instance;
    }

    public void StartResults()
    {
        ResultsCam.Priority = 20;
        PlayerHUD.SetActive(false);
        ResultsUI.SetActive(true);
        ExpScreenUI.SetActive(false);
        CalculateRank();
        StartCoroutine(RevealResults());
    }

    IEnumerator RevealResults()
    {
        yield return new WaitForSeconds(1f);
        RESULTSTime.text = attributes.time.ToString("F2") + "s";

        yield return new WaitForSeconds(0.5f);
        RESULTSScore_Rings.text = ringBonus.ToString("F0");

        yield return new WaitForSeconds(0.5f);
        RESULTSScore_Tricks.text = trickScore.ToString("F0");

        yield return new WaitForSeconds(0.5f);
        RESULTSScore_Enemies.text = enemyBonus.ToString("F0");

        yield return new WaitForSeconds(0.5f);
        RESULTSBonus.text = bonusScore.ToString("F0");

        yield return new WaitForSeconds(0.5f);
        RESULTSTotalScore.text = totalScore.ToString("F0");

        yield return new WaitForSeconds(0.5f);
        RANKS[RANK].SetActive(true);

        yield return new WaitForSeconds(0.5f);
        readyForInput = true; // Enable button input after everything is shown
    }

    void Update()
    {
        if (readyForInput && input.actions["Jump"].WasPressedThisFrame())
        {
            if (!showingExpScreen)
            {
                ShowExpScreen();
            }
            else
            {
                ReturnToEntranceStage();
            }
        }
    }

    void ShowExpScreen()
    {
        ResultsUI.SetActive(false);
        ExpScreenUI.SetActive(true);
        GrantExpFromScore();
        showingExpScreen = true;
    }

    void GrantExpFromScore()
    {
        expGained = Mathf.FloorToInt(totalScore / 100f);
        startingExp = PlayerPrefs.GetInt("PlayerEXP");

        // Start the EXP animation
        if (expAnimCoroutine != null) StopCoroutine(expAnimCoroutine);
        expAnimCoroutine = StartCoroutine(AnimateExpGain(startingExp, startingExp + expGained));
    }

    IEnumerator AnimateExpGain(int fromExp, int toExp)
    {
        float timer = 0f;
        float currentAdd = expGained;
        PlayerPrefs.SetInt("PlayerEXP", toExp); // Update storage, but show it gradually

        Color originalColor = RESULTSEXPNewAdd.color;

        while (timer < expAnimDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / expAnimDuration);

            int displayedStoredExp = Mathf.FloorToInt(Mathf.Lerp(fromExp, toExp, progress));
            int displayedAddedExp = Mathf.CeilToInt(Mathf.Lerp(expGained, 0, progress));

            RESULTSEXPBefore.text = "EXP: " + displayedStoredExp;
            RESULTSEXPNewAdd.text = "+" + displayedAddedExp;

            yield return null;
        }

        // Final values
        RESULTSEXPBefore.text = "EXP: " + toExp;
        RESULTSEXPNewAdd.text = "";

        // Optional fade out if you want:
        yield return new WaitForSeconds(0.2f);
        for (float f = 1f; f > 0f; f -= Time.deltaTime * 2f)
        {
            RESULTSEXPNewAdd.color = new Color(originalColor.r, originalColor.g, originalColor.b, f);
            yield return null;
        }

        RESULTSEXPNewAdd.gameObject.SetActive(false); // Hide after fading
    }

    void ReturnToEntranceStage()
    {
        attributes.InResults = false;
        LoadMiscScene("mainmenu");
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

    public int CalculateRank()
    {
        timeBonus = Mathf.Clamp(600 - attributes.time, 0, 600) * 5f;

        bonusScore = ringPercentBonus + deathPenalty;
        totalScore = timeBonus + ringBonus + trickScore + enemyBonus + bonusScore;

        if (totalScore >= 10000 && !attributes.HasDiedOnceInLevel) return RANK = 0; // S
        else if (totalScore >= 8000) return RANK = 1; // A
        else if (totalScore >= 6000) return RANK = 2; // B
        else if (totalScore >= 4000) return RANK = 3; // C
        else if (totalScore >= 2000) return RANK = 4; // D
        else return RANK = 5; // E
    }
}
