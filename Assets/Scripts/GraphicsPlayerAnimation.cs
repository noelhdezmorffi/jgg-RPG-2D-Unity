using UnityEngine;
using UnityEngine.InputSystem;

public class GraphicsPlayerAnimation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // [SerializeField] private float moveSpeed = 5f;
    // private Vector2 moveInput;
    private Animator animator;
    private Vector2 lastMoveDirection = Vector2.down;
    private PlayerController player;
    void Awake()
    {
        animator = GetComponent<Animator>();
        player = GetComponentInParent<PlayerController>();
    }
   
    // void OnMove(InputValue value)
    // {
    //     moveInput = value.Get<Vector2>();
    // }

    void Update()
    {
        Vector2 moveInput = player.MoveInput;
        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            animator.SetBool("IsMoving", true);
            
            //Idle
            lastMoveDirection = moveInput.normalized;
            animator.SetFloat("IdleX", lastMoveDirection.x);
            animator.SetFloat("IdleY", lastMoveDirection.y);
        }

        else
        {
            animator.SetBool("IsMoving", false);        
        }
    }
}
