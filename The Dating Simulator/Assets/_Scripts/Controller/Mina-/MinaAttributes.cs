using UnityEngine;

namespace Luci
{
    public class MinaAttributes : MonoBehaviour
    {
        [Header("REALLY IMPORTANT STUFF")]
        public int score;
        public int time;
        public int lives;
        public bool HasDiedOnceInLevel;

        [Header("Important things to worry about")]
        public bool GravityEnabled = true;

        [Space(5)]
        public bool InResults;

        [Space(5)]
        public bool InTimerEvent;
        public bool InQTE;
        public bool InChaosControl;

        [Space(5)]
        public bool PlayerDisabled;
        public bool CameraDisabled;
        public bool PlayerAndCameraDisabled;

        public bool IsGrounded;
        public bool IsFallingOffStage;

        [Header("Less.. Important things..")]

        public bool Drowning;
        public enum Music { Main, Underwater}
        public Music music;

        public static MinaAttributes Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void SetSpecialTimerEvent(bool Active, string Type)
        {
            InTimerEvent = Active;

            if (Type == "QTE")
            {
                InQTE = Active;
            }
            else if (Type == "Chaos")
            {
                InChaosControl = Active;
            }
            else
            {
                Debug.LogWarning($"Dumbass can't spell, {Type}");
            }
        }

        private void LateUpdate()
        {
            PlayerAndCameraDisabled = PlayerDisabled && CameraDisabled;
        }
    }
}