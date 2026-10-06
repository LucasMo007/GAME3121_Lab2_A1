using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform playerTransform;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
    
        if (playerTransform != null && playerTransform.gameObject.activeSelf)
        {
            agent.SetDestination(playerTransform.position);
        }
        else if (agent.enabled)
        {
            agent.isStopped = true;
        }
    }
}
