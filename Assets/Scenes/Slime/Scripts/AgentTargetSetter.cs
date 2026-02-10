using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentTargetSetter : MonoBehaviour
{
    public string targetName;
    private Transform target;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.Find(targetName).transform;
    }

    private void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
