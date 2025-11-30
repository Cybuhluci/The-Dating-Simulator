using Luci;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MinaGravity : MonoBehaviour
{
    MinaAttributes attributes = MinaAttributes.Instance;

    [Header("References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] Animator animator;
    [SerializeField] Transform rayOrigin;
    [SerializeField] float groundRayLength = 1f;
    [SerializeField] float groundRayRadius = 0.3f;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float gravityStrength = 50f;

    public Vector3 SurfaceNormal { get; private set; } = Vector3.up;

    [SerializeField] MinaJump jump;

    public Vector3 GravityDirection => -SurfaceNormal;

    public bool gravityEnabled;

    void Start()
    {
        attributes = MinaAttributes.Instance;
    }

    private void Update()
    {
        gravityEnabled = attributes.GravityEnabled;
    }

    public void ManualUpdate()
    {
        if (attributes.GravityEnabled == false) return;

        CheckGrounded();
    }

    void CheckGrounded()
    {
        Vector3 origin = rayOrigin.position + transform.up * groundRayRadius;
        Vector3 direction = -transform.up;

        bool grounded = Physics.SphereCast(origin, groundRayRadius, direction, out RaycastHit hit, groundRayLength + groundRayRadius, groundMask);

        attributes.IsGrounded = grounded;

        if (jump.IsInJumpLockout)
        {
            // During jump lockout, pretend we're NOT grounded
            attributes.IsGrounded = false;
            SurfaceNormal = Vector3.up;
            return;
        }

        if (grounded)
        {
            attributes.IsGrounded = true;
            SurfaceNormal = hit.normal;
            animator.SetBool("BigJump", false);
            animator.SetBool("SmallJump", false);
            animator.SetBool("Airboost", false);
            jump.InBigJump = false;

            // Stick-to-ground correction:
            // If the player's current velocity has a component away from the surface (positive along the normal),
            // remove that upward component and add a small downward bias so the player remains in contact with the surface
            if (rb != null)
            {
                float velAlongNormal = Vector3.Dot(rb.linearVelocity, hit.normal);
                if (velAlongNormal > 0f)
                {
                    // Remove upward component
                    Vector3 horizontal = Vector3.ProjectOnPlane(rb.linearVelocity, hit.normal);
                    // Apply a small downward bias to ensure contact (tweak bias as needed)
                    float downwardBias = Mathf.Max(0.5f, velAlongNormal * 0.5f);
                    rb.linearVelocity = horizontal + (-hit.normal * downwardBias);
                }
            }
        }
        else
        {
            attributes.IsGrounded = false;
            SurfaceNormal = Vector3.up;
        }
    }

    public void ApplyGravity()
    {
        if (attributes.GravityEnabled == false) return; 

        if (!attributes.IsGrounded)
        {
            rb.AddForce(GravityDirection * gravityStrength, ForceMode.Acceleration);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (rayOrigin == null) return;

        Vector3 origin = rayOrigin.position + transform.up * groundRayRadius;
        Vector3 direction = -transform.up;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin + direction * groundRayLength, groundRayRadius);
        Gizmos.DrawLine(origin, origin + direction * groundRayLength);
    }
}