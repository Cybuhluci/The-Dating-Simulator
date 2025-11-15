using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Luci;

public class QTEManager : MonoBehaviour
{
    public MinaAttributes attributes = MinaAttributes.Instance;
    public static QTEManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject QTEPopup;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Image QTETimerBar;

    [Header("Difficulty Settings")]
    [Range(1, 10)] public int difficultyLevel = 3;
    public float baseTimeLimit = 5f; // Total QTE duration at difficulty 1

    [Tooltip("Allow multiple QTE sets even at lower difficulties")]
    public bool allowMultipleSetsAtLowDifficulties = false;

    [Tooltip("Maximum QTE sets to run")]
    public int maxQTESets = 3;

    [Header("QTE Action Presets")]
    public List<QTEInput> qtePresets = new List<QTEInput>();

    [Header("Legacy Glyph Settings")]
    public bool useOGXboxGlyphs = false;

    [SerializeField] private HashSet<string> bannedInputs = new HashSet<string>
    {
        "Look", "Move", "Zoom", "Roll", "Trick", "Start", "Select"
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        attributes = MinaAttributes.Instance;
    }

     void Start()
     {
        attributes = MinaAttributes.Instance;
     }

    public IEnumerator RunQTESequence(System.Action<bool> onComplete)
    {
        QTEPopup.SetActive(true);
        promptText.text = "";

        int requiredSets = Mathf.Clamp(difficultyLevel / 4, 1, maxQTESets);
        if (allowMultipleSetsAtLowDifficulties)
            requiredSets = Mathf.Clamp(difficultyLevel, 1, maxQTESets);

        for (int setIndex = 0; setIndex < requiredSets; setIndex++)
        {
            bool setSuccess = false;

            int sequenceLength = Mathf.Clamp(difficultyLevel + 1, 2, 7);
            float timePerAction = Mathf.Lerp(baseTimeLimit, baseTimeLimit * 0.3f, (difficultyLevel - 1) / 9f);
            float totalTime = sequenceLength * timePerAction;

            var sequence = GenerateRandomSequence(sequenceLength);
            List<QTEInput> remaining = new List<QTEInput>(sequence);
            float timer = 0f;

            promptText.text = FormatQTEPrompt(sequence);
            QTETimerBar.fillAmount = 1f;

            while (timer < totalTime)
            {
                QTETimerBar.fillAmount = 1f - (timer / totalTime);

                if (remaining.Count > 0)
                {
                    QTEInput current = remaining[0];
                    string currentAction = current.actionName;

                    bool inputMatched = false;

                    if (currentAction == "SidestepLeft" || currentAction == "SidestepRight")
                    {
                        float value = playerInput.actions["Sidestep"].ReadValue<float>();
                        if (currentAction == "SidestepLeft" && value < -0.5f)
                            inputMatched = true;
                        else if (currentAction == "SidestepRight" && value > 0.5f)
                            inputMatched = true;
                    }
                    else if (playerInput.actions[currentAction].WasPerformedThisFrame())
                    {
                        inputMatched = true;
                    }

                    if (inputMatched)
                    {
                        remaining.RemoveAt(0);
                        //SonicScoreSystem.Instance.GainScore(50); // Score per correct input
                        promptText.text = FormatQTEPrompt(remaining);
                    }
                    else if (difficultyLevel > 1)
                    {
                        foreach (var action in playerInput.actions)
                        {
                            if (action.name != currentAction &&
                                action.WasPerformedThisFrame() &&
                                !bannedInputs.Contains(action.name))
                            {
                                attributes.SetSpecialTimerEvent(false, "QTE");
                                promptText.text = "FAIL!";
                                QTETimerBar.fillAmount = 0f;
                                QTEPopup.SetActive(false);
                                onComplete?.Invoke(false);
                                yield break;
                            }
                        }
                    }
                }
                else
                {
                    setSuccess = true;
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            QTETimerBar.fillAmount = 0f;

            if (!setSuccess)
            {
                attributes.SetSpecialTimerEvent(false, "QTE");
                promptText.text = "FAIL!";
                QTEPopup.SetActive(false);
                onComplete?.Invoke(false);
                yield break;
            }

            //SonicScoreSystem.Instance.GainScore(150); // Bonus for completing a full QTE set
        }

        attributes.SetSpecialTimerEvent(false, "QTE");
        promptText.text = "SUCCESS!";
        //SonicScoreSystem.Instance.GainScore(300); // Bonus for completing entire sequence
        QTEPopup.SetActive(false);
        onComplete?.Invoke(true);
    }

    public IEnumerator RunButtonMashingQTE(string actionName, System.Action<bool> onComplete)
    {
        QTEPopup.SetActive(true);
        QTEInput matchedQTE = qtePresets.Find(qte => qte.actionName == actionName);
        if (matchedQTE == null)
        {
            Debug.LogWarning($"QTEInput for action '{actionName}' not found in presets.");
            promptText.text = "???";
            onComplete?.Invoke(false);
            yield break;
        }

        string glyph = GetGlyphForCurrentScheme(matchedQTE);

        int targetMashCount = Mathf.RoundToInt(Mathf.Lerp(15, 50, difficultyLevel / 10f));
        float mashTime = targetMashCount * 0.3f;

        QTETimerBar.fillAmount = 1f;

        int mashCount = 0;
        promptText.text = $"{glyph} x{targetMashCount}";

        float timer = 0f;
        while (timer < mashTime)
        {
            if (playerInput.actions[actionName].WasPerformedThisFrame())
            {
                mashCount++;
                //SonicScoreSystem.Instance.GainScore(5);
                int remaining = Mathf.Max(targetMashCount - mashCount, 0);
                promptText.text = $"{glyph} x{remaining}";
            }

            if (mashCount >= targetMashCount)
            {
                break;
            }

            QTETimerBar.fillAmount = 1f - (timer / mashTime);
            timer += Time.deltaTime;
            yield return null;
        }

        QTETimerBar.fillAmount = 0f;

        if (mashCount >= targetMashCount)
        {
            attributes.SetSpecialTimerEvent(false, "QTE");
            promptText.text = "MASH SUCCESS!";
            //SonicScoreSystem.Instance.GainScore(300);
            QTEPopup.SetActive(false);
            onComplete?.Invoke(true);
        }
        else
        {
            attributes.SetSpecialTimerEvent(false, "QTE");
            promptText.text = "MASH FAIL!";
            QTEPopup.SetActive(false);
            onComplete?.Invoke(false);
        }
    }

    public IEnumerator RunSingleQTE(string actionName, System.Action<bool> onComplete)
    {
        QTEInput matchedQTE;

        if (actionName == "Random")
        {
            var validPresets = qtePresets.FindAll(q =>
                q.actionName != "SidestepLeft" && q.actionName != "SidestepRight");

            if (validPresets.Count == 0)
            {
                Debug.LogWarning("No valid QTE presets available after exclusions!");
                promptText.text = "???";
                onComplete?.Invoke(false);
                yield break;
            }

            matchedQTE = validPresets[Random.Range(0, validPresets.Count)];
        }
        else
        {
            matchedQTE = qtePresets.Find(qte => qte.actionName == actionName);
            if (matchedQTE == null)
            {
                Debug.LogWarning($"QTEInput for action '{actionName}' not found in presets.");
                promptText.text = "???";
                onComplete?.Invoke(false);
                QTEPopup.SetActive(false);
                yield break;
            }
        }

        QTEPopup.SetActive(true);
        promptText.text = "";

        string glyph = GetGlyphForCurrentScheme(matchedQTE);
        float timeLimit = Mathf.Lerp(baseTimeLimit, baseTimeLimit * 0.3f, (difficultyLevel - 1) / 9f);

        promptText.text = glyph;
        QTETimerBar.fillAmount = 1f;

        float timer = 0f;
        bool success = false;

        while (timer < timeLimit)
        {
            if (playerInput.actions[matchedQTE.actionName].WasPerformedThisFrame())
            {
                success = true;
                break;
            }

            QTETimerBar.fillAmount = 1f - (timer / timeLimit);
            timer += Time.deltaTime;
            yield return null;
        }

        QTETimerBar.fillAmount = 0f;

        if (success)
        {
            attributes.SetSpecialTimerEvent(false, "QTE");
            promptText.text = "SUCCESS!";
            //SonicScoreSystem.Instance.GainScore(200); // Adjust score as you like
            QTEPopup.SetActive(false);
            onComplete?.Invoke(true);
        }
        else
        {
            attributes.SetSpecialTimerEvent(false, "QTE");
            promptText.text = "FAIL!";
            QTEPopup.SetActive(false);
            onComplete?.Invoke(false);
        }
    }


    private List<QTEInput> GenerateRandomSequence(int length)
    {
        var sequence = new List<QTEInput>();
        QTEInput previous = null;

        for (int i = 0; i < length; i++)
        {
            QTEInput preset;

            // Ensure no duplicates back-to-back
            do
            {
                preset = qtePresets[Random.Range(0, qtePresets.Count)];
            }
            while (previous != null && preset.actionName == previous.actionName);

            QTEInput newQTE = new QTEInput
            {
                actionName = preset.actionName,
                Xbox = preset.Xbox,
                PlayStation = preset.PlayStation,
                Keyboard = preset.Keyboard
            };

            sequence.Add(newQTE);
            previous = newQTE;
        }

        return sequence;
    }

    private string FormatQTEPrompt(List<QTEInput> sequence)
    {
        string output = "";

        foreach (var qte in sequence)
        {
            output += GetGlyphForCurrentScheme(qte) + " ";
        }

        return output.TrimEnd();
    }

    private string GetGlyphForCurrentScheme(QTEInput qte)
    {
        if (useOGXboxGlyphs && !string.IsNullOrEmpty(qte.OGXBOX))
            return qte.OGXBOX;

        string controlScheme = playerInput.currentControlScheme;

        if (controlScheme == "Keyboard&Mouse")
            return qte.Keyboard;

        if (controlScheme == "Gamepad")
        {
            var gamepad = Gamepad.current;
            if (gamepad is UnityEngine.InputSystem.XInput.XInputController)
                return qte.Xbox;
            if (gamepad is UnityEngine.InputSystem.DualShock.DualShockGamepad)
                return qte.PlayStation;
        }

        return qte.Keyboard; // Fallback
    }

    [System.Serializable]
    public class QTEInput
    {
        [Header("PlayerInput Name")]
        public string actionName;

        [Header("Button Glyphs")]
        public string Xbox;
        public string PlayStation;
        public string Keyboard;
        public string OGXBOX;
    }
}
