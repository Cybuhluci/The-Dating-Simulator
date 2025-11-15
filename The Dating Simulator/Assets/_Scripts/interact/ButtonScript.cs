using UnityEngine;
using UnityEngine.Events;

public class ButtonScript : MonoBehaviour, IInteractable
{
    public UnityEvent onButtonPressed;  // Drag and drop events in the Inspector

    public void Interact()
    {
        Debug.Log("Button pressed!");
        onButtonPressed.Invoke();  // Calls whatever is assigned in the Inspector
        // Add button functionality here
    }

    public string GetInteractionType()
    {
        return "Button"; // Used to set the correct crosshair
    }
}
