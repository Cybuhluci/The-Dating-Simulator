using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

public class QTESequence : MonoBehaviour
{
    [System.Serializable]
    public class QTEInput
    {
        public string actionName; // "Jump", "Dash", etc.
        public float timeLimit = 1f;
    }

    [SerializeField] bool useRandomSequence = true;
    [SerializeField] int randomSequenceLength = 3;

    [Header("QTE Sequence")]
    public List<QTEInput> sequence = new List<QTEInput>();

    [Header("Random Input Pool")]
    [SerializeField] List<string> possibleActions = new List<string> { "Jump", "Dash", "Trick" };

    [Header("References")]
    [SerializeField] PlayerInput playerInput;
    [SerializeField] TMP_Text promptText;

    [Header("Events")]
    public UnityEvent OnSuccess;
    public UnityEvent OnFailure;

    private bool inputReceived = false;
    private int currentIndex = 0;

    public bool IsRunning { get; private set; }
    public bool QTESucceeded { get; private set; } = false;

    public void StartQTE()
    {
        if (IsRunning) return;

        if (useRandomSequence)
            GenerateRandomSequence(randomSequenceLength);

        StartCoroutine(RunQTE());
    }

    private IEnumerator RunQTE()
    {
        IsRunning = true;
        QTESucceeded = false;
        currentIndex = 0;
        promptText.text = "";

        foreach (var qte in sequence)
        {
            promptText.text += qte.actionName + " ";
        }

        foreach (var qte in sequence)
        {
            float timer = 0f;
            inputReceived = false;

            while (timer < qte.timeLimit)
            {
                if (playerInput.actions[qte.actionName].WasPerformedThisFrame())
                {
                    inputReceived = true;
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (!inputReceived)
            {
                Debug.Log($"QTE Failed on {qte.actionName}");
                QTESucceeded = false;
                promptText.text = "FAIL!";
                OnFailure?.Invoke();
                IsRunning = false;
                yield break;
            }

            currentIndex++;
        }

        Debug.Log("QTE Success!");
        QTESucceeded = true;
        promptText.text = "SUCCESS!";
        OnSuccess?.Invoke();
        IsRunning = false;
    }

    public void GenerateRandomSequence(int count)
    {
        sequence.Clear();

        string previousAction = "";
        for (int i = 0; i < count; i++)
        {
            string randomAction;
            do
            {
                randomAction = possibleActions[Random.Range(0, possibleActions.Count)];
            }
            while (randomAction == previousAction); // Avoid same back-to-back

            previousAction = randomAction;
            sequence.Add(new QTEInput { actionName = randomAction, timeLimit = 1f });
        }
    }
}
