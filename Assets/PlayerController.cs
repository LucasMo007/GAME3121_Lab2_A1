using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float speed = 10f;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI winLoseText;
    public int totalPickups = 12; 

    private Rigidbody rb;
    private int count = 0;
    private bool isGameOver = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        winLoseText.text = "";
        UpdateCountText();
    }

    void FixedUpdate()
    {
        if (isGameOver) return;

        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D
        float moveVertical = Input.GetAxis("Vertical");     // W/S

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.AddForce(movement * speed);
    }

    //void OnTriggerEnter(Collider other)
    //{

    //    if (other.gameObject.CompareTag("PickUp") && !isGameOver)
    //    {
    //        other.gameObject.SetActive(false);
    //        count++;
    //        UpdateCountText();

    //        if (count >= totalPickups)
    //        {
    //            WinGame();
    //        }
    //    }
    //}

    //void OnCollisionEnter(Collision collision)
    //{

    //    if (collision.gameObject.CompareTag("Enemy") && !isGameOver)
    //    {
    //        LoseGame();
    //    }
    //}
    void OnTriggerEnter(Collider other)
    {
     
        if (other.gameObject.CompareTag("PickUp") && !isGameOver)
        {
            other.gameObject.SetActive(false);
            count++;
            UpdateCountText();

            if (count >= totalPickups)
            {
                WinGame();
            }
        }

      
        if (other.gameObject.CompareTag("Enemy") && !isGameOver)
        {
            LoseGame();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
      
        if (collision.gameObject.CompareTag("Enemy") && !isGameOver)
        {
            LoseGame();
        }
    }

    void UpdateCountText()
    {
        countText.text = "Count: " + count.ToString();
    }

    void WinGame()
    {
        isGameOver = true;
        winLoseText.text = "You Win";

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
    }

    void LoseGame()
    {
        isGameOver = true;
        winLoseText.text = "You Lose!";
        gameObject.SetActive(false); 
    }
}
