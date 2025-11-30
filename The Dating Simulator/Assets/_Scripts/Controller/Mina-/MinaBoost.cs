using UnityEngine;
using Luci;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MinaBoost : MonoBehaviour
{
    MinaAttributes attributes = MinaAttributes.Instance;

    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] GameObject Aura;
    [SerializeField] AudioSource boostSound;
    [SerializeField] MinaGroundMove move;
    [SerializeField] Image BoostMeterFill;
    [SerializeField] PlayerInput input; // boost button is "Boost", so use "input.actions["Boost"]"

    [Header("Boost Settings")]
    [SerializeField] float boostMultiplier = 3f;
    [SerializeField] float burnRate = 0.6f; // fraction of meter drained per second while boosting
    [SerializeField] float regenRate = 0.25f; // fraction of meter regained per second when not boosting
    [SerializeField] float minToStart = 0.05f; // minimum meter required to start boosting

    float boostMeter = 1f; //0..1
    bool isBoosting = false;
    bool boostSoundHasPlayedForBoost = false;

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

        if (wantBoost && boostMeter > minToStart)
        {
            // Start boost if not already
            if (!isBoosting) StartBoost();

            // Drain meter
            boostMeter -= burnRate * Time.deltaTime;
            if (boostMeter <= 0f)
            {
                boostMeter = 0f;
                StopBoost();
            }
        }
        else
        {
            // Regen when not boosting
            if (isBoosting) StopBoost();
            if (!wantBoost) // only refills when boost button not held
            {
                boostMeter = Mathf.Min(1f, boostMeter + regenRate * Time.deltaTime);
            }
        }

        UpdateBoostMeter(boostMeter);
    }

    void StartBoost()
    {
        isBoosting = true;
        if (Aura != null) Aura.SetActive(true);
        if (!boostSoundHasPlayedForBoost) 
        {
            boostSound.Play();
            boostSoundHasPlayedForBoost = true;
        }
        if (animator != null) animator.SetBool("Boosting", true);
        if (move != null) move.BoostMult = boostMultiplier;
    }

    void StopBoost()
    {
        isBoosting = false;
        if (Aura != null) Aura.SetActive(false);
        if (animator != null) animator.SetBool("Boosting", false);
        if (move != null) move.BoostMult = 1f;
        boostSoundHasPlayedForBoost = false;
    }

    public void UpdateBoostMeter(float value) // works with values 0 to1.
    {
        if (BoostMeterFill != null) BoostMeterFill.fillAmount = Mathf.Clamp01(value);
    }
}
