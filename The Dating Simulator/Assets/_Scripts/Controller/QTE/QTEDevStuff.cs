using UnityEngine;
using UnityEngine.InputSystem;

public class QTEDevStuff : MonoBehaviour
{
    public Transform Player;
    public PlayerInput playerInput;
    public Transform Location;

    // Update is called once per frame
    void Update()
    {
        if (playerInput.actions["Zoom"].WasPerformedThisFrame())
        {
            Player.position = Location.position;
            Player.rotation = Location.rotation;
        }
    }
}
