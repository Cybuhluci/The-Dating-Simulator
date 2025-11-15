using Luci;
using UnityEngine;

public class PlayerGuidedSpring : MonoBehaviour
{
    MinaAttributes attributes;
    [SerializeField] Animator animator;

    private Transform[] path;
    private float speed;
    private int index = 0;
    private bool active = false;

    private void Start()
    {
        attributes = MinaAttributes.Instance;
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!active || path == null || index >= path.Length) return;

        Transform target = path[index];
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
            index++;

        if (index >= path.Length)
            EndSpring();
    }

    public void StartGuidedSpring(Transform[] newPath, float travelSpeed)
    {
        path = newPath;
        speed = travelSpeed;
        index = 0;
        active = true;

        animator.SetBool("InSpring", true);
        animator.SetTrigger("Springing");

        attributes.GravityEnabled = false;
        attributes.PlayerDisabled = true;
    }

    void EndSpring()
    {
        active = false;

        animator.SetBool("InSpring", false);

        attributes.PlayerDisabled = false;
        attributes.GravityEnabled = true;
    }
}

