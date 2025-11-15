using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Luci;

public class TrickJumpPanel : StageObject
{
    MinaAttributes attributes = MinaAttributes.Instance;

    public enum QTEType { Sequence, Mashing }

    [Header("QTE Settings")]
    [Range(1, 10)] public int Difficulty = 3;
    public QTEType qteType = QTEType.Sequence;
    public string mashAction = "Jump";

    [Header("References")]
    [SerializeField] private AudioSource PanelSource;
    [SerializeField] private PlayerInput playerInput;

    [Header("Arc Parameters")]
    public float gravity = -9.8f;

    [Header("Arc 1 - Launch to QTE")]
    public Vector3 arc1Offset = new Vector3(0, 5, 10);
    public float arc1Time = 1.2f;

    [Header("Arc 2 - QTE Success Path")]
    public Vector3 arc2SuccessOffset = new Vector3(0, 3, 12);
    public float arc2SuccessTime = 1.0f;

    [Header("Arc 3 - QTE Fail Path")]
    public Vector3 arc3FailOffset = new Vector3(-2, -2, 8);
    public float arc3FailTime = 1.0f;

    [Header("Pause & Boost")]
    public float qtePauseDuration = 0.4f;
    public float finalBoostForce = 50f;

    private bool qteSuccess = false;
    private bool inTrickJump = false;
    private Rigidbody playerRb;
    private Transform playerTransform;

    void Start()
    {
        attributes = MinaAttributes.Instance;
    }

    void OnTriggerEnter(Collider other)
    {
        if (inTrickJump) return;

        playerRb = other.GetComponent<Rigidbody>();
        playerTransform = other.transform;

        StartCoroutine(TrickJumpRoutine());
    }

    IEnumerator TrickJumpRoutine()
    {
        attributes.SetSpecialTimerEvent(true, "QTE");
        inTrickJump = true;
        PanelSource?.Play();

        playerRb.isKinematic = true;
        attributes.PlayerDisabled = true;

        // Arc 1: to QTE point
        Vector3 arc1Target = transform.position + transform.rotation * arc1Offset;
        yield return MoveAlongArc(playerTransform, transform.position, arc1Target, arc1Time);

        yield return new WaitForSeconds(qtePauseDuration);

        QTEManager.Instance.difficultyLevel = Difficulty;

        if (qteType == QTEType.Mashing)
            yield return QTEManager.Instance.RunButtonMashingQTE(mashAction, result => qteSuccess = result);
        else
            yield return QTEManager.Instance.RunQTESequence(result => qteSuccess = result);

        // Arc 2: success or fail path
        Vector3 endTarget = arc1Target + transform.rotation * (qteSuccess ? arc2SuccessOffset : arc3FailOffset);
        float airTime = qteSuccess ? arc2SuccessTime : arc3FailTime;

        yield return MoveAlongArc(playerTransform, arc1Target, endTarget, airTime);

        playerRb.isKinematic = false;
        attributes.PlayerDisabled = false;

        if (qteSuccess)
        {
            Vector3 boostDir = (endTarget - arc1Target).normalized + Vector3.up;
            playerRb.AddForce(boostDir.normalized * finalBoostForce, ForceMode.VelocityChange);
            // TODO: Add trick animation trigger
        }

        inTrickJump = false;
    }

    IEnumerator MoveAlongArc(Transform obj, Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y += gravity * Mathf.Sin(Mathf.PI * t); // simple arc shaping
            obj.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.position = end;
    }

    void OnDrawGizmos()
    {
        Vector3 start = transform.position;

        // Arc 1: to QTE position
        Vector3 arc1End = start + transform.rotation * arc1Offset;
        Gizmos.color = Color.yellow;
        DrawArcGizmo(start, arc1End, arc1Time);

        // Arc 2: success
        Vector3 successEnd = arc1End + transform.rotation * arc2SuccessOffset;
        Gizmos.color = Color.green;
        DrawArcGizmo(arc1End, successEnd, arc2SuccessTime);

        // Arc 3: fail
        Vector3 failEnd = arc1End + transform.rotation * arc3FailOffset;
        Gizmos.color = Color.red;
        DrawArcGizmo(arc1End, failEnd, arc3FailTime);
    }

    void DrawArcGizmo(Vector3 start, Vector3 end, float time)
    {
        Vector3 previous = start;
        for (float t = 0; t <= 1f; t += 0.05f)
        {
            Vector3 point = Vector3.Lerp(start, end, t);
            point.y += gravity * Mathf.Sin(Mathf.PI * t);
            Gizmos.DrawLine(previous, point);
            previous = point;
        }
        Gizmos.DrawLine(previous, end);
    }
}
