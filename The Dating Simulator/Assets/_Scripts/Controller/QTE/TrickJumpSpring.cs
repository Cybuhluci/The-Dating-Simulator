// Adapted from TrickJumpPanel to work with springs
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Luci;
using static Spring2;

public class TrickJumpSpring : MonoBehaviour
{
    // Fixed references to MinaAttributes
    MinaAttributes attributes = MinaAttributes.Instance;

    public enum QTEType { Single, Mashing }
    public enum QTERandomness { Random, Preset }

    [Header("QTE Settings")]
    [Range(1, 10)] public int Difficulty = 3;
    public QTEType qteType = QTEType.Single;
    public string mashAction = "Jump";

    [Header("Spring Parameters")]
    public Vector3 failOffset = new Vector3(0, 3, 10);
    public float travelTime = 0.6f;
    public float failFallTime = 1.2f;
    public float finalBoostForce = 50f;

    [Header("Chained Springs & QTE Buttons")]
    public QTERandomness qteRandomness = QTERandomness.Random;
    public SubsequentSprings[] subsequentSprings; // This replaces chainedSprings array

    [Header("References")]
    [SerializeField] private AudioSource springSFX;
    [SerializeField] private PlayerInput playerInput;

    private bool qteSuccess = false;
    private bool inSpringJump = false;
    private Rigidbody playerRb;
    private Transform playerTransform;

    void Start()
    {
        attributes = MinaAttributes.Instance;

        if (subsequentSprings != null && subsequentSprings.Length > 1)
        {
            for (int i = 0; i < subsequentSprings.Length - 1; i++)
            {
                Transform current = subsequentSprings[i].Spring;
                Transform next = subsequentSprings[i + 1].Spring;
                Vector3 direction = (next.position - current.position).normalized;

                current.rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (inSpringJump) return;

        playerRb = other.GetComponent<Rigidbody>();
        playerTransform = other.transform;

        if (playerRb != null && playerTransform != null)
        {
            StartCoroutine(SpringRoutine());
        }
    }

    IEnumerator SpringRoutine()
    {
        inSpringJump = true;
        springSFX?.Play();

        attributes.SetSpecialTimerEvent(true, "QTE");
        playerRb.isKinematic = true;
        attributes.PlayerDisabled = true;

        // Move to first spring (always successful)
        yield return MoveStraight(playerTransform, transform.position, subsequentSprings[1].Spring.position, travelTime);

        int chainLength = subsequentSprings.Length;

        for (int i = 1; i < chainLength; i++) // Start from 1 to include last spring
        {
            QTEManager.Instance.difficultyLevel = Difficulty;

            if (qteRandomness == QTERandomness.Random)
            {
                if (qteType == QTEType.Mashing)
                    yield return QTEManager.Instance.RunButtonMashingQTE(mashAction, result => qteSuccess = result);
                else
                    yield return QTEManager.Instance.RunSingleQTE("Random", result => qteSuccess = result);
            }
            else if (qteRandomness == QTERandomness.Preset)
            {
                string qteButton = subsequentSprings[i].QTEButton;

                if (string.IsNullOrEmpty(qteButton))
                {
                    Debug.LogWarning($"Preset QTE Button is empty on spring index {i}");
                    qteSuccess = false;
                }
                else
                {
                    if (qteType == QTEType.Mashing)
                        yield return QTEManager.Instance.RunButtonMashingQTE(qteButton, result => qteSuccess = result);
                    else
                        yield return QTEManager.Instance.RunSingleQTE(qteButton, result => qteSuccess = result);
                }
            }
            else
            {
                Debug.LogWarning("Unknown QTERandomness mode, failing QTE.");
                qteSuccess = false;
            }

            if (!qteSuccess)
            {
                Vector3 failDir = subsequentSprings[i].Spring.up * failOffset.magnitude;
                yield return MoveStraight(playerTransform, subsequentSprings[i].Spring.position, subsequentSprings[i].Spring.position + failDir, failFallTime);
                break;
            }

            // Move to the next spring position after successful QTE
            // Only move if this is not the last spring (to avoid moving past the end)
            if (i < chainLength - 1)
            {
                Vector3 start = subsequentSprings[i].Spring.position;
                Vector3 end = subsequentSprings[i + 1].Spring.position;
                yield return MoveStraight(playerTransform, start, end, travelTime);
            }
        }

        // End sequence logic
        playerRb.isKinematic = false;
        attributes.PlayerDisabled = false;

        if (qteSuccess)
        {
            Vector3 finalDirection = subsequentSprings[^1].Spring.up;
            playerRb.AddForce(finalDirection.normalized * finalBoostForce, ForceMode.VelocityChange);
        }

        inSpringJump = false;
    }

    IEnumerator MoveStraight(Transform obj, Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            obj.position = Vector3.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.position = end;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if (subsequentSprings != null && subsequentSprings.Length > 1)
        {
            for (int i = 0; i < subsequentSprings.Length - 1; i++)
            {
                Gizmos.DrawLine(subsequentSprings[i].Spring.position, subsequentSprings[i + 1].Spring.position);
            }
        }
    }
}

[System.Serializable]
public class SubsequentSprings
{
    public Transform Spring; // used anyway, since its the spring transform
    public string QTEButton; // used is preset
}
