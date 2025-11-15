using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    void Interact();
    string GetInteractionType(); // Returns a string that tells us what kind of object this is
}

public class PlayerLookInteract : MonoBehaviour
{
    public Camera playerCamera;
    public float raycastDistance = 5f;
    public LayerMask interactableLayer;
    public PlayerInput playerInput;

    public GameObject crosshairDefault;
    public GameObject crosshairPress;
    public GameObject crosshairOpenHand;
    public GameObject crosshairCloseHand;
    public GameObject crosshairKey;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        ResetCrosshairs();
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, raycastDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                SetCrosshair(interactable.GetInteractionType());

                if (playerInput.actions["Interact"].WasPressedThisFrame())
                {
                    interactable.Interact();
                }
            }
            else
            {
                SetCrosshair("Normal");
            }
        }
        else
        {
            SetCrosshair("Normal");
        }
    }

    void SetCrosshair(string interactionType)
    {
        ResetCrosshairs();

        switch (interactionType)
        {
            case "Normal":
                crosshairDefault.SetActive(true);
                break;
            case "Door":
                crosshairOpenHand.SetActive(true);
                break;
            case "Key Door":
                crosshairKey.SetActive(true);
                break;
            case "Basic Door":
                crosshairOpenHand.SetActive(true);
                break;
            case "Basic Key Door":
                crosshairKey.SetActive(true);
                break;
            case "Button":
                crosshairPress.SetActive(true);
                break;
            case "Prop":
                crosshairCloseHand.SetActive(true);
                break;
            case "Tardis Door":
                crosshairOpenHand.SetActive(true);
                break;
            case "Tardis Lock":
                crosshairKey.SetActive(true);
                break;
             case "Collectible":
                crosshairKey.SetActive(true);
                break;
             case "Lever":
                crosshairKey.SetActive(true);
                break;
            default:
                crosshairDefault.SetActive(true);
                break;
        }
    }

    //void ShowCrosshair(GameObject crosshair)
    //{
    //    ResetCrosshairs();
    //    crosshair.SetActive(true);
    //}

    void ResetCrosshairs()
    {
        crosshairDefault.SetActive(false);
        crosshairPress.SetActive(false);
        crosshairOpenHand.SetActive(false);
        crosshairCloseHand.SetActive(false);
        crosshairKey.SetActive(false);
    }
}
