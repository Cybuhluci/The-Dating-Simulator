using Luci;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MinaGroundMove : MonoBehaviour
{
    MinaAttributes attributes = MinaAttributes.Instance;

    [Header("References")]
    public Rigidbody rb;
    [SerializeField] PlayerInput input;
    [SerializeField] MinaGravity gravity;
    [SerializeField] MinaJump jump;
    public Transform playerModel;
    [SerializeField] Transform cameraTransform;
    [SerializeField] MinaPlayerCamera playerCamera;

    [Header("Movement Settings")]
    [SerializeField] float baseSpeed = 30f; // target run speed when holding input on flat ground
    [SerializeField] float acceleration = 90f; // how fast we accelerate up to baseSpeed
    [SerializeField] float deceleration = 45f; // how fast we slow when no input
    [SerializeField] float airControl = 20f;
    [SerializeField] float steerSpeed = 8f;   // how quickly direction steers toward input at high speed
    [SerializeField] float turnSpeed = 15f;
    [SerializeField] float sideInfluence = 0.4f;
    [SerializeField] public float jetSpeed = 100f; // For animation only

    [Header("Slope Settings")]
    [SerializeField] float slopeAccelMultiplier = 50f; // extra speed gain when going downhill (units/sec^2)
    [SerializeField] float slopeDecelMultiplier = 60f; // extra speed loss when going uphill (units/sec^2)

    // readouts
    public float velocity;
    public Vector3 projVelocity;
    public Vector2 moveInput;
    private Vector3 lastPosition;
    public float currentSpeed;

    // runtime
    private bool wasGroundedLastFrame;
    public float BoostMult;

    void Start()
    {
        attributes = MinaAttributes.Instance;
        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera reference is not set in MinaGroundMove!", this);
        }
    }

    void Update()
    {
        moveInput = input.actions["Move"].ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        if (attributes.PlayerDisabled) return;

        Vector3 normal = gravity.SurfaceNormal;

        // camera-relative input, projected to ground later when used
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, normal).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, normal).normalized;
        Vector3 inputDir = (camForward * moveInput.y + camRight * moveInput.x);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        if (attributes.IsGrounded)
            GroundMovement(inputDir, normal);
        else
            AirMovement(inputDir, normal);

        RotateModel(normal);

        currentSpeed = ((transform.position - lastPosition) / Time.fixedDeltaTime).magnitude;
        lastPosition = transform.position;
        wasGroundedLastFrame = attributes.IsGrounded;
        gravity.ManualUpdate();

        velocity = rb.linearVelocity.magnitude;
    }

    // Hybrid velocity-preserving controller using rb.linearVelocity:
    // - preserves existing speed when steering at high speed (so boosts/air-dashes stick)
    // - accelerates up to baseSpeed when input held and current speed is lower
    // - steers direction smoothly when above baseSpeed using steerSpeed
    void GroundMovement(Vector3 inputDir, Vector3 groundNormal)
    {
        // Split current velocity
        Vector3 verticalVel = Vector3.Project(rb.linearVelocity, groundNormal);
        Vector3 horizontalVel = Vector3.ProjectOnPlane(rb.linearVelocity, groundNormal);
        float speedOnPlane = horizontalVel.magnitude;

        // downhill direction (points along surface toward world-down)
        Vector3 downhillDir = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;

        // Desired behavior when player gives input
        if (inputDir.magnitude > 0.1f)
        {
            // compute camera-relative, ground-projected desired direction
            Vector3 desiredDir = Vector3.ProjectOnPlane(inputDir, groundNormal).normalized;

            // If we are effectively stopped, accelerate up toward baseSpeed
            if (speedOnPlane < 0.1f)
            {
                Vector3 targetVel = desiredDir * baseSpeed;
                Vector3 newHoriz = Vector3.MoveTowards(horizontalVel, targetVel, acceleration * Time.fixedDeltaTime);

                // slope effect: if moving downhill, gain speed; if uphill, lose speed
                float slopeDot = Vector3.Dot(desiredDir, downhillDir);
                if (slopeDot > 0f)
                    newHoriz = newHoriz.normalized * (newHoriz.magnitude + slopeDot * slopeAccelMultiplier * Time.fixedDeltaTime);
                else
                    newHoriz = newHoriz.normalized * Mathf.Max(0f, newHoriz.magnitude + slopeDot * slopeDecelMultiplier * Time.fixedDeltaTime);

                rb.linearVelocity = newHoriz + verticalVel;
            }
            else
            {
                // If below baseSpeed, accelerate towards baseSpeed in input direction
                if (speedOnPlane < baseSpeed * (BoostMult > 1f ? BoostMult : 1f))
                {
                    Vector3 targetVel = desiredDir * baseSpeed * (BoostMult > 1f ? BoostMult : 1f);
                    Vector3 newHoriz = Vector3.MoveTowards(horizontalVel, targetVel, acceleration * Time.fixedDeltaTime);

                    float slopeDot = Vector3.Dot(desiredDir, downhillDir);
                    if (slopeDot > 0f)
                        newHoriz = newHoriz.normalized * (newHoriz.magnitude + slopeDot * slopeAccelMultiplier * Time.fixedDeltaTime);
                    else
                        newHoriz = newHoriz.normalized * Mathf.Max(0f, newHoriz.magnitude + slopeDot * slopeDecelMultiplier * Time.fixedDeltaTime);

                    rb.linearVelocity = newHoriz + verticalVel;
                }
                else
                {
                    // Above baseSpeed: preserve magnitude, steer direction toward desiredDir
                    float preservedSpeed = speedOnPlane;
                    // avoid NaN
                    if (horizontalVel.sqrMagnitude < 0.0001f)
                    {
                        horizontalVel = desiredDir * preservedSpeed;
                    }

                    Vector3 currentDir = horizontalVel.normalized;
                    // slerp-like steering of direction (using Slerp for better angular feel)
                    Vector3 steeredDir = Vector3.Slerp(currentDir, desiredDir, Mathf.Clamp01(steerSpeed * Time.fixedDeltaTime));
                    Vector3 newHoriz = steeredDir.normalized * preservedSpeed;

                    // apply slope influence on preserved speed (only if moving along downhill/uphill)
                    float slopeDot = Vector3.Dot(steeredDir, downhillDir);
                    if (slopeDot > 0f)
                        newHoriz = newHoriz.normalized * (newHoriz.magnitude + slopeDot * slopeAccelMultiplier * Time.fixedDeltaTime);
                    else
                        newHoriz = newHoriz.normalized * Mathf.Max(0f, newHoriz.magnitude + slopeDot * slopeDecelMultiplier * Time.fixedDeltaTime);

                    rb.linearVelocity = newHoriz + verticalVel;
                }
            }
        }
        else
        {
            // No input: gently decelerate horizontal velocity, preserve vertical
            Vector3 newHoriz = Vector3.MoveTowards(horizontalVel, Vector3.zero, deceleration * Time.fixedDeltaTime);
            rb.linearVelocity = newHoriz + verticalVel;
        }

        // update projection for animations/logic
        projVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, groundNormal);
    }

    void AirMovement(Vector3 inputDir, Vector3 normal)
    {
        // Let gravity system handle vertical acceleration
        gravity.ApplyGravity();

        // Air control: small steering force to influence direction while preserving speed
        Vector3 horizontalVel = Vector3.ProjectOnPlane(rb.linearVelocity, normal);
        float speedOnPlane = horizontalVel.magnitude;

        if (inputDir.magnitude > 0.1f && speedOnPlane > 0.01f)
        {
            Vector3 desiredDir = Vector3.ProjectOnPlane(inputDir, normal).normalized;
            Vector3 currentDir = horizontalVel.normalized;
            Vector3 steeredDir = Vector3.Slerp(currentDir, desiredDir, Mathf.Clamp01((airControl * 0.1f) * Time.fixedDeltaTime));
            Vector3 newHoriz = steeredDir * speedOnPlane;
            Vector3 verticalVel = Vector3.Project(rb.linearVelocity, normal);
            rb.linearVelocity = newHoriz + verticalVel;
        }
        // if near zero horizontal speed, small impulse to start movement in air if input given
        else if (inputDir.magnitude > 0.1f && speedOnPlane <= 0.01f)
        {
            Vector3 desiredDir = Vector3.ProjectOnPlane(inputDir, normal).normalized;
            Vector3 verticalVel = Vector3.Project(rb.linearVelocity, normal);
            rb.linearVelocity = desiredDir * (baseSpeed * 0.2f) + verticalVel;
        }
    }

    void RotateModel(Vector3 normal)
    {
        Vector3 movementDirection = Vector3.ProjectOnPlane(rb.linearVelocity, normal);
        if (movementDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(movementDirection, normal);
            // rotate playerModel to face movement (keeps Rigidbody free for collisions)
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRot, Time.fixedDeltaTime * turnSpeed);
        }
    }
}