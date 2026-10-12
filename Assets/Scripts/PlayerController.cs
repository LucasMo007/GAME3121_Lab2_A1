//using UnityEngine;
//using TMPro;

//public class PlayerController : MonoBehaviour
//{
//    [Header("Movement & Game Settings")]
//    public float speed = 10f;
//    public TextMeshProUGUI countText;
//    public TextMeshProUGUI winLoseText;
//    public int totalPickups = 12;

//    [Header("Audio Clips")]
//    public AudioClip hitWallClip;       // 1. 撞墙音效 (1 point)
//    public AudioClip winClip;           // 2. 胜利音效 (1 point)
//    public AudioClip enemyHitClip;      // 4. 被敌人撞毁音效 (1 point)

//    private Rigidbody rb;
//    private int count = 0;
//    private bool isGameOver = false;
//    private AudioSource audioSource;

//    void Awake()
//    {
//        // 自动获取或添加 AudioSource 组件
//        audioSource = GetComponent<AudioSource>();
//        if (audioSource == null)
//        {
//            audioSource = gameObject.AddComponent<AudioSource>();
//        }
//        audioSource.playOnAwake = false;
//    }

//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        if (winLoseText != null)
//        {
//            winLoseText.text = "";
//        }
//        UpdateCountText();
//    }

//    void FixedUpdate()
//    {
//        if (isGameOver || rb == null) return;

//        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D
//        float moveVertical = Input.GetAxis("Vertical");     // W/S

//        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
//        rb.AddForce(movement * speed);
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        // 捡金币（兼容两种常见命名大小写）
//        if ((other.gameObject.CompareTag("PickUp") || other.gameObject.CompareTag("Pickup")) && !isGameOver)
//        {
//            other.gameObject.SetActive(false);
//            count++;
//            UpdateCountText();

//            if (count >= totalPickups)
//            {
//                WinGame();
//            }
//        }
//        // 敌人作为触发器碰撞的情况
//        else if (other.gameObject.CompareTag("Enemy") && !isGameOver)
//        {
//            LoseGame();
//        }
//    }

//    void OnCollisionEnter(Collision collision)
//    {
//        // 1. 撞击外围墙壁：播放撞墙声
//        if (collision.gameObject.CompareTag("Wall"))
//        {
//            if (audioSource != null && hitWallClip != null)
//            {
//                audioSource.PlayOneShot(hitWallClip);
//            }
//        }
//        // 4. 撞击敌人：触发失败并销毁玩家
//        else if (collision.gameObject.CompareTag("Enemy") && !isGameOver)
//        {
//            LoseGame();
//        }
//    }

//    void UpdateCountText()
//    {
//        if (countText != null)
//        {
//            countText.text = "Count: " + count.ToString();
//        }
//    }

//    void WinGame()
//    {
//        isGameOver = true;
//        if (winLoseText != null)
//        {
//            winLoseText.text = "You Win!";
//        }

//        // 停止小球物理运动
//        if (rb != null)
//        {
//            rb.linearVelocity = Vector3.zero;
//            rb.angularVelocity = Vector3.zero;
//        }

//        // 停止敌人 AI 巡逻
//        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
//        if (enemy != null)
//        {
//            UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
//            if (agent != null)
//            {
//                agent.isStopped = true;
//                agent.velocity = Vector3.zero;
//            }
//        }

//        // 2. 播放胜利音效
//        if (audioSource != null && winClip != null)
//        {
//            audioSource.PlayOneShot(winClip);
//        }
//    }

//    void LoseGame()
//    {
//        isGameOver = true;
//        if (winLoseText != null)
//        {
//            winLoseText.text = "You Lose!";
//        }

//        // 4. 播放敌人撞毁音效：由于自身即将 Destroy，在主摄像机位置生成独立音效播放器
//        if (enemyHitClip != null)
//        {
//            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : transform.position;
//            AudioSource.PlayClipAtPoint(enemyHitClip, soundPos, 1.0f);
//        }

//        // 彻底销毁玩家对象，符合要求 "Player GameObject is destroyed"
//        Destroy(gameObject);
//    }
//}
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

    [Header("Visual Effects (Lab 5)")]
    public ParticleSystem dustTrail;           // 移动拖尾尘土 (挂在小球底部的子物体)
    public GameObject pickupEffectPrefab;      // 拾取黄色方块粒子预制体
    public GameObject explosionEffectPrefab;   // 撞到敌人爆炸粒子预制体
    public GameObject winCelebrationPrefab;    // 胜利庆祝粒子预制体

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

        // 初始确保拖尾粒子停止发射
        if (dustTrail != null && dustTrail.isPlaying)
        {
            dustTrail.Stop();
        }
    }

    void FixedUpdate()
    {
        if (isGameOver || rb == null) return;

        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D
        float moveVertical = Input.GetAxis("Vertical");     // W/S

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.AddForce(movement * speed);
    }

    void Update()
    {
        if (isGameOver || rb == null) return;

        // --- Lab 5 要求 4: 仅在小球运动时激活拖尾，并根据运动方向动态调整朝向 ---
        if (dustTrail != null)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            // 当速度大于阈值时激活
            if (horizontalVelocity.magnitude > 0.15f)
            {
                if (!dustTrail.isPlaying)
                {
                    dustTrail.Play();
                }

                // 让拖尾发射方向朝向小球运动的反方向（喷在身后）
                dustTrail.transform.rotation = Quaternion.LookRotation(-horizontalVelocity.normalized);
            }
            else
            {
                // 静止时停止发射
                if (dustTrail.isPlaying)
                {
                    dustTrail.Stop();
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 捡金币（兼容两种常见命名大小写）
        if ((other.gameObject.CompareTag("PickUp") || other.gameObject.CompareTag("Pickup")) && !isGameOver)
        {
            // --- Lab 5 要求 1: 触发拾取金币粒子特效 ---
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, other.transform.position, Quaternion.identity);
            }

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

        // 停止拖尾
        if (dustTrail != null && dustTrail.isPlaying)
        {
            dustTrail.Stop();
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

        // --- Lab 5 要求 3: 触发胜利庆祝特效 ---
        if (winCelebrationPrefab != null)
        {
            Instantiate(winCelebrationPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        }
    }

    void LoseGame()
    {
        isGameOver = true;
        if (winLoseText != null)
        {
            winLoseText.text = "You Lose!";
        }

        // --- Lab 5 要求 1: 玩家被抓到时生成爆炸特效 ---
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
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