using UnityEngine;
using UnityEngine.InputSystem;
using Luci;

public class MinaJump : MonoBehaviour
{
    MinaAttributes attributes = MinaAttributes.Instance;

    [Header("References")]
    [SerializeField] PlayerInput input;
    [SerializeField] Rigidbody rb;
    [SerializeField] MinaGravity gravity;
    [SerializeField] MinaGroundMove move;
    [SerializeField] Animator animator;
    [SerializeField] AudioSource SourceJump;
    [SerializeField] AudioSource SourceSpin;
    [SerializeField] AudioClip Jump;
    [SerializeField] AudioClip Spin;
    public GameObject Spinball;

    [Header("Jump Settings")]
    [SerializeField] float jumpHeight = 12f;
    [SerializeField] float maxJumpHorizontalSpeed = 15f;
    [SerializeField] float gravityStrength = 50f;

    [Header("Variable Jump")]
    [SerializeField] float jumpCutMultiplier = 0.5f;
    [SerializeField] float spinJumpTriggerHeight = 2.5f; // Switch to spin animation past this height

    private bool isJumping;
    private float jumpStartY;

    [Header("Coyote Time")]
    [SerializeField] float coyoteTime = 0.2f;
    private float coyoteTimer;

    [Header("Jump Buffer")]
    [SerializeField] float jumpBufferTime = 0.15f;
    private float jumpBufferTimer;

    [Header("Lockout After Jump")]
    [SerializeField] float lockoutTime = 0.2f;
    private float lockoutTimer;

    public bool hasJumped;
    public bool InBigJump;
    public bool inAirBoost;

    public bool IsInJumpLockout => lockoutTimer > 0f;

    [Header("Grounded Jump Cooldown")]
    [SerializeField] float groundedJumpCooldown = 0.15f;
    private float groundedJumpCooldownTimer = 0f;

    void Start()
    {
        attributes = MinaAttributes.Instance;
    }

    void Update()
    {
        if (attributes.PlayerDisabled) return;

        if (!inAirBoost)
        {
            Spinball.SetActive(InBigJump);
        }
        else 
        {             
            Spinball.SetActive(false);
        }

        if (attributes.IsGrounded)
        {
            coyoteTimer = coyoteTime;
            groundedJumpCooldownTimer -= Time.deltaTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            groundedJumpCooldownTimer = groundedJumpCooldown; // Reset cooldown when airborne
        }

        if (lockoutTimer > 0f)
            lockoutTimer -= Time.deltaTime;

        if (input.actions["Jump"].WasPressedThisFrame())
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        TryJump();

        // Switch to spin animation if held and height exceeded
        if (isJumping)
        {
            float jumpHeightSoFar = transform.position.y - jumpStartY;

            if (input.actions["Jump"].IsPressed() && jumpHeightSoFar > spinJumpTriggerHeight)
            {
                if (!InBigJump)
                {
                    InBigJump = true;

                    animator.SetBool("BigJump", true);   // Activate spin
                    animator.SetBool("SmallJump", false); // Deactivate hurdle

                    if (!SourceSpin.isPlaying)
                    {
                        SourceSpin.Play();
                    }
                    
                }
            }

            // Cut jump early if released
            if (!input.actions["Jump"].IsPressed())
            {
                if (rb.linearVelocity.y > 0f)
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier, rb.linearVelocity.z);

                isJumping = false;
            }
        }

        // Stop looping spin sound if interrupted
        if (!InBigJump && SourceSpin.isPlaying)
        {
            SourceSpin.Stop();
            animator.SetBool("BigJump", false);
        }
    }

    void TryJump()
    {
        if (jumpBufferTimer > 0f && coyoteTimer > 0f && lockoutTimer <= 0f && groundedJumpCooldownTimer <= 0f)
        {
            PerformJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            lockoutTimer = lockoutTime;
        }
    }

    void PerformJump()
    {
        SourceJump.PlayOneShot(Jump);

        Vector3 up = gravity.SurfaceNormal;

        // Horizontal momentum
        Vector3 forward = Vector3.ProjectOnPlane(move.playerModel.forward, up).normalized;
        Vector3 horizontalVelocity = forward * move.currentSpeed;

        if (horizontalVelocity.magnitude > maxJumpHorizontalSpeed)
            horizontalVelocity = horizontalVelocity.normalized * maxJumpHorizontalSpeed;

        float jumpVelocity = Mathf.Sqrt(2f * gravityStrength * jumpHeight);
        rb.linearVelocity = horizontalVelocity + up * jumpVelocity;

        hasJumped = true;
        isJumping = true;
        jumpStartY = transform.position.y;

        animator.SetBool("SmallJump", true);  // Start with hurdle
        animator.SetBool("BigJump", false);   // Not in spin yet
        InBigJump = false;
        SourceSpin.Stop();
    }
}