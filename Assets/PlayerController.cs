using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement & Game Settings")]
    public float speed = 10f;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI winLoseText;
    public int totalPickups = 12;

    [Header("Audio Clips")]
    public AudioClip hitWallClip;       // 1. 撞墙音效 (1 point)
    public AudioClip winClip;           // 2. 胜利音效 (1 point)
    public AudioClip enemyHitClip;      // 4. 被敌人撞毁音效 (1 point)

    private Rigidbody rb;
    private int count = 0;
    private bool isGameOver = false;
    private AudioSource audioSource;

    void Awake()
    {
        // 自动获取或添加 AudioSource 组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (winLoseText != null)
        {
            winLoseText.text = "";
        }
        UpdateCountText();
    }

    void FixedUpdate()
    {
        if (isGameOver || rb == null) return;

        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D
        float moveVertical = Input.GetAxis("Vertical");     // W/S

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.AddForce(movement * speed);
    }

    void OnTriggerEnter(Collider other)
    {
        // 捡金币（兼容两种常见命名大小写）
        if ((other.gameObject.CompareTag("PickUp") || other.gameObject.CompareTag("Pickup")) && !isGameOver)
        {
            other.gameObject.SetActive(false);
            count++;
            UpdateCountText();

            if (count >= totalPickups)
            {
                WinGame();
            }
        }
        // 敌人作为触发器碰撞的情况
        else if (other.gameObject.CompareTag("Enemy") && !isGameOver)
        {
            LoseGame();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1. 撞击外围墙壁：播放撞墙声
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (audioSource != null && hitWallClip != null)
            {
                audioSource.PlayOneShot(hitWallClip);
            }
        }
        // 4. 撞击敌人：触发失败并销毁玩家
        else if (collision.gameObject.CompareTag("Enemy") && !isGameOver)
        {
            LoseGame();
        }
    }

    void UpdateCountText()
    {
        if (countText != null)
        {
            countText.text = "Count: " + count.ToString();
        }
    }

    void WinGame()
    {
        isGameOver = true;
        if (winLoseText != null)
        {
            winLoseText.text = "You Win!";
        }

        // 停止小球物理运动
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 停止敌人 AI 巡逻
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null)
        {
            UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }

        // 2. 播放胜利音效
        if (audioSource != null && winClip != null)
        {
            audioSource.PlayOneShot(winClip);
        }
    }

    void LoseGame()
    {
        isGameOver = true;
        if (winLoseText != null)
        {
            winLoseText.text = "You Lose!";
        }

        // 4. 播放敌人撞毁音效：由于自身即将 Destroy，在主摄像机位置生成独立音效播放器
        if (enemyHitClip != null)
        {
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(enemyHitClip, soundPos, 1.0f);
        }

        // 彻底销毁玩家对象，符合要求 "Player GameObject is destroyed"
        Destroy(gameObject);
    }
}
