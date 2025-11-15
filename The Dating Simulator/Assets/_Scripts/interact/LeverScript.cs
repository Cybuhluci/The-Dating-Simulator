using UnityEngine;

public class LeverScript : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Lever pulled!");
    }

    public string GetInteractionType()
    {
        return "Lever";
    }
}