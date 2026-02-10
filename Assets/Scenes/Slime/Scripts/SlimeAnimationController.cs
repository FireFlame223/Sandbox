using UnityEngine;
using UnityEngine.AI;

public class SlimeAnimatorController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    private void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent not found on " + gameObject.name);
        }
        if (animator == null)
        {
            Debug.LogError("Animator not found on " + gameObject.name);
        }
    }

    private void Update()
    {
        if (agent != null && animator != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("speed", speed);
        }
    }
}