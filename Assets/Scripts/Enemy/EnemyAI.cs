using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        Roaming,
        Chasing,
        Attacking // <-- NUEVO ESTADO
    }

    private State state;
    private EnemyPathfinding enemyPathfinding;
    private EnemyCombat enemyCombat; // Referencia al nuevo script de combate
    private Transform playerTransform;

    [Header("Configuración de Visión y Ataque")]
    public float detectRange = 4f;
    public float stopChaseRange = 6f;
    [Tooltip("Distancia a la que se lanza a atacar al jugador.")]
    public float attackRange = 1.5f; // <-- NUEVO RANGO

    public Vector2 roamingPosition;

    void Awake()
    {
        state = State.Roaming;
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        enemyCombat = GetComponent<EnemyCombat>();
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        StartCoroutine(RoamingRoutine());
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        switch (state)
        {
            case State.Roaming:
                if (distanceToPlayer < detectRange)
                {
                    state = State.Chasing;
                }
                break;

            case State.Chasing:
                if (distanceToPlayer > stopChaseRange)
                {
                    state = State.Roaming;
                    StartCoroutine(RoamingRoutine());
                }
                // SI ESTÁ LO SUFICIENTEMENTE CERCA, ¡ATACA!
                else if (distanceToPlayer <= attackRange)
                {
                    state = State.Attacking;
                    enemyPathfinding.MoveTo(Vector2.zero); // Deja de caminar
                    enemyCombat.StartAttack(playerTransform.position); // Inicia la embestida
                }
                else
                {
                    Vector2 chaseDirection = (playerTransform.position - transform.position).normalized;
                    enemyPathfinding.MoveTo(chaseDirection);
                }
                break;

            case State.Attacking:
                // Mientras ataca, la IA no hace nada en Update. 
                // Esperamos a que EnemyCombat nos avise que terminó.
                break;
        }
    }

    private IEnumerator RoamingRoutine()
    {
        while (state == State.Roaming)
        {
            roamingPosition = GetRoamingPosition();
            enemyPathfinding.MoveTo(roamingPosition);
            yield return new WaitForSeconds(2f);
        }
    }

    private Vector2 GetRoamingPosition()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    // NUEVO: Esta función la llama EnemyCombat al terminar los 3 tiempos (Windup, Dash, Recovery)
    public void OnAttackFinished()
    {
        state = State.Chasing;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopChaseRange);

        // NUEVO: Círculo rojo en el editor para ver el rango donde decide atacar
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}