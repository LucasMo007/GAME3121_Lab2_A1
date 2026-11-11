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
//using UnityEngine;
//using UnityEngine.AI;

//public class EnemyAI : MonoBehaviour
//{
//    public Transform playerTransform;
//    private NavMeshAgent agent;

//    void Awake()
//    {
//        // 放在 Awake 确保即使该脚本默认处于 disabled 状态，依然能提前获取到 agent 组件
//        agent = GetComponent<NavMeshAgent>();
//    }

//    void Update()
//    {
//        if (playerTransform != null && playerTransform.gameObject.activeSelf)
//        {
//            if (agent.isStopped)
//            {
//                agent.isStopped = false;
//            }
//            agent.SetDestination(playerTransform.position);
//        }
//        else if (agent != null && agent.enabled)
//        {
//            agent.isStopped = true;
//        }
//    }

//    /// <summary>
//    /// 提供给 UI 按钮（SLOW / MEDIUM / FAST）调用的公开方法
//    /// </summary>
//    public void SetSpeed(float newSpeed)
//    {
//        if (agent == null)
//        {
//            agent = GetComponent<NavMeshAgent>();
//        }

//        if (agent != null)
//        {
//            agent.speed = newSpeed;
//        }
//    }
//}
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform playerTransform;
    private NavMeshAgent agent;
    private Animator animator;     // 敌人角色模型上的 Animator（Week 9 动画）
    private bool isChasing = true; // 控制是否允许追击

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // 角色模型是 AI 的子物体，所以从子物体里找 Animator
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 只有在允许追击、且小球存在时才寻路
        if (isChasing && playerTransform != null && playerTransform.gameObject.activeSelf)
        {
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }
            agent.SetDestination(playerTransform.position);
        }

        // 把移动速度传给 Animator：Speed > 0.1 播放 Run，否则播放 Idle
        if (animator != null && agent != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    /// <summary>
    /// 供 PlayerController 在胜利或失败时调用，彻底停止敌人
    /// </summary>
    public void StopEnemy()
    {
        isChasing = false; // 锁死追击开关

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath(); // 清空当前路径
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f); // 停下后回到 Idle
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