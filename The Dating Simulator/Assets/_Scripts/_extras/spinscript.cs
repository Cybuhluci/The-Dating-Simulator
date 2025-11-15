using UnityEngine;

public class SpinScript : MonoBehaviour
{
    public float rotationSpeed = 100f; // Adjust this value for faster/slower rotation

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}