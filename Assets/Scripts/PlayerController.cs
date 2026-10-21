
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
    public AudioClip hitWallClip;       
    public AudioClip winClip;           
    public AudioClip enemyHitClip;      

    [Header("Visual Effects (Lab 5)")]
    public ParticleSystem dustTrail;           
    public GameObject pickupEffectPrefab;      
    public GameObject explosionEffectPrefab;   
    public GameObject winCelebrationPrefab;    

    private Rigidbody rb;
    private int count = 0;
    private bool isGameOver = false;
    private AudioSource audioSource;

    void Awake()
    {
       
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

      
        if (dustTrail != null)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

           
            if (horizontalVelocity.magnitude > 0.15f)
            {
                if (!dustTrail.isPlaying)
                {
                    dustTrail.Play();
                }

            
                dustTrail.transform.rotation = Quaternion.LookRotation(-horizontalVelocity.normalized);
            }
            else
            {
             
                if (dustTrail.isPlaying)
                {
                    dustTrail.Stop();
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
     
        if ((other.gameObject.CompareTag("PickUp") || other.gameObject.CompareTag("Pickup")) && !isGameOver)
        {
       
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
     
        else if (other.gameObject.CompareTag("Enemy") && !isGameOver)
        {
            LoseGame();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (audioSource != null && hitWallClip != null)
            {
                audioSource.PlayOneShot(hitWallClip);
            }
        }
     
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

       
        if (dustTrail != null && dustTrail.isPlaying)
        {
            dustTrail.Stop();
        }

        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

  
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

      
        if (audioSource != null && winClip != null)
        {
            audioSource.PlayOneShot(winClip);
        }

      
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

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

    
        if (enemyHitClip != null)
        {
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(enemyHitClip, soundPos, 1.0f);
        }

       
        Destroy(gameObject);
    }
}