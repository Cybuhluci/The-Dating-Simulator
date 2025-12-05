using Luci;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class MinaRailGrind : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public MinaGroundMove move;
    public MinaGravity gravity;
    public Animator animator;

    [Header("Tuning")]
    public int samplesPerSegment = 12;
    public float modelOrientSpeed = 20f;
    public float exitBlend = 8f;

    // runtime
    bool onRail;
    Rail activeRail;
    List<Vector3> samples = new List<Vector3>();
    List<float> cumulative = new List<float>();
    float totalLength;
    float traveled;

    // parametric position along spline (0..1)
    float tParam;
    int tDirection = 1; //1 = forward, -1 = backward along spline

    [Tooltip("multiply player's horizontal velocity to determine initial grind speed (1 = preserve)")]
    public float preserveSpeedMultiplier = 1f;

    [Tooltip("Allow player to jump off the rail")]
    public bool allowJumpOff = true;

    // store previous rigidbody settings to restore
    CollisionDetectionMode previousCollisionMode = CollisionDetectionMode.Discrete;
    RigidbodyInterpolation previousInterpolation = RigidbodyInterpolation.None;

    // grind runtime
    float currentGrindSpeed = 0f;
    Vector3 lastPos;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!onRail || activeRail == null) return;

        var container = activeRail.spline;
        if (container == null) return;

        Vector3 up = gravity != null ? gravity.SurfaceNormal : Vector3.up;

        // Use the maintained currentGrindSpeed for movement (don't read rb.velocity here)
        float railMax = activeRail.maxSpeed > 0f ? activeRail.maxSpeed : float.MaxValue;
        float speed = Mathf.Clamp(currentGrindSpeed, 0.01f, railMax);

        // Convert linear speed (units/sec) into deltaT using spline length
        float railLength = container.CalculateLength();
        if (railLength <= 0f) railLength = 1f;
        float deltaT = (speed * Time.fixedDeltaTime) / railLength * tDirection;

        tParam += deltaT;

        // handle open/closed spline wrap/clamp
        bool closed = container.Spline.Closed;
        if (closed)
        {
            tParam = Mathf.Repeat(tParam, 1f);
        }
        else
        {
            if (tParam >= 1f)
            {
                tParam = 1f;
                ExitRail();
                return;
            }
            else if (tParam <= 0f)
            {
                tParam = 0f;
                ExitRail();
                return;
            }
        }

        // Sample precise world position and move
        Vector3 pos = (Vector3)container.EvaluatePosition(tParam);

        // compute displacement-based velocity to keep physics coherent
        Vector3 displacement = pos - lastPos;
        Vector3 newVel = displacement / Mathf.Max(Time.fixedDeltaTime, 1e-6f);

        rb.MovePosition(pos);
        rb.linearVelocity = newVel;

        // Orient model to tangent
        Vector3 newTangent = (Vector3)container.EvaluateTangent(tParam);
        if (newTangent.sqrMagnitude > 1e-6f) newTangent.Normalize();
        if (move != null && move.playerModel != null && newTangent.sqrMagnitude > 0f)
        {
            Quaternion targetRot = Quaternion.LookRotation(newTangent, up);
            move.playerModel.rotation = Quaternion.Slerp(move.playerModel.rotation, targetRot, Time.fixedDeltaTime * modelOrientSpeed);
        }

        lastPos = pos;
    }

    // Called by Rail trigger when player enters the rail trigger
    public void EnterRail(Rail rail)
    {
        if (rail == null || rail.spline == null) return;
        if (onRail) return; // already on a rail

        activeRail = rail;
        var container = activeRail.spline;
        if (container == null) return;

        // find nearest t on spline by sampling fallback
        tParam = FindNearestTBySampling(container);

        Vector3 up = gravity != null ? gravity.SurfaceNormal : Vector3.up;

        // Determine direction sign from current velocity
        Vector3 tang = (Vector3)container.EvaluateTangent(tParam);
        if (tang.sqrMagnitude > 1e-6f) tang.Normalize();
        float forwardDot = Vector3.Dot(rb.linearVelocity, tang);
        tDirection = forwardDot >= 0f ? 1 : -1;

        // capture incoming horizontal speed BEFORE we modify rb
        float incomingSpeed = Vector3.ProjectOnPlane(rb.linearVelocity, up).magnitude;

        // Teleport to evaluated position to avoid large MovePosition deltas and jitter
        Vector3 startPos = (Vector3)container.EvaluatePosition(tParam);
        rb.position = startPos;
        rb.rotation = Quaternion.identity; // keep Rigidbody rotation neutral

        // initialize grind speed from incoming momentum and multiplier
        float railMax = activeRail.maxSpeed > 0f ? activeRail.maxSpeed : float.MaxValue;
        currentGrindSpeed = Mathf.Clamp(incomingSpeed * preserveSpeedMultiplier, 0.01f, railMax);

        // set initial velocity along tangent so physics queries are coherent
        rb.linearVelocity = (tang.sqrMagnitude > 1e-6f ? tang.normalized : transform.forward) * currentGrindSpeed + Vector3.Project(rb.linearVelocity, up);
        rb.angularVelocity = Vector3.zero;

        // set lastPos for displacement calc
        lastPos = startPos;

        // choose starting traveled distance as closest point to player (kept for potential exit logic)
        float railLength = container.CalculateLength();
        traveled = Mathf.Clamp01(tParam) * railLength;

        // disable normal control and gravity
        MinaAttributes.Instance.PlayerDisabled = true;
        if (gravity != null) gravity.gravityEnabled = false;

        // animator
        if (animator != null) animator.SetBool("IsGrinding", true);

        // store and set collision & interpolation for smoother movement
        previousCollisionMode = rb.collisionDetectionMode;
        previousInterpolation = rb.interpolation;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        onRail = true;
    }

    float FindNearestTBySampling(SplineContainer container)
    {
        int segmentCount = Mathf.Max(1, container.Spline.Count - 1);
        int totalSamples = segmentCount * samplesPerSegment;
        float bestT = 0f;
        float bestDist = float.MaxValue;
        for (int i = 0; i <= totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            Vector3 p = (Vector3)container.EvaluatePosition(t);
            float d = Vector3.Distance(transform.position, p);
            if (d < bestDist)
            {
                bestDist = d;
                bestT = t;
            }
        }
        return bestT;
    }

    public void ExitRail(bool keepVelocity = true)
    {
        if (!onRail) return;

        onRail = false;

        // restore gravity and player control
        if (gravity != null) gravity.gravityEnabled = true;
        MinaAttributes.Instance.PlayerDisabled = false;

        // animator
        if (animator != null) animator.SetBool("IsGrinding", false);

        // smooth exit: optionally keep velocity along tangent at exit
        if (activeRail != null)
        {
            var container = activeRail.spline;
            if (container != null)
            {
                Vector3 tangent = (Vector3)container.EvaluateTangent(tParam);
                if (tangent.sqrMagnitude > 1e-6f) tangent.Normalize();
                if (keepVelocity && tangent.sqrMagnitude > 0f)
                {
                    float exitSpeed = Mathf.Clamp(Vector3.ProjectOnPlane(rb.linearVelocity, gravity != null ? gravity.SurfaceNormal : Vector3.up).magnitude, 0f, activeRail != null ? activeRail.maxSpeed : float.MaxValue);
                    Vector3 newVel = tangent.normalized * exitSpeed + Vector3.Project(rb.linearVelocity, gravity != null ? gravity.SurfaceNormal : Vector3.up);
                    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, newVel, Time.fixedDeltaTime * exitBlend);
                }
            }
        }

        // restore previous collision and interpolation
        rb.collisionDetectionMode = previousCollisionMode;
        rb.interpolation = previousInterpolation;

        activeRail = null;
        // reset any internal sampling state (none maintained now)
    }

    // optional helper for debugging
    void OnDrawGizmosSelected()
    {
        if (activeRail != null && activeRail.spline != null)
        {
            Gizmos.color = Color.yellow;
            int steps = 32;
            for (int i = 0; i < steps; i++)
            {
                float a = (float)i / steps;
                float b = (float)(i + 1) / steps;
                Vector3 pa = (Vector3)activeRail.spline.EvaluatePosition(a);
                Vector3 pb = (Vector3)activeRail.spline.EvaluatePosition(b);
                Gizmos.DrawLine(pa, pb);
            }
        }
    }
}
