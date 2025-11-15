using Luci;
using UnityEngine;


public class MinaIdleAnims : MonoBehaviour
{
    MinaAttributes attributes = MinaAttributes.Instance;

    [SerializeField] Animator animator;
    [SerializeField] int idletimecounter = 0;

    private float idleTimer = 0f;
    private const float tickRate = 1f; // Every second

    void Start()
    {
        attributes = MinaAttributes.Instance;
    }

    void Update()
    {
        if (animator.GetFloat("Speed") < 1)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= tickRate)
            {
                idleTimer = 0f;
                idletimecounter++;
                animator.SetInteger("idletime", idletimecounter);
            }
        }
        else
        {
            idleTimer = 0f;
            idletimecounter = 0;
            animator.SetInteger("idletime", 0);
        }
    }
}