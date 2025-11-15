using UnityEngine;

public class PropScript : MonoBehaviour, IInteractable
{
    public string itemName;  // Name of the item

    public void Interact()
    {
        //InventoryManager.Instance.AddItem(itemName);  // Add to inventory
        Destroy(gameObject);  // Remove from the scene

        if (itemName == "TARDISkey")
        {
            PlayerPrefs.SetInt("HasTardisKey", 1);
        }
    }

    public string GetInteractionType()
    {
        return "Prop";
    }
}
