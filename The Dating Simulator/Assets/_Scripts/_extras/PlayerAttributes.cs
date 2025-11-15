//using Luci;
//using RagdollEngine;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class PlayerAttributes : MonoBehaviour // Player Attributes holds all states that the player can be in and more.
//{
//    public movementstate state;
//    public enum movementstate
//    {
//        idle, walking, running, fastrunning, boosting, jumping, falling, sliding, stomping, homing
//    }

//    public Rigidbody RB;
//    public float VisualSpeed;
//    public TMP_Text SpeedText;

//    [Header("behaviour tree")]

//    public Transform playerTransform;

//    public Transform modelTransform;

//    public Animator animator;

//    public GroundInformation groundInformation;

//    public LayerMask layerMask;

//    public float height;

//    public float moveDeadzone;

//    [SerializeField, Tooltip("Dynamically increase collision accuracy")] bool dynamicSolverIterations;

//    public float solverIterationDistance;

//    public List<Volume> volumes;

//    public List<StageObject> stageObjects;

//    public Character character;

//    public Transform cameraTransform => character.cameraTransform;

//    public Canvas canvas => character.canvas;

//    public InputHandler inputHandler => character.inputHandler;

//    public Vector3 additiveVelocity;

//    public Vector3 movePosition;

//    public Vector3 moveVelocity;

//    public Vector3 accelerationVector;

//    public Vector3 tangent;

//    public Vector3 plane;

//    public bool overrideModelTransform;

//    public bool kinematic;

//    public bool respawnTrigger;

//    public bool moving;

//    public bool initialized;

//    public struct GroundInformation
//    {
//        public RaycastHit hit;

//        public bool ground;

//        public bool cast;

//        public bool slope;

//        public bool enter;
//    }

//    [Header("player ground")]

//    public float maxSpeed;
//    public float baseSpeed;
//    public float acceleration;
//    [SerializeField, Range(0, 1)] public float smoothness;
//    public float uphillSlopeRatio;
//    public float downhillSlopeRatio;
//    public float maxSpeedUphillSlopeRatio;
//    public float maxSpeedDownhillSlopeRatio;

//    public bool wasMoving;

//    // Start is called before the first frame update
//    void Start()
//    {
//        RB = GetComponent<Rigidbody>();
//        //get components like rigidbody and others for movement in PlayerMovementGround script
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        VisualSpeed = Mathf.RoundToInt(RB.linearVelocity.magnitude);
//        SpeedText.text = VisualSpeed + " SPD";

//        //when idle, allow cycling between idle animations after short periods of time
//    }

//    [Header("input")]

//    [HideInInspector] public Vector2 move;

//    [HideInInspector] public Vector2 lookDelta;

//    [HideInInspector] public Vector2 look;

//    public Button start = new Button();

//    public Button select = new Button();

//    public Button jump = new Button();

//    public Button roll = new Button();

//    public Button stomp = new Button();

//    public Button boost = new Button();

//    public Button cyloop = new Button();

//    public Button attack = new Button();

//    public Button sidestep = new Button();

//    public Button zoomDelta = new Button();

//    public Button zoom = new Button();

//    public class Button
//    {
//        public bool hold => Mathf.Abs(value) >= InputSystem.settings.defaultButtonPressPoint;

//        public bool pressed => Mathf.RoundToInt(Time.unscaledTime / Time.unscaledDeltaTime) == Mathf.RoundToInt((Time.inFixedTimeStep ? fixedPressed : framePressed) / Time.unscaledDeltaTime) + 1;

//        /*public bool pressed
//        {
//            get
//            {
//                bool returning = Mathf.RoundToInt(Time.unscaledTime / Time.unscaledDeltaTime) == Mathf.RoundToInt((Time.inFixedTimeStep ? fixedPressed : framePressed) / Time.unscaledDeltaTime) + 1;

//                if (returning)
//                    print((Time.unscaledTime / Time.unscaledDeltaTime).ToString() + "; " + (Mathf.RoundToInt((Time.inFixedTimeStep ? fixedPressed : framePressed) / Time.unscaledDeltaTime) + 1).ToString());

//                return returning;
//            }
//        }*/

//        public bool released => Mathf.RoundToInt(Time.unscaledTime / Time.unscaledDeltaTime) == Mathf.RoundToInt((Time.inFixedTimeStep ? fixedReleased : frameReleased) / Time.unscaledDeltaTime) + 1;

//        public float value;

//        float framePressed = -2;

//        float fixedPressed = -2;

//        float frameReleased = -2;

//        float fixedReleased = -2;

//        public void Set(float value)
//        {
//            if (this.value == value) return;

//            this.value = value;

//            if (hold)
//            {
//                framePressed = Time.unscaledTime;

//                if (Time.timeScale > 0)
//                    fixedPressed = Time.fixedUnscaledTime;
//            }
//            else
//            {
//                frameReleased = Time.unscaledTime;

//                if (Time.timeScale > 0)
//                    fixedReleased = Time.fixedUnscaledTime;
//            }
//        }
//    }
//}
