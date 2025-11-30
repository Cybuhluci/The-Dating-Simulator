//using System.Collections;
//using Luci;
//using TMPro;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class SonicLifeSystem : MonoBehaviour
//{
//    public static SonicLifeSystem Instance;
//    MinaAttributes attributes = MinaAttributes.Instance;

//    [SerializeField] private GameObject respawnUI;
//    [SerializeField] private RespawnUI activeUI;
//    [SerializeField] private GameObject Player;
//    [SerializeField] private AudioSource deathsound;

//    [Header("Settings")]
//    public int startingLives = 3;
//    public float respawnDelay = 1.5f;
//    public Transform respawnPoint;
//    public int Lives;
//    public int LivesLostInStage;

//    void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);
//    }

//    void Start()
//    {
//        attributes = MinaAttributes.Instance;
//        Lives = startingLives;
//        attributes.lives = Lives;
//        attributes.HasDiedOnceInLevel = false;
//        LivesLostInStage = 0;
//    }

//    public void PlayerDied()
//    {
//        Lives--;

//        if (Lives >= 1)
//        {
//            deathsound.Play();
//            attributes.IsFallingOffStage = true;
//            StartCoroutine(HandleRespawn());
//        }
//        else
//        {
//            GameOver();
//        }
//    }

//    private IEnumerator HandleRespawn()
//    {
//        if (respawnUI != null)
//        {
//            activeUI.Enter(); // fade to black
//            yield return new WaitForSeconds(activeUI.FadeDuration);
//        }

//        yield return new WaitForSeconds(respawnDelay);

//        RespawnPlayer();

//        if (activeUI != null)
//        {
//            activeUI.Exit(); // fade back in
//        }
//    }

//    void RespawnPlayer()
//    {
//        //SonicScoreSystem.Instance.ResetScore();
//        attributes.HasDiedOnceInLevel = true;
//        LivesLostInStage++;

//        // Reset velocity, animations, etc.
//        Rigidbody rb = Player.GetComponent<Rigidbody>();
//        if (rb != null)
//            rb.MovePosition(respawnPoint.position);
//        rb.linearVelocity = Vector3.zero;

//        attributes.lives = Lives;
//        attributes.IsFallingOffStage = false;
//    }

//    [SerializeField] private Checkpoint currentCheckpoint;

//    public void UpdateCheckpoint(Checkpoint newCheckpoint)
//    {
//        if (currentCheckpoint != null)
//            currentCheckpoint.Deactivate();

//        currentCheckpoint = newCheckpoint;
//        currentCheckpoint.Activate();
//        respawnPoint = currentCheckpoint.GetRespawnPoint();
//    }

//    void GameOver()
//    {
//        // Reload the scene or go to a Game Over screen
//        LoadMiscScene(SceneManager.GetActiveScene().name);
//    }

//    public void LoadMiscScene(string sceneName)
//    {
//        Time.timeScale = 1f;
//        if (string.IsNullOrEmpty(sceneName))
//        {
//            Debug.LogError("ERROR: Scene name is empty or null!");
//            return;
//        }

//        // Save scene names
//        PlayerPrefs.SetString("NextScene", sceneName);
//        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
//        PlayerPrefs.SetInt("IsStageLoading", 0); // 0 = Misc
//        PlayerPrefs.Save(); // Ensure data is written

//        Debug.Log($"Loading screen opened. Next Scene: {sceneName}, Previous Scene: {SceneManager.GetActiveScene().name}");

//        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
//    }
//}
