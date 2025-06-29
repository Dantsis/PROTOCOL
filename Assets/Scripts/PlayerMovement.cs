using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private string lastDirection = "Down";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement != Vector2.zero)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                if (movement.x > 0)
                {
                    animator.Play("Walk_Right");
                    lastDirection = "Right";
                }
                else
                {
                    animator.Play("Walk_Left");
                    lastDirection = "Left";
                }
            }
            else
            {
                if (movement.y > 0)
                {
                    animator.Play("Walk_Up");
                    lastDirection = "Up";
                }
                else
                {
                    animator.Play("Walk_Down");
                    lastDirection = "Down";
                }
            }
        }
        else
        {
            animator.Play("Idle_" + lastDirection);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}


