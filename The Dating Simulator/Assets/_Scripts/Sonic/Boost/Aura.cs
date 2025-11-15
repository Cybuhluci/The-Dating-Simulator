using UnityEngine;

public class Aura : MonoBehaviour
{
    [SerializeField] Camera distortionCamera;

    void OnDisable()
    {
        DistortionEnd();
    }

    public void DistortionStart()
    {
        if (!distortionCamera) return;

        distortionCamera.gameObject.SetActive(true);
    }

    public void DistortionEnd()
    {
        if (!distortionCamera) return;

        distortionCamera.gameObject.SetActive(false);
    }
}
