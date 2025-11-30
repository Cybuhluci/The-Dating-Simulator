using Luci;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MinaLightSpeedDash : MonoBehaviour // NOT IN WORKING STATE, well it works, but badly.
{
    [Header("References")]
    public Rigidbody rb;
    public MinaGroundMove move;
    public MinaGravity gravity;
    public PlayerInput input; // optional, will listen for action "LightDash" if present
    public Animator animator;

    [Header("Detection")]
    public float detectRadius =25f;
    public float maxLinkDistance =20f; // max distance between rings to chain
    public int maxChain =20;

    [Header("Spline/Dash")]
    public int samplesPerSegment =12; // sampling resolution per spline segment
    public float dashSpeed =140f; // units per second along spline
    public float ringProximity =1.2f; // collection radius along spline
    public float perRingDelay =0.02f;
    public bool consumeRings = true;

    [Header("Hooks")]
    public AudioSource dashAudio;

    bool isDashing;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (input != null && !isDashing)
        {
            var action = input.actions["LightDash"];
            if (action != null && action.WasPressedThisFrame())
            {
                StartLightSpeedDash();
            }
        }
    }

    public void StartLightSpeedDash()
    {
        if (isDashing) return;

        List<Transform> chain = BuildChain();
        if (chain.Count ==0) return;

        StartCoroutine(PerformSplineDash(chain));
    }

    List<Transform> BuildChain()
    {
        List<Transform> chain = new List<Transform>();

        Ring[] rings = GameObject.FindObjectsOfType<Ring>();
        if (rings == null || rings.Length ==0) return chain;

        Vector3 origin = transform.position;

        Transform first = null;
        float bestDist = float.MaxValue;
        foreach (var r in rings)
        {
            if (r == null) continue;
            float d = Vector3.Distance(origin, r.transform.position);
            if (d <= detectRadius && d < bestDist)
            {
                bestDist = d;
                first = r.transform;
            }
        }

        if (first == null) return chain;

        chain.Add(first);

        Transform last = first;
        for (int i =1; i < maxChain; i++)
        {
            Transform best = null;
            float bestScore = float.MaxValue;
            foreach (var r in rings)
            {
                if (r == null) continue;
                Transform t = r.transform;
                if (chain.Contains(t)) continue;

                float d = Vector3.Distance(last.position, t.position);
                if (d > maxLinkDistance) continue;

                if (d < bestScore)
                {
                    bestScore = d;
                    best = t;
                }
            }

            if (best == null) break;
            chain.Add(best);
            last = best;
        }

        return chain;
    }

    // Catmull-Rom interpolation
    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // standard Catmull-Rom with tension0.5
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 -5f * p1 +4f * p2 - p3) * t2 + (-p0 +3f * p1 -3f * p2 + p3) * t3);
    }

    IEnumerator PerformSplineDash(List<Transform> chain)
    {
        isDashing = true;

        // backup states
        bool oldGravity = (gravity != null) ? gravity.gravityEnabled : true;
        bool oldPlayerDisabled = MinaAttributes.Instance.PlayerDisabled;

        // disable player control and gravity so dash is deterministic
        MinaAttributes.Instance.PlayerDisabled = true;
        if (gravity != null) gravity.gravityEnabled = false;

        if (animator != null) animator.SetBool("LightDash", true);
        dashAudio.Play();

        // Build control points for Catmull-Rom: duplicate start and end to have proper endpoints
        List<Vector3> ctrl = new List<Vector3>();
        // prepend twice the current position for smooth start
        ctrl.Add(transform.position);
        ctrl.Add(transform.position);
        foreach (var t in chain) ctrl.Add(t.position);
        // append last twice
        ctrl.Add(chain[chain.Count -1].position);
        ctrl.Add(chain[chain.Count -1].position);

        // sample spline to points and compute arc-length table
        List<Vector3> samples = new List<Vector3>();
        List<float> cumulative = new List<float>();
        float total =0f;

        for (int i =0; i < ctrl.Count -3; i++)
        {
            for (int s =0; s <= samplesPerSegment; s++)
            {
                float t = (float)s / (float)samplesPerSegment;
                Vector3 p = CatmullRom(ctrl[i], ctrl[i +1], ctrl[i +2], ctrl[i +3], t);
                if (samples.Count >0)
                {
                    total += Vector3.Distance(samples[samples.Count -1], p);
                }
                samples.Add(p);
                cumulative.Add(total);
            }
        }

        if (samples.Count <2)
        {
            // nothing to do
            if (gravity != null) gravity.gravityEnabled = oldGravity;
            MinaAttributes.Instance.PlayerDisabled = oldPlayerDisabled;
            if (animator != null) animator.SetBool("LightDash", false);
            isDashing = false;
            yield break;
        }

        // set continuous collision detection for safety
        var oldCollision = rb.collisionDetectionMode;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // keep list of remaining ring transforms to collect
        HashSet<Transform> remaining = new HashSet<Transform>(chain);

        float traveled =0f;
        float lastTotal = cumulative[cumulative.Count -1];

        // initial position set to first sample to avoid snap
        rb.MovePosition(samples[0]);

        while (traveled < lastTotal)
        {
            float delta = dashSpeed * Time.fixedDeltaTime;
            traveled += delta;
            if (traveled > lastTotal) traveled = lastTotal;

            // find sample index
            int idx = cumulative.BinarySearch(traveled);
            if (idx <0) idx = ~idx;
            idx = Mathf.Clamp(idx,1, samples.Count -1);

            float prevLen = cumulative[idx -1];
            float nextLen = cumulative[idx];
            float segmentLen = nextLen - prevLen;
            float localT = (segmentLen >0f) ? (traveled - prevLen) / segmentLen :0f;

            Vector3 pos = Vector3.Lerp(samples[idx -1], samples[idx], localT);

            // set position via MovePosition for deterministic path following
            rb.MovePosition(pos);

            // orient model to tangent
            Vector3 tangent = (samples[Mathf.Min(idx +1, samples.Count -1)] - samples[Mathf.Max(0, idx -1)]).normalized;
            if (move != null && move.playerModel != null && tangent.sqrMagnitude >0f)
            {
                Quaternion targetRot = Quaternion.LookRotation(tangent, gravity != null ? gravity.SurfaceNormal : Vector3.up);
                move.playerModel.rotation = Quaternion.Slerp(move.playerModel.rotation, targetRot, Time.fixedDeltaTime *20f);
            }

            // check for rings within proximity and collect
            List<Transform> collected = new List<Transform>();
            foreach (var r in remaining)
            {
                if (r == null) continue;
                if (Vector3.Distance(pos, r.position) <= ringProximity)
                {
                    collected.Add(r);
                }
            }

            foreach (var r in collected)
            {
                remaining.Remove(r);
                if (consumeRings && r != null) Destroy(r.gameObject);
                yield return new WaitForSeconds(perRingDelay);
            }

            yield return new WaitForFixedUpdate();
        }

        // restore
        rb.collisionDetectionMode = oldCollision;
        if (gravity != null) gravity.gravityEnabled = oldGravity;
        MinaAttributes.Instance.PlayerDisabled = oldPlayerDisabled;
        if (animator != null) animator.SetBool("LightDash", false);

        isDashing = false;
        yield break;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
