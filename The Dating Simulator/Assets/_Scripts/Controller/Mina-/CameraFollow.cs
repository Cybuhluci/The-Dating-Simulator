using Unity.Cinemachine;
using Luci;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class MinaCameraFollow : MonoBehaviour
{
    MinaAttributes attributes = MinaAttributes.Instance;

    [SerializeField] Transform CameraRotatorObject;
    [SerializeField] Transform CameraPivot;
    [SerializeField] MinaGravity gravity;
    [SerializeField] PlayerInput Input;
    [SerializeField] GameObject Sonic;

    [Header("3D Settings")]
    [SerializeField] float sensitivityX = 3f;
    [SerializeField] float sensitivityY = 1.5f;
    [SerializeField] float minY = -80f;
    [SerializeField] float maxY = 80f;
    [SerializeField] float tiltSmooth = 8f;

    private float yaw = 0f;
    private float pitch = 0f;
    private Quaternion tilt = Quaternion.identity;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        attributes = MinaAttributes.Instance;
        Cursor.lockState = CursorLockMode.Locked;
        tilt = Quaternion.LookRotation(CameraRotatorObject.forward, gravity.SurfaceNormal);
    }

    // Update is called once per frame
    void Update()
    {
        Update3DCamera();
    }

    void Update3DCamera()
    {
        Vector2 look = Input.actions["Look"].ReadValue<Vector2>();
        if (look.sqrMagnitude < 0.0001f) look = Vector2.zero;

        yaw += look.x * sensitivityX * Time.deltaTime;
        pitch -= look.y * sensitivityY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minY, maxY);

        Vector3 normal = gravity.SurfaceNormal;
        Vector3 forward = Vector3.ProjectOnPlane(CameraRotatorObject.forward, normal).normalized;
        if (forward == Vector3.zero)
            forward = Vector3.ProjectOnPlane(transform.forward, normal).normalized;

        Quaternion targetTilt = Quaternion.LookRotation(forward, normal);
        tilt = Quaternion.Slerp(tilt, targetTilt, Time.deltaTime * tiltSmooth);

        CameraRotatorObject.rotation = tilt;
        CameraPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
