using UnityEngine;

public class PlayerMovementControl: MonoBehaviour
{
    public float moveSpeed = 5f; 

    private Animator animator; 
    bool isRunning = false; 
    private float baseMoveSpeed;

    void Start()
    {
        animator = GetComponent<Animator>(); 
        baseMoveSpeed = moveSpeed;
    }

    void Update()
    {
        Vector3 moveDirection = Vector3.zero; 
        isRunning = false;

        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveDirection.y = 1;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveDirection.y = -1;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection.x = -1;
            isRunning = true; 
            transform.localScale = new Vector3(-1, transform.localScale.y); 
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveDirection.x = 1;
            isRunning = true;
            transform.localScale = new Vector3(1, transform.localScale.y);
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;

            animator.SetBool("Run", isRunning);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // Toggle running state or modify speed when Shift is pressed.
            isRunning = !isRunning;
            moveSpeed = isRunning ? baseMoveSpeed * 1.5f : baseMoveSpeed;
            animator.SetBool("Run", isRunning);
        }

        // Ensure movement is applied for other directions as well (D handled above already).
        // Only add movement here if there's any non-zero direction and it wasn't already applied.
        if (moveDirection != Vector3.zero && !(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)))
        {
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
        }

        // Ensure animator always reflects running state
        animator.SetBool("Run", isRunning);
    }
}