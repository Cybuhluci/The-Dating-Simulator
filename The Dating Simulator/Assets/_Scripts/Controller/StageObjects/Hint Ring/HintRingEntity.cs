using Luci;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HintRingEntity : StageObject
{
    [SerializeField] private string[] hintKeyboard;
    [SerializeField] private string[] hintXbox;
    [SerializeField] private string[] hintPS;

    private string[] hint; // Active hint based on input
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private GameObject hintTextPrefab; // Prefab for hint display

    [HideInInspector] public bool touched;

    private void Start()
    {
        UpdateHintBasedOnInput();
    }

    private void Update()
    {
        DetectInputChange(); // Continuously check for device changes
    }

    private void OnTriggerEnter(Collider other)
    {
        if (touched) return;

        Touch();
    }

    public void Touch()
    {
        if (touched) return;

        touched = true;

        animator.SetTrigger("Touched");
        audioSource.Play();

        DisplayHint();
    }

    private void DetectInputChange()
    {
        if (Gamepad.current != null)
        {
            string deviceName = Gamepad.current.name.ToLower();

            if (deviceName.Contains("xbox") || deviceName.Contains("microsoft"))
            {
                hint = hintXbox;
            }
            else if (deviceName.Contains("dualshock") || deviceName.Contains("dualsense") || deviceName.Contains("playstation") || deviceName.Contains("sony"))
            {
                hint = hintPS;
            }
            else
            {
                hint = hintXbox; // Default to Xbox layout if unknown
            }
        }
        else
        {
            hint = hintKeyboard;
        }
    }

    private void UpdateHintBasedOnInput()
    {
        DetectInputChange(); // Ensure correct hints on startup
    }

    private void DisplayHint()
    {
        if (hint.Length > 0 && hintTextPrefab != null)
        {
            GameObject hintObject = Instantiate(hintTextPrefab); // Spawn hint UI
            TMP_Text hintText = hintObject.GetComponentInChildren<TMP_Text>();

            hintText.text = hint[0]; // Set hint text
            Destroy(hintObject, 3f); // Destroy after 3 seconds
            
        }
    }

    public void Destroy()
    {
        Destroy(gameObject, 3.5f);
    }
}
