using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Collider))]
public class Rail : MonoBehaviour
{
    public SplineContainer spline;
    [Tooltip("Maximum grind speed on this rail (units/sec). Typically set to player's boost top speed.")]
    public float maxSpeed = 300f;

    [Tooltip("If true the trigger on this GameObject will allow the player to attach to this rail.")]
    public bool enableTriggerAttach = true;

    [Tooltip("How close the player must be to the spline to be considered 'on rail' when attaching manually (world units).")]
    public float attachRadius = 1.0f;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!enableTriggerAttach) return;

        MinaRailGrind grinder = other.GetComponentInParent<MinaRailGrind>();
        if (grinder == null) return;

        grinder.EnterRail(this);
    }

    void OnDrawGizmosSelected()
    {
        if (spline == null) return;
        Gizmos.color = Color.yellow;
        int steps = 32;
        Vector3 prev = spline.EvaluatePosition(0f);
        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector3 p = spline.EvaluatePosition(t);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }
}
