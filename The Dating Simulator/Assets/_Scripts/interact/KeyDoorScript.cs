using UnityEngine;

public class KeyDoorScript : MonoBehaviour, IInteractable
{
    public string requiredKeyName; // The name of the key needed to open this door
    public Animator doorAnimator;  // Assign the Animator in the Inspector
    public string openAnimationName = "DoorOpen";  // Unique animation name for each door

    private bool isOpen = false;

    public void Interact()
    {
        Debug.Log($"Attempting to open KeyDoor. Required key: {requiredKeyName}");

        //if (InventoryManager.Instance.HasItem(requiredKeyName))  // Check if player has the required key
        //{
        //    Debug.Log($"Key {requiredKeyName} found! Opening door.");
        //    OpenDoor();
        //}
        //else
        //{
        //    Debug.Log($"Door requires key: {requiredKeyName}. Player does not have it.");
        //}
    }
    public string GetInteractionType()
    {
        return "Key Door";
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            doorAnimator.SetBool(openAnimationName, true);
            Debug.Log("Opening door: " + gameObject.name);
        }
    }
}
