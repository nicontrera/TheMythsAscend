using System.Collections;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    public int damage = 10;
    public float dashSpeed = 12f;
    public float knockbackForce = 15f;
    public float attackRadius = 1f;
    public LayerMask playerLayer; // Capa del Jugador

    [Header("Tiempos (Game Feel)")]
    public float windUpTime = 0.4f;   // Tiempo avisando antes de saltar
    public float dashDuration = 0.2f; // Cuánto dura la embestida
    public float recoveryTime = 0.6f; // Cuánto descansa después de atacar

    [Header("Feedback Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color warningColor = Color.yellow; // Color de aviso
    private Color originalColor;

    private Rigidbody2D rb;
    private EnemyPathfinding pathfinding;
    private EnemyAI enemyAI;
    private bool hasDealtDamage = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pathfinding = GetComponent<EnemyPathfinding>();
        enemyAI = GetComponent<EnemyAI>();
        
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    // Esta función será llamada por EnemyAI cuando decida atacar
    public void StartAttack(Vector2 targetPosition)
    {
        StartCoroutine(AttackRoutine(targetPosition));
    }

    private IEnumerator AttackRoutine(Vector2 targetPosition)
    {
        // --- FASE 1: WIND-UP (AVISO) ---
        // Frenamos al enemigo en seco y lo hacemos parpadear/cambiar de color
        pathfinding.Stagger(windUpTime + dashDuration); 
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = warningColor;
        
        yield return new WaitForSeconds(windUpTime);

        // --- FASE 2: DASH (EL GOLPE) ---
        spriteRenderer.color = originalColor;
        hasDealtDamage = false; // Reseteamos para poder hacer daño en este ataque

        // Calculamos la dirección hacia donde estaba el jugador
        Vector2 dashDir = (targetPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = dashDir * dashSpeed;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            // Mientras dura la embestida, revisamos en cada frame si tocamos a Amira
            CheckHitPlayer();
            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- FASE 3: RECOVERY (RECUPERACIÓN) ---
        // Frenamos la embestida y dejamos al enemigo vulnerable un momento
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(recoveryTime);

        // Le avisamos a la IA que terminamos el ataque y puede volver a perseguir
        enemyAI.OnAttackFinished();
    }

    private void CheckHitPlayer()
    {
        // Si ya le hicimos daño en esta embestida, no le pegamos 60 veces por segundo
        if (hasDealtDamage) return;

        // Dibujamos un círculo invisible para ver si chocamos con el jugador
        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, attackRadius, playerLayer);
        if (playerHit != null)
        {
            PlayerHealth playerHealth = playerHit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (playerHit.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(damage, knockbackDir, knockbackForce);
                hasDealtDamage = true; // ¡Golpe registrado!
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}