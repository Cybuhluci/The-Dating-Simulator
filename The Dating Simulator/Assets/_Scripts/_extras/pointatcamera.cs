using UnityEngine;

public class pointatcamera : MonoBehaviour
{
    public Camera cam;
    public GameObject HomingReticle;

    // Update is called once per frame
    void Update()
    {
        HomingReticle.transform.forward = cam.transform.forward;
    }
}
