using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; 
    
    private InputSystem_Actions playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator playerAnimator;

    private readonly int moveX = Animator.StringToHash("moveX");
    private readonly int moveY = Animator.StringToHash("moveY");
    private readonly int isMoving = Animator.StringToHash("isMoving");

    public Transform Aim;
    public bool isWalking = false;
    public bool isAttacking = false; // <-- EL NUEVO ESTADO DE ATAQUE

    Vector2 lastMoveDirection;

    void Awake()
    {
        playerControls = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
    }

    void OnEnable() { playerControls.Enable(); }
    void OnDisable() { playerControls.Disable(); }

    void Update()
    {
        // Si está atacando, no procesamos nuevos inputs
        if (isAttacking) return;

        PlayerInput();
    }

    void FixedUpdate()
    {
        // Si está atacando, frenamos su velocidad física en seco
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Move();

        if (isWalking)
        {
            Vector3 vector3 = Vector3.left * movement.x + Vector3.down * movement.y;
            Aim.rotation = Quaternion.LookRotation(Vector3.forward, vector3);
        }
    }

    void PlayerInput()
    {
        movement = playerControls.Player.Move.ReadValue<Vector2>().normalized;
        
        if (movement == Vector2.zero)
        {
            playerAnimator.SetBool(isMoving, false);
            isWalking = false;

            // CORRECCIÓN: No igualar lastMoveDirection a movement aquí, porque movement es (0,0).
            // Usamos el lastMoveDirection que ya traíamos guardado.
            Vector3 vector3 = Vector3.left * lastMoveDirection.x + Vector3.down * lastMoveDirection.y;
            
            // Chequeo rápido para que no tire error en el frame 1 del juego
            if (vector3 != Vector3.zero) 
            {
                Aim.rotation = Quaternion.LookRotation(Vector3.forward, vector3);
            }
            return;
        }
        else
        {
            isWalking = true;
            lastMoveDirection = movement; // Aquí sí guardamos la dirección válida
        }

        playerAnimator.SetBool(isMoving, true);
        playerAnimator.SetFloat(moveX, movement.x);
        playerAnimator.SetFloat(moveY, movement.y);
    }

    void Move()
    {
        rb.linearVelocity = movement * moveSpeed;
    }
}