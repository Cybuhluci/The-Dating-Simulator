using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Luci
{
    public class GoalRing : MonoBehaviour
    {
        MinaAttributes attributes = MinaAttributes.Instance;
        [SerializeField] ResultsScreen Results;
        [SerializeField] Transform Sonic;
        [SerializeField] Animator animator;
        [SerializeField] Rigidbody rb;

        [Header("Goal Ring Settings")]
        [SerializeField] private GameObject ringModel;
        [SerializeField] private Transform SonicResultsLocation;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip idleLoop;
        [SerializeField] private AudioClip touchedSound;
        [SerializeField] private string sceneToLoad;
        [SerializeField] private float shrinkSpeed = 0.1f;

        private bool isTriggered = false;

        void Start()
        {
            attributes = MinaAttributes.Instance;

            if (audioSource != null && idleLoop != null)
            {
                audioSource.clip = idleLoop;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            Sonic = other.transform;
            animator = other.GetComponentInChildren<MinaAnimations>().animator;
            rb = other.GetComponent<Rigidbody>();

            rb.linearVelocity = Vector3.zero;

            attributes.PlayerDisabled = true;
            attributes.CameraDisabled = true;

            if (isTriggered) return;
            StartCoroutine(GoalSequence());

            isTriggered = true;
        }

        private IEnumerator GoalSequence()
        {
            attributes.InResults = true;

            if (audioSource != null && touchedSound != null)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(touchedSound);
            }

            yield return StartCoroutine(ShrinkRing());

            Sonic.localPosition = SonicResultsLocation.position;
            Sonic.localRotation = SonicResultsLocation.rotation;
            Sonic.localPosition = SonicResultsLocation.position;
            Sonic.localRotation = SonicResultsLocation.rotation;

            animator.SetInteger("RESULT", Results.CalculateRank());
            animator.SetTrigger("Results");

            Results.StartResults();
        }

        private IEnumerator ShrinkRing()
        {
            while (ringModel.transform.localScale.x > 0.01f)
            {
                ringModel.transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;
                yield return null;
            }
        }
    }
}
