using UnityEngine;
using System.Collections;
using Luci;

public class Spring2 : MonoBehaviour
{
    public enum SpringType { Push, Force }
    public SpringType springType = SpringType.Push;

    public float launchForce = 15f;

    [Header("Force Spring Settings")]
    public float length = 10f;         // How far the spring launches the player
    public float speed = 20f;          // Movement speed along the path

    [SerializeField] private AudioSource springAudio;
    [SerializeField] private Animator animator;

    private MinaAttributes attributes;

    private void Start()
    {
        attributes = MinaAttributes.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        springAudio?.Play();

        switch (springType)
        {
            case SpringType.Push:
                LaunchPush(rb);
                break;
            case SpringType.Force:
                LaunchForce(rb);
                break;
        }
    }

    void LaunchPush(Rigidbody rb)
    {
        animator.SetBool("InSpring", true);
        animator.SetTrigger("Springing");
        StartCoroutine(ResetInSpringBool());

        rb.linearVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, transform.up);
        rb.AddForce(transform.up * launchForce, ForceMode.Impulse);
    }

    void LaunchForce(Rigidbody rb)
    {
        Vector3 start = transform.position;
        Vector3 direction = transform.up.normalized;
        Vector3 end = start + direction * length;
        float duration = length / speed;

        StartCoroutine(ForceSpringRoutine(rb, start, end, duration));
    }

    private IEnumerator ForceSpringRoutine(Rigidbody rb, Vector3 start, Vector3 end, float duration)
    {
        animator.SetBool("InSpring", true);
        animator.SetTrigger("Springing");

        attributes.GravityEnabled = false;
        attributes.PlayerDisabled = true;
        rb.isKinematic = true;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 position = Vector3.Lerp(start, end, t);
            rb.MovePosition(position);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(end);
        rb.isKinematic = false;
        attributes.GravityEnabled = true;
        attributes.PlayerDisabled = false;
        animator.SetBool("InSpring", false);
    }

    private IEnumerator ResetInSpringBool()
    {
        yield return new WaitForSeconds(15f);
        if (animator != null)
            animator.SetBool("InSpring", false);
    }

    private void OnDrawGizmos()
    {
        if (springType == SpringType.Force)
        {
            Vector3 start = transform.position;
            Vector3 end = start + transform.up.normalized * length;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, 0.25f);
        }
    }
}
