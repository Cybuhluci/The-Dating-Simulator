using Luci;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MinaGroundMove : MonoBehaviour
{
    // Fixed references to MinaAttributes
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
    [SerializeField] float baseSpeed = 30f; 
    public float maxSpeed = 60f; 
    [SerializeField] float acceleration = 90f; 
    [SerializeField] float deceleration = 45f;
    [SerializeField] float airControl = 20f;
    [SerializeField] float turnSpeed = 15f; 
    [SerializeField] float sideInfluence = 0.4f;

    public Vector3 velocity;
    public Vector2 moveInput;

    private Vector3 lastPosition;
    public float currentSpeed;

    private bool wasGroundedLastFrame;
    public float BoostMult;

    void Start()
    {
        attributes = MinaAttributes.Instance;
        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera reference is not set in SonicGroundMove!", this);
        }
    }

    void Update()
    {
        moveInput = input.actions["Move"].ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 normal = gravity.SurfaceNormal;
        Vector3 processedInputDir;

        // --- Determine Actual Speed/Control Values based on Mode ---
        float currentBaseSpeed = baseSpeed;
        float currentMaxSpeed = maxSpeed;
        float currentAcceleration = acceleration;
        float currentDeceleration = deceleration;
        float currentAirControl = airControl;

        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, normal).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, normal).normalized;
        processedInputDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        if (attributes.IsGrounded)
        {
            if (!wasGroundedLastFrame)
            {
                Vector3 landingVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, normal);
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, landingVelocity, 0.5f);
                velocity = landingVelocity;
            }

            if (!jump.hasJumped)
            {
                GroundMovement(processedInputDir, normal, currentBaseSpeed, currentMaxSpeed, currentAcceleration, currentDeceleration);
            }
        }
        else
        {
            AirMovement(processedInputDir, normal, currentBaseSpeed, currentAirControl);
        }

        RotateModel(normal);

        currentSpeed = ((transform.position - lastPosition) / Time.fixedDeltaTime).magnitude;
        lastPosition = transform.position;

        wasGroundedLastFrame = attributes.IsGrounded;
        jump.hasJumped = false;
        gravity.ManualUpdate();
    }

    // Modified GroundMovement to accept current speed/accel values
    void GroundMovement(Vector3 inputDir, Vector3 groundNormal, float currentBaseSpeed, float currentMaxSpeed, float currentAcceleration, float currentDeceleration)
    {
        if (attributes.PlayerDisabled) return;

        Vector3 horizontalVel = Vector3.ProjectOnPlane(rb.linearVelocity, groundNormal);

        if (BoostMult > 1f)
        {
            Vector3 forward = Vector3.ProjectOnPlane(playerModel.forward, groundNormal).normalized;
            Vector3 right = Vector3.Cross(groundNormal, forward).normalized;

            float forwardSpeed = currentBaseSpeed * BoostMult;

            float driftAmount = Vector3.Dot(inputDir, right);
            Vector3 sideDrift = right * driftAmount * sideInfluence * forwardSpeed;

            Vector3 forwardTarget = forward * forwardSpeed;
            Vector3 targetVelocity = forwardTarget + sideDrift;

            horizontalVel = Vector3.MoveTowards(horizontalVel, targetVelocity, currentAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            if (inputDir.magnitude > 0.1f)
            {
                float targetSpeed = currentBaseSpeed;
                Vector3 targetVelocity = inputDir * targetSpeed;
                horizontalVel = Vector3.MoveTowards(horizontalVel, targetVelocity, currentAcceleration * Time.fixedDeltaTime);
            }
            else
            {
                horizontalVel = Vector3.MoveTowards(horizontalVel, Vector3.zero, currentDeceleration * Time.fixedDeltaTime);
            }
        }

        Vector3 newVelocity = Vector3.ProjectOnPlane(horizontalVel, groundNormal);
        Vector3 downwardForce = -groundNormal * 2f; // Ensure consistent grounded gravity pull

        rb.linearVelocity = newVelocity + downwardForce;
        velocity = newVelocity;
    }

    // Modified AirMovement to accept current speed/control values
    void AirMovement(Vector3 inputDir, Vector3 normal, float currentBaseSpeed, float currentAirControl)
    {
        if (attributes.PlayerDisabled) return;

        gravity.ApplyGravity(); // Main gravity applied here

        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, normal);
        Vector3 verticalVelocity = Vector3.Project(rb.linearVelocity, normal);

        if (inputDir.magnitude > 0.1f)
        {
            Vector3 targetVelocity = inputDir * currentBaseSpeed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, currentAirControl * Time.fixedDeltaTime);
        }

        rb.linearVelocity = horizontalVelocity + verticalVelocity;
    }

    void RotateModel(Vector3 normal)
    {
        Vector3 movementDirection;

        if (attributes.IsGrounded)
        {
            movementDirection = Vector3.ProjectOnPlane(rb.linearVelocity, normal);
        }
        else
        {
            movementDirection = Vector3.ProjectOnPlane(rb.linearVelocity, normal);
        }

        if (movementDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(movementDirection, normal);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * turnSpeed));
        }
    }
}