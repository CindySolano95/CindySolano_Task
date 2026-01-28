using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    public float runSpeed = 10f; // Velocidad de movimiento al correr
    public KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] private Vector2 direction;
    private Rigidbody2D rb;
    private Vector2 input;
    public float jumpForce = 10f;

    [Header("Animations")]
    [SerializeField] Animator animator;
    private bool isAttacking = false;
    private bool isJumping = false;
    private bool isRunning = false;
    private bool isDefending = false;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        direction = new Vector2(input.x, input.y).normalized;

        //Check if any of the axes has a value other than zero
        bool isWalking = (input.x != 0f || input.y != 0f);

        //Set the boolean isWalking in the Animator
        animator.SetBool("isWalking", isWalking);

        if (Input.GetMouseButtonDown(1))
        {
            //Check if the click is not on a user interface element
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                isAttacking = true;
                animator.SetInteger("Hit", 1);
            }

        }
        else
        {
            isAttacking = false;
            animator.SetInteger("Hit", 0);
        }

        //Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isJumping)
            {
                isJumping = true;
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                Debug.Log("isJumping");
            }
        }
        else
        {
            isJumping = false;
        }

        //Run
        if (Input.GetKey(runKey))
        {
            isRunning = true;
            //Increase movement speed when running
            rb.linearVelocity = new Vector2(input.x * runSpeed, rb.linearVelocity.y);

            Debug.Log("isrunning");
        }
        else
        {
            isRunning = false;
            //Restore normal movement speed
            rb.linearVelocity = new Vector2(input.x * moveSpeed, rb.linearVelocity.y);
        }

        //Defense
        if (Input.GetKey(KeyCode.X))
        {
            isDefending = true;
            Debug.Log("isDefending");
        }
        else
        {
            isDefending = false;
        }

        //Update the Animator parameters
        animator.SetBool("isJump", isJumping);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isDefending", isDefending);
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);


    }
}
