using Luci;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MinaHomingAttack : MonoBehaviour
{
    MinaAttributes attributes = MinaAttributes.Instance;

    [Header("References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] MinaGravity gravity;
    [SerializeField] MinaGroundMove move;
    [SerializeField] MinaJump jump;
    [SerializeField] GameObject homingReticle;
    [SerializeField] Animator animator;
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] AudioSource HomingSource;
    [SerializeField] private HomingTarget homingTarget;

    [Header("Homing Settings")]
    [SerializeField] float homingRadius = 10f;
    [SerializeField] float homingSpeed = 70f;
    [SerializeField] float attackDuration = 0.25f;
    [SerializeField] float hitProximity = 1.2f;

    [SerializeField] float airDashForce = 20f;
    [SerializeField] float airDashUpForce = 8f; // added upward arc strength
    [SerializeField] float airdashLength = 1.0f;
    [SerializeField] private bool hasAirDashed = false;

    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float attackTimer;
    [SerializeField] private bool isHoming;

    public bool inAirBoost;

    void Start()
    {
        attributes = MinaAttributes.Instance;
    }

    void Update()
    {
        if (attributes.PlayerDisabled) return;

        if (inAirBoost)
        {
            trailRenderer.emitting = false;
        }

        TryFindTarget();
        TryUpdateReticle();

        if (playerInput.actions["Attack"].WasPressedThisFrame())
        {
            if (!attributes.IsGrounded && homingTarget != null)
            {
                if (isHoming) return;
                StartHomingAttack(homingTarget.transform.position);
            }
            else if (!attributes.IsGrounded && !hasAirDashed)
            {
                DoAirDash();
            }
        }

        if (attributes.IsGrounded)
        {
            hasAirDashed = false;
            trailRenderer.emitting = false;
        }
    }

    void FixedUpdate()
    {
        if (!isHoming) return;

        attackTimer += Time.fixedDeltaTime;

        Vector3 direction = (homingTarget.reticlePosition.position - transform.position).normalized;
        rb.linearVelocity = direction * homingSpeed;

        if (Vector3.Distance(transform.position, targetPosition) <= hitProximity || attackTimer >= attackDuration)
        {
            EndHoming();
        }
    }

    void TryFindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, homingRadius);
        float bestScore = float.MinValue;
        HomingTarget bestTarget = null;

        foreach (var hit in hits)
        {
            HomingTarget target = hit.GetComponentInParent<HomingTarget>();
            if (target == null) continue;

            Vector3 toTarget = target.transform.position - transform.position;
            float distance = toTarget.magnitude;
            float forwardDot = Vector3.Dot(toTarget.normalized, transform.forward);

            float score = (1f - distance / homingRadius) + forwardDot;

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = target;
            }
        }

        homingTarget = bestTarget;
    }

    void TryUpdateReticle()
    {
        if (homingReticle == null) return;

        if (homingTarget != null)
        {
            homingReticle.SetActive(true);
            homingReticle.transform.position = homingTarget.reticlePosition.position;
        }
        else
        {
            homingReticle.SetActive(false);
        }
    }

    void StartHomingAttack(Vector3 targetPos)
    {
        HomingSource.Play();
        trailRenderer.emitting = true;

        animator.SetBool("BigJump", true);
        animator.SetBool("SmallJump", false);
        animator.SetBool("Airboost", false);

        isHoming = true;
        gravity.gravityEnabled = false;
        targetPosition = targetPos;
        attackTimer = 0f;
    }

    void EndHoming()
    {
        isHoming = false;
        gravity.gravityEnabled = true;

        if (homingTarget != null)
        {
            Enemy enemy = homingTarget.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.DestroySelf();
                Vector3 bounce = gravity.SurfaceNormal * enemy.BounceHeight;
                rb.linearVelocity = bounce;
            }

            homingTarget = null;
        }
    }

    void DoAirDash()
    {
        HomingSource.Play();
        trailRenderer.emitting = true;

        animator.SetBool("BigJump", true);
        animator.SetBool("SmallJump", false);
        animator.SetBool("Airboost", false);
        animator.SetTrigger("Airdashing");

        hasAirDashed = true;
        gravity.gravityEnabled = false;

        // Use playerModel.forward so dash follows the model facing, not Rigidbody rotation
        Vector3 modelForward = move != null ? move.playerModel.forward : transform.forward;
        // Project forward onto plane orthogonal to surface normal so dash follows surface orientation
        Vector3 forwardAlongSurface = Vector3.ProjectOnPlane(modelForward, gravity.SurfaceNormal).normalized;

        // Compose dash velocity: forward plus a bit of upward along surface normal for an arc
        Vector3 dashVelocity = forwardAlongSurface * airDashForce + gravity.SurfaceNormal * airDashUpForce;

        // Set velocity directly for deterministic dash behavior
        rb.linearVelocity = dashVelocity;

        Invoke(nameof(RestoreGravity), airdashLength);
    }

    void RestoreGravity()
    {
        gravity.gravityEnabled = true;
    }

}
