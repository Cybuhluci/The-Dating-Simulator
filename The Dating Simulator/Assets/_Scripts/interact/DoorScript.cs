using UnityEngine;

public class DoorScript : MonoBehaviour, IInteractable
{
    public Animator doorAnimator;  // Assign the Animator in the Inspector
    public string openAnimationName = "DoorOpen";  // Unique animation name for each door

    private bool isOpen = false;

    public void Interact()
    {
        // Add door opening functionality here
        if (!isOpen)
        {
            isOpen = true;
            doorAnimator.SetBool(openAnimationName,true);
            Debug.Log("Opening door: " + gameObject.name);
        }
        else
        {
            doorAnimator.SetBool(openAnimationName,false);
        }
    }

    public string GetInteractionType()
    {
        return "Door";
    }
}