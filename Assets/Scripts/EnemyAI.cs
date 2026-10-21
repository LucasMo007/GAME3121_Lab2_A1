//using UnityEngine;
//using UnityEngine.AI;

//public class EnemyAI : MonoBehaviour
//{
//    public Transform playerTransform;
//    private NavMeshAgent agent;

//    void Start()
//    {
//        agent = GetComponent<NavMeshAgent>();
//    }

//    void Update()
//    {

//        if (playerTransform != null && playerTransform.gameObject.activeSelf)
//        {
//            agent.SetDestination(playerTransform.position);
//        }
//        else if (agent.enabled)
//        {
//            agent.isStopped = true;
//        }
//    }
//}
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform playerTransform;
    private NavMeshAgent agent;

    void Awake()
    {
        // 放在 Awake 确保即使该脚本默认处于 disabled 状态，依然能提前获取到 agent 组件
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (playerTransform != null && playerTransform.gameObject.activeSelf)
        {
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }
            agent.SetDestination(playerTransform.position);
        }
        else if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
        }
    }

    /// <summary>
    /// 提供给 UI 按钮（SLOW / MEDIUM / FAST）调用的公开方法
    /// </summary>
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
