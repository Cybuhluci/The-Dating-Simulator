using Unity.Cinemachine; 
using Luci;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using Unity.Mathematics; // Required for float3 etc.

// Renamed class from PlayerCamera to MinaPlayerCamera
public class MinaPlayerCamera : MonoBehaviour
{
    // Fixed references to MinaAttributes
    MinaAttributes attributes = MinaAttributes.Instance;

    [SerializeField] Transform CameraRotatorObject; // The rig's main rotation controlled by gravity
    [SerializeField] Transform CameraPivot;       // The pivot for player input look (child of CameraRotatorObject)
    [SerializeField] MinaGravity gravity;
    [SerializeField] PlayerInput Input;
    [SerializeField] CinemachineCamera FollowCam;
    [SerializeField] GameObject Sonic;

    [Header("3D Settings")]
    [SerializeField] float sensitivityX = 3f;
    [SerializeField] float sensitivityY = 1.5f;
    [SerializeField] float minY = -80f;
    [SerializeField] float maxY = 80f;
    [SerializeField] float tiltSmooth = 8f;
    [SerializeField] Vector3 threeDOffset = new Vector3(0f, 1.5f, -5f);

    private float yaw = 0f;
    private float pitch = 0f;
    private Quaternion tilt = Quaternion.identity;

    private Quaternion rotationOffset;

    // References to Cinemachine components
    private CinemachineFollow _transposer;
    private CinemachineRotationComposer _composer;

    void Start()
    {
        attributes = MinaAttributes.Instance;
        Cursor.lockState = CursorLockMode.Locked;
        rotationOffset = Quaternion.Euler(0, 90, 0);

        _transposer = FollowCam.GetComponent<CinemachineFollow>();
        if (_transposer == null) Debug.LogError("FollowCam requires a CinemachineFollow component!", this);

        _composer = FollowCam.GetComponent<CinemachineRotationComposer>();
        if (_composer == null) Debug.LogError("FollowCam requires a CinemachineRotationComposer component!", this);

        if (CameraRotatorObject != null && gravity != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(CameraRotatorObject.forward, gravity.SurfaceNormal).normalized;
            if (forward == Vector3.zero) forward = Vector3.ProjectOnPlane(transform.forward, gravity.SurfaceNormal).normalized;
            tilt = Quaternion.LookRotation(forward, gravity.SurfaceNormal);

            Vector3 currentEuler = CameraPivot.localEulerAngles;
            pitch = currentEuler.x > 180 ? currentEuler.x - 360 : currentEuler.x;
            yaw = currentEuler.y;
        }
    }

    private void Awake()
    {

    }

    void Update()
    {
        if (CameraRotatorObject == null || CameraPivot == null || gravity == null || attributes.CameraDisabled || Sonic == null || FollowCam == null || _transposer == null || _composer == null)
            return;

        if (!attributes.IsFallingOffStage) // Only follow Sonic if not falling off stage
        {
            FollowCam.Follow = Sonic.transform;

            // Enable Cinemachine components for 3D behavior
            _transposer.enabled = true;
            _composer.enabled = true;

            // Enable and update the 3D camera rig
            CameraRotatorObject.gameObject.SetActive(true);
            CameraPivot.gameObject.SetActive(true);
            Update3DCamera();

            _transposer.FollowOffset = threeDOffset;

            // Set LookAt to CameraPivot
            FollowCam.LookAt = CameraPivot;

            FollowCam.Lens.FieldOfView = 60f;
        }
        else // Falling off stage
        {
            FollowCam.Follow = null;
            FollowCam.LookAt = null;
            _transposer.enabled = false;
            _composer.enabled = false;
            CameraRotatorObject.gameObject.SetActive(false);
            CameraPivot.gameObject.SetActive(false);
        }
    }

    // Fixed Update3DCamera to ensure pitch and yaw are applied correctly
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