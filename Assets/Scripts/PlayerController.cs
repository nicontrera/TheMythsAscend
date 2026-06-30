using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    private InputSystem_Actions playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator playerAnimator;

    private SpriteRenderer playerSpriteRenderer;
    private Vector2 moveDirection;
    private readonly int moveX = Animator.StringToHash("moveX");
    private readonly int moveY = Animator.StringToHash("moveY");
    private readonly int isMoving = Animator.StringToHash("isMoving");

    void Awake()
    {
        playerControls = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        playerControls.Enable();
    }

    void Update()
    {
        PlayerInput();
    }

    void FixedUpdate()
    {
        Move();
    }

    void PlayerInput()
    {
        movement = playerControls.Player.Move.ReadValue<Vector2>().normalized;
        if (movement == Vector2.zero)
        {
            playerAnimator.SetBool(isMoving, false);
            return;
        }

        playerAnimator.SetBool(isMoving, true);
        playerAnimator.SetFloat(moveX, movement.x);
        playerAnimator.SetFloat(moveY, movement.y);

    }

    void Move()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    void ReadMovement()
    {
        moveDirection = playerControls.Player.Move.ReadValue<Vector2>().normalized;
    }

    void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
            Debug.Log("asd");


        if (mousePos.x < playerScreenPoint.x)
        {
            Debug.Log("hola");
            // playerSpriteRenderer.flipX = true;
        playerAnimator.SetFloat("moveY", 0.5f);

        }
        else
        {
            // playerSpriteRenderer.flipX = false;
        playerAnimator.SetFloat("moveY", -.9f);

        }
    }
}
