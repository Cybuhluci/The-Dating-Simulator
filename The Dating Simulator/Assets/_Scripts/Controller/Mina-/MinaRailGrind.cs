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

    [Tooltip("multiply player's horizontal velocity to determine initial grind speed (1 = preserve)")]
    public float preserveSpeedMultiplier = 1f;

    [Tooltip("Allow player to jump off the rail")]
    public bool allowJumpOff = true;

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
        if (!onRail || activeRail == null || samples.Count < 2) return;

        // Determine current grind speed: preserve current horizontal speed but clamp to rail max
        Vector3 horizVel = Vector3.ProjectOnPlane(rb.linearVelocity, gravity != null ? gravity.SurfaceNormal : Vector3.up);
        float speed = horizVel.magnitude * preserveSpeedMultiplier;
        float railMax = activeRail.maxSpeed > 0f ? activeRail.maxSpeed : float.MaxValue;
        speed = Mathf.Clamp(speed, 0.1f, railMax);

        // Advance traveled by speed * dt, clamped to total length
        float delta = speed * Time.fixedDeltaTime;
        traveled = Mathf.Min(traveled + delta, totalLength);

        // Sample position via arc-length table
        Vector3 pos = SamplePositionAtDistance(traveled);

        // Move player along rail deterministically (preserve collisions)
        rb.MovePosition(pos);

        // Orient model to tangent
        Vector3 tangent = SampleTangentAtDistance(traveled);
        if (move != null && move.playerModel != null && tangent.sqrMagnitude > 0f)
        {
            Quaternion targetRot = Quaternion.LookRotation(tangent.normalized, gravity != null ? gravity.SurfaceNormal : Vector3.up);
            move.playerModel.rotation = Quaternion.Slerp(move.playerModel.rotation, targetRot, Time.fixedDeltaTime * modelOrientSpeed);
        }

        // If we reached the end of the rail, exit
        if (Mathf.Approximately(traveled, totalLength))
        {
            ExitRail();
        }
    }

    // Called by Rail trigger when player enters the rail trigger
    public void EnterRail(Rail rail)
    {
        if (rail == null || rail.spline == null) return;
        if (onRail) return; // already on a rail

        activeRail = rail;
        BuildSamplesFromSpline(rail.spline);
        if (samples.Count < 2) return;

        // choose starting traveled distance as closest point to player
        traveled = FindClosestDistanceOnSamples(transform.position);

        // disable normal control and gravity
        //MinaAttributes.Instance.PlayerDisabled = true;
        if (gravity != null) gravity.gravityEnabled = false;

        // animator
        if (animator != null) animator.SetBool("IsGrinding", true);

        // setup collisions for high speed
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        onRail = true;
    }

    public void ExitRail(bool keepVelocity = true)
    {
        if (!onRail) return;

        onRail = false;

        // restore gravity and player control
        if (gravity != null) gravity.gravityEnabled = true;
        //MinaAttributes.Instance.PlayerDisabled = false;

        // animator
        if (animator != null) animator.SetBool("IsGrinding", false);

        // smooth exit: optionally keep velocity along tangent at exit
        Vector3 tangent = SampleTangentAtDistance(traveled);
        if (keepVelocity && tangent.sqrMagnitude > 0f)
        {
            float exitSpeed = Mathf.Clamp(Vector3.ProjectOnPlane(rb.linearVelocity, gravity != null ? gravity.SurfaceNormal : Vector3.up).magnitude, 0f, activeRail != null ? activeRail.maxSpeed : float.MaxValue);
            Vector3 newVel = tangent.normalized * exitSpeed + Vector3.Project(rb.linearVelocity, gravity != null ? gravity.SurfaceNormal : Vector3.up);
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, newVel, Time.fixedDeltaTime * exitBlend);
        }

        // reset collision mode if needed
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        activeRail = null;
        samples.Clear();
        cumulative.Clear();
        totalLength = 0f;
        traveled = 0f;
    }

    void BuildSamplesFromSpline(SplineContainer container)
    {
        samples.Clear();
        cumulative.Clear();
        totalLength = 0f;

        if (container == null) return;

        Spline spline = container.Spline;
        if (spline.Count == 0) return;

        int segmentCount = Mathf.Max(1, spline.Count - 1);
        int totalSamples = segmentCount * samplesPerSegment;
        Vector3 prev = container.EvaluatePosition(0f);
        samples.Add(prev);
        cumulative.Add(0f);

        for (int i = 1; i <= totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            Vector3 p = container.EvaluatePosition(t);
            totalLength += Vector3.Distance(prev, p);
            samples.Add(p);
            cumulative.Add(totalLength);
            prev = p;
        }
    }

    float FindClosestDistanceOnSamples(Vector3 worldPos)
    {
        float best = 0f;
        float bestDist = float.MaxValue;
        for (int i = 0; i < samples.Count; i++)
        {
            float d = Vector3.Distance(worldPos, samples[i]);
            if (d < bestDist)
            {
                bestDist = d;
                best = cumulative[i];
            }
        }
        return best;
    }

    Vector3 SamplePositionAtDistance(float distance)
    {
        if (samples.Count == 0) return transform.position;
        distance = Mathf.Clamp(distance, 0f, totalLength);

        int idx = cumulative.BinarySearch(distance);
        if (idx < 0) idx = ~idx;
        idx = Mathf.Clamp(idx, 1, samples.Count - 1);

        float prev = cumulative[idx - 1];
        float next = cumulative[idx];
        float seg = next - prev;
        float t = seg > 0f ? (distance - prev) / seg : 0f;
        return Vector3.Lerp(samples[idx - 1], samples[idx], t);
    }

    Vector3 SampleTangentAtDistance(float distance)
    {
        if (samples.Count < 2) return transform.forward;
        distance = Mathf.Clamp(distance, 0f, totalLength);
        int idx = cumulative.BinarySearch(distance);
        if (idx < 0) idx = ~idx;
        idx = Mathf.Clamp(idx, 1, samples.Count - 1);

        int i0 = Mathf.Max(0, idx - 2);
        int i1 = Mathf.Max(0, idx - 1);
        int i2 = Mathf.Min(samples.Count - 1, idx);
        int i3 = Mathf.Min(samples.Count - 1, idx + 1);

        Vector3 tangent = (samples[i2] - samples[i1]).normalized;
        if (tangent.sqrMagnitude < 1e-6f)
            tangent = (samples[i3] - samples[i0]).normalized;
        return tangent;
    }
}
