using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    private EnemyAI enemyAI;
    [SerializeField] private float moveSpeed = 1f;
    private Vector2 moveDir;
    private Rigidbody2D rb;
    private Animator slimeAnimator;

    void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        rb = GetComponent<Rigidbody2D>();
        slimeAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        EnemyInput();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));
    }

    void EnemyInput()
    {
        moveDir = enemyAI.roamingPosition.normalized;
        if (moveDir == Vector2.zero)
        {
            // playerAnimator.SetBool(isMoving, false);
            return;
        }

        // playerAnimator.SetBool(isMoving, true);
        // playerAnimator.SetFloat(moveX, moveDir.x);
        // playerAnimator.SetFloat(moveY, moveDir.y);

    }

    public void MoveTo(Vector2 targetPosition)
    {
        moveDir = targetPosition;
    }
}
