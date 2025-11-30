using Luci;
using UnityEngine;


public class MinaAnimations : MonoBehaviour
{
    MinaAttributes attributes = MinaAttributes.Instance;

    [SerializeField] MinaGroundMove move;
    [SerializeField] Rigidbody rb;
    public Animator animator;
    [SerializeField] MinaGravity gravity;

    void Start()
    {
        attributes = MinaAttributes.Instance;
    }

    void Update()
    {
        if (attributes.PlayerDisabled) return;

        float speed = move.currentSpeed;

        // Set movement parameters
        animator.SetFloat("Speed", speed);
        animator.SetFloat("Speed Percent", speed / move.jetSpeed);

        float leftRightInput = move.moveInput.x;
        
        // Correct horizontal velocity calculations
        Vector3 rbHorizontal = Vector3.ProjectOnPlane(rb.linearVelocity, gravity.SurfaceNormal);
        float rbHorizontalSpeed = rbHorizontal.magnitude;

        // Fix: vertical velocity signed
        float rbVerticalSpeed = Vector3.Dot(rb.linearVelocity, gravity.SurfaceNormal);

        animator.SetFloat("RB Horizontal Velocity", rbHorizontalSpeed);
        animator.SetFloat("RB Vertical Velocity", rbVerticalSpeed);

        // Also based on MovePosition movement velocity
        Vector3 moveHorizontal = Vector3.ProjectOnPlane(move.projVelocity, gravity.SurfaceNormal);
        float moveHorizontalSpeed = moveHorizontal.magnitude;
        animator.SetFloat("Move Horizontal Velocity", moveHorizontalSpeed);

        animator.SetBool("Grounded", attributes.IsGrounded);

        // New movement checks
        bool rbMoving = rb.linearVelocity.sqrMagnitude > 0.01f;
        bool moveMoving = move.projVelocity.sqrMagnitude > 0.01f;
        bool theoMoving = move.moveInput.sqrMagnitude > 0.01f;
        bool anyMoving = rbMoving || moveMoving;

        animator.SetBool("RB Moving", rbMoving);
        animator.SetBool("Move Moving", moveMoving);
        animator.SetBool("Theo Moving", theoMoving);
        animator.SetBool("Moving", anyMoving);
    }
}