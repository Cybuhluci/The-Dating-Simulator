using UnityEngine;

public class BasicDoorScript : MonoBehaviour, IInteractable
{
    public Transform doorTransform;  // The part of the door to move/rotate
    public Vector3 moveAmount;  // How much to move the door
    public Vector3 rotateAmount;  // How much to rotate the door
    public float speed = 2f;  // Speed of movement/rotation

    private bool isOpen = false;
    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        if (doorTransform == null)
            doorTransform = transform;

        startPos = doorTransform.position;
        startRot = doorTransform.rotation;
    }

    public void Interact()
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
    }

    public string GetInteractionType()
    {
        return "Basic Door";
    }
}
