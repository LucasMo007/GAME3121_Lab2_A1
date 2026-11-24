
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform playerTransform;
    private NavMeshAgent agent;
    private Animator animator;     
    private bool isChasing = true; 

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
       
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
      
        if (isChasing && playerTransform != null && playerTransform.gameObject.activeSelf)
        {
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }
            agent.SetDestination(playerTransform.position);
        }

      
        if (animator != null && agent != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

   
    public void StopEnemy()
    {
        isChasing = false; 

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath(); 
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f); 
        }
    }

   
    public void SetSpeed(float newSpeed)
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent != null)
        {
            agent.speed = newSpeed;
        }
    }
}