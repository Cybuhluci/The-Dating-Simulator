using UnityEngine;

public class BasicKeyDoorScript : MonoBehaviour, IInteractable
{
    public Transform doorTransform;
    public Vector3 moveAmount;
    public Vector3 rotateAmount;
    public float speed = 2f;

    private bool isOpen = false;
    private Vector3 startPos;
    private Quaternion startRot;

    public bool requiresKey = true;
    public bool singleUseKey = false;
    public string requiredKeyName;

    void Start()
    {
        if (doorTransform == null)
            doorTransform = transform;

        startPos = doorTransform.position;
        startRot = doorTransform.rotation;
    }

    public void Interact()
    {
        //if (requiresKey)
        //{
        //    if (InventoryManager.Instance.HasItem(requiredKeyName))  // Check if player has the key
        //    {
        //        Debug.Log("Key found: Opening Key Door.");
        //        OpenDoor();
        //        if (singleUseKey)
        //        {
        //            InventoryManager.Instance.RemoveItem(requiredKeyName);
        //        }
        //    }
        //    else
        //    {
        //        Debug.Log("Door requires key: " + requiredKeyName);
        //    }
        //}
        //else
        //{
        //    OpenDoor();
        //}
    }

    public string GetInteractionType()
    {
        return "Basic Key Door";
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            StartCoroutine(MoveAndRotate(doorTransform.position + moveAmount, doorTransform.rotation * Quaternion.Euler(rotateAmount)));
        }
    }

    private System.Collections.IEnumerator MoveAndRotate(Vector3 targetPos, Quaternion targetRot)
    {
        float elapsedTime = 0;
        Vector3 initialPos = doorTransform.position;
        Quaternion initialRot = doorTransform.rotation;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * speed;
            doorTransform.position = Vector3.Lerp(initialPos, targetPos, elapsedTime);
            doorTransform.rotation = Quaternion.Slerp(initialRot, targetRot, elapsedTime);
            yield return null;
        }

        doorTransform.position = targetPos;
        doorTransform.rotation = targetRot;
        Destroy(gameObject);
    }
}