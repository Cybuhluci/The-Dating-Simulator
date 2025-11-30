using UnityEngine;
using Luci;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MinaBoost : MonoBehaviour
{
    MinaAttributes attributes = MinaAttributes.Instance;

    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] MinaGravity gravity;   
    [SerializeField] GameObject Aura;
    [SerializeField] AudioSource boostSound;
    [SerializeField] MinaGroundMove move;
    [SerializeField] Image BoostMeterFill;
    [SerializeField] PlayerInput input; // boost button is "Boost", so use "input.actions[\"Boost\"]"
    [SerializeField] MinaJump jump;
    [SerializeField] MinaHomingAttack homingAttack;

    [Header("Boost Settings")]
    [SerializeField] float boostMultiplier = 3f;
    [SerializeField] float burnRate = 0.6f; // fraction of meter drained per second while boosting
    [SerializeField] float regenRate = 0.25f; // fraction of meter regained per second when not boosting
    [SerializeField] float minToStart = 0.05f; // legacy read, kept for display but not used to stop an active boost

    [Header("Burst Settings")]
    [SerializeField] float boostStartSpeed = 40f; // one-time burst added to velocity when boost starts
    [SerializeField] float boostMaxSpeed = 300f; // don't apply start burst if current speed >= this

    [Header("Refill Settings")]
    [SerializeField] float refillDelay = 1.5f; // seconds to wait after boost ends before regen

    [Header("Start Cost")]
    [SerializeField] float startCost = 0.5f; // cost deducted immediately when starting boost (use 0.5 per spec)

    [Header("Air Boost Settings")]
    [SerializeField] float airBoostUpForce = 12f; // upward component when air-boosting
    [SerializeField] float airBoostForwardForce = 30f; // forward component when air-boosting
    [SerializeField] float airBoostCost = 0.25f; // fraction of meter consumed when performing air-boost

    float boostMeter = 1f; // 0..1
    bool isBoosting = false; // ground boost active
    bool boostSoundHasPlayedForBoost = false;

    // refill timer counts down after boost stops or meter emptied
    float refillTimer = 0f;

    // air boost runtime flag
    bool hasAirBoosted = false;

    private void Start()
    {
        attributes = MinaAttributes.Instance;
        // initialize UI
        if (BoostMeterFill != null) BoostMeterFill.fillAmount = boostMeter;
        if (Aura != null) Aura.SetActive(false);
    }

    void Update()
    {
        if (attributes.PlayerDisabled) return;
        if (input == null) return;

        bool wantBoost = input.actions["Boost"].IsPressed();

        // Air boost trigger: one-shot when pressing boost in air
        if (!attributes.IsGrounded && input.actions["Boost"].WasPressedThisFrame())
        {
            if (!hasAirBoosted && boostMeter >= airBoostCost)
            {
                DoAirBoost();
            }
        }

        if (attributes.IsGrounded)
        {
            // Reset air-boost availability when grounded
            if (hasAirBoosted)
            {
                hasAirBoosted = false;
                jump.inAirBoost = false;
                homingAttack.inAirBoost = false;
                if (Aura != null) Aura.SetActive(false);
                if (animator != null) animator.SetBool("Airboost", false);
            }

            // Ground boost logic: allow starting only when grounded
            if (wantBoost && (isBoosting || boostMeter >= startCost))
            {
                // Start boost if not already
                if (!isBoosting) StartBoost();

                // Drain meter
                boostMeter -= burnRate * Time.deltaTime;

                // allow meter to reach exact zero
                if (boostMeter <= 0f)
                {
                    boostMeter = 0f;
                    // stop boosting and start refill delay
                    StopBoost();
                    refillTimer = refillDelay;
                }
                else
                {
                    // while actively boosting, reset refill timer so regen doesn't start
                    refillTimer = refillDelay;
                }
            }
            else
            {
                // Not boosting (either because wantBoost false or meter too low)
                if (isBoosting) StopBoost();

                // Only start refilling after the delay and if boost button is not held
                if (!wantBoost)
                {
                    if (refillTimer > 0f)
                    {
                        refillTimer -= Time.deltaTime;
                    }
                    else
                    {
                        boostMeter = Mathf.Min(1f, boostMeter + regenRate * Time.deltaTime);
                    }
                }
                else
                {
                    // player is holding boost but meter is < startCost: keep refillTimer ticking down
                    if (refillTimer > 0f)
                        refillTimer -= Time.deltaTime;
                }
            }
        }
        else
        {
            // Air state: ensure ground boost is not active
            if (isBoosting) StopBoost();

            // While airborne, do not start ground boost; only handle refill timing
            if (!wantBoost)
            {
                if (refillTimer > 0f)
                {
                    refillTimer -= Time.deltaTime;
                }
                else
                {
                    boostMeter = Mathf.Min(1f, boostMeter + regenRate * Time.deltaTime);
                }
            }
            else
            {
                // player holding boost in air and meter low: keep refill timer ticking
                if (refillTimer > 0f)
                    refillTimer -= Time.deltaTime;
            }
        }

        // clamp tiny floating residues to zero for clean UI
        if (boostMeter > 0f && boostMeter < 0.000001f) boostMeter = 0f;

        UpdateBoostMeter(boostMeter);
    }

    void StartBoost()
    {
        isBoosting = true;
        if (Aura != null) Aura.SetActive(true);

        // initial cost to start boost (use configured startCost)
        boostMeter = Mathf.Max(0f, boostMeter - startCost);

        // mark the sound as played before playing to avoid re-entrancy issues
        if (!boostSoundHasPlayedForBoost)
        {
            boostSoundHasPlayedForBoost = true;
            if (boostSound != null && !boostSound.isPlaying)
                boostSound.Play();
        }

        if (animator != null) animator.SetBool("Boosting", true);
        if (move != null) move.BoostMult = boostMultiplier;

        // One-time burst: add to player's linear velocity in the model's facing direction
        // Only apply the burst if current horizontal speed is below boostMaxSpeed
        if (move != null && move.rb != null)
        {
            // use horizontal speed (projected onto world up plane) to avoid vertical components affecting the check
            Vector3 horizontalVel = Vector3.ProjectOnPlane(move.rb.linearVelocity, Vector3.up);
            float currentSpeed = horizontalVel.magnitude;
            if (currentSpeed < boostMaxSpeed)
            {
                Vector3 modelForward = move.playerModel != null ? move.playerModel.forward : transform.forward;
                Vector3 forwardPlanar = Vector3.ProjectOnPlane(modelForward, Vector3.up);
                if (forwardPlanar.sqrMagnitude > 0.0001f)
                    forwardPlanar.Normalize();
                else
                    forwardPlanar = modelForward.normalized;

                move.rb.linearVelocity = move.rb.linearVelocity + forwardPlanar * boostStartSpeed;
            }
        }

        // reset refill timer while boosting
        refillTimer = refillDelay;
    }

    void StopBoost()
    {
        isBoosting = false;
        if (Aura != null) Aura.SetActive(false);
        if (animator != null) animator.SetBool("Boosting", false);
        if (move != null) move.BoostMult = 1f;
        boostSoundHasPlayedForBoost = false;

        // start refill timer when boost stops
        refillTimer = refillDelay;
    }

    void DoAirBoost()
    {
        if (move == null || move.rb == null) return;

        // consume meter
        boostMeter = Mathf.Max(0f, boostMeter - airBoostCost);
        refillTimer = refillDelay;
        hasAirBoosted = true;
        jump.inAirBoost = true;
        homingAttack.inAirBoost = true;
        if (Aura != null) Aura.SetActive(true);

        boostSoundHasPlayedForBoost = false;
        // mark the sound as played before playing to avoid re-entrancy issues
        if (!boostSoundHasPlayedForBoost)
        {
            boostSoundHasPlayedForBoost = true;
            if (boostSound != null && !boostSound.isPlaying)
                boostSound.Play();
        }

        // trigger animation
        if (animator != null)
        {
            animator.SetBool("Airboost", true);
        }

        // compute direction and apply velocity
        Vector3 modelForward = move.playerModel != null ? move.playerModel.forward : transform.forward;
        Vector3 forwardAlongSurface = Vector3.ProjectOnPlane(modelForward, gravity.SurfaceNormal).normalized;
        Vector3 launch = forwardAlongSurface * airBoostForwardForce + gravity.SurfaceNormal * airBoostUpForce;

        //move.rb.linearVelocity = launch;
        move.rb.AddForce(launch, ForceMode.Impulse);
    }

    public void UpdateBoostMeter(float value) // works with values 0 to 1.
    {
        if (BoostMeterFill != null) BoostMeterFill.fillAmount = Mathf.Clamp01(value);
    }
}
