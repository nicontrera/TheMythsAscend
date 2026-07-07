using System.Collections;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    private EnemyAI enemyAI;
    [SerializeField] private float moveSpeed = 1f;
    private Vector2 moveDir;
    private Rigidbody2D rb;
    private Animator slimeAnimator;

    // NUEVO: Booleano para saber si el enemigo está siendo empujado
    public bool isStaggered = false; 

    void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        rb = GetComponent<Rigidbody2D>();
        slimeAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // Si está aturdido recibiendo un golpe, no calculamos nueva dirección
        if (isStaggered) return; 
        
        EnemyInput();
    }

    void FixedUpdate()
    {
        // Si está aturdido, NO sobrescribimos la velocidad del Rigidbody.
        // Dejamos que el empuje de la espada tome el control físico al 100%.
        if (isStaggered) return; 

        // SOLUCIÓN AL CONFLICTO: Usamos linearVelocity en lugar de MovePosition
        rb.linearVelocity = moveDir * moveSpeed;
    }

    void EnemyInput()
    {
        moveDir = enemyAI.roamingPosition.normalized;
        if (moveDir == Vector2.zero)
        {
            return;
        }
    }

    public void MoveTo(Vector2 targetPosition)
    {
        moveDir = targetPosition;
    }

    // NUEVA FUNCIÓN: Pausa el movimiento por una fracción de segundo al recibir daño
    public void Stagger(float duration = 0.25f)
    {
        StartCoroutine(StaggerRoutine(duration));
    }

    private IEnumerator StaggerRoutine(float duration)
    {
        isStaggered = true;
        yield return new WaitForSeconds(duration);
        isStaggered = false;
    }
}