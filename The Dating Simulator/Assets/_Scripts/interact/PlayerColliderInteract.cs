using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerColliderInteract : MonoBehaviour
{
    public LayerMask interactableLayer;
    public PlayerInput playerInput;

    public float prefabheight;

    public GameObject InteractIconPrefab;
    public GameObject currentIconInstance;

    public float interactionRadius = 3f;
    public List<IInteractable> interactableList = new List<IInteractable>();
    public List<Transform> interactableTransforms = new List<Transform>();
    public int currentIndex = 0;

    public IInteractable currentInteractable;
    public Transform currentTargetTransform;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (PlayerPrefs.GetInt("CameraDisable", 0) == 1) return;

        UpdateNearbyInteractables();
        HandleInteractionInput();
        HandleScrollInput();
        UpdateInteractIcon();
    }

    void UpdateNearbyInteractables()
    {
        interactableList.Clear();
        interactableTransforms.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius, interactableLayer);

        foreach (Collider collider in hits)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactableList.Add(interactable);
                interactableTransforms.Add(collider.transform);
            }
        }

        // Ensure index is valid
        if (interactableList.Count == 0)
        {
            currentInteractable = null;
            currentTargetTransform = null;
            currentIndex = 0;

            if (currentIconInstance != null)
                Destroy(currentIconInstance);
        }
        else
        {
            // Clamp the index and set the current interactable
            currentIndex = Mathf.Clamp(currentIndex, 0, interactableList.Count - 1);
            currentInteractable = interactableList[currentIndex];
            currentTargetTransform = interactableTransforms[currentIndex];
        }
    }

    void HandleInteractionInput()
    {
        if (currentInteractable != null && playerInput.actions["Interact"].WasPressedThisFrame())
        {
            currentInteractable.Interact();
        }
    }

    void HandleScrollInput()
    {
        if (interactableList.Count <= 1)
            return;

        if (playerInput.actions["nextinteract"].WasPressedThisFrame())
        {
            currentIndex++;
            if (currentIndex >= interactableList.Count)
                currentIndex = 0;
        }

        if (playerInput.actions["previousinteract"].WasPressedThisFrame())
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = interactableList.Count - 1;
        }
    }

    void UpdateInteractIcon()
    {
        if (currentInteractable == null || currentTargetTransform == null)
            return;

        Vector3 targetPos = currentTargetTransform.position + Vector3.up * prefabheight;

        if (currentIconInstance == null)
        {
            currentIconInstance = Instantiate(InteractIconPrefab, targetPos, Quaternion.identity);
            currentIconInstance.transform.SetParent(currentTargetTransform);
        }
        else
        {
            currentIconInstance.transform.position = targetPos;
            if (currentIconInstance.transform.parent != currentTargetTransform)
                currentIconInstance.transform.SetParent(currentTargetTransform);
        }
    }
}
