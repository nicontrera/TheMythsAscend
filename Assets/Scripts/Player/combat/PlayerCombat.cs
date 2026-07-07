using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private InputSystem_Actions playerControls;
    private Animator playerAnimator;
    private PlayerController playerController;

    private readonly int attackTrigger = Animator.StringToHash("Attack");

    [Header("Configuración de Hitbox de Espada")]
    public Transform attackPoint; 
    // Ahora usamos un Vector2 para controlar Ancho y Largo del corte por separado
    public Vector2 hitboxSize = new Vector2(1.2f, 1f); 
    public LayerMask enemyLayers;
    public int attackDamage = 10;

    [Tooltip("Fuerza base del empuje del arma.")]
    public float knockbackForce = 50f;

    void Awake()
    {
        playerControls = new InputSystem_Actions();
        playerAnimator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    void OnEnable() { playerControls.Enable(); }
    void OnDisable() { playerControls.Disable(); }

    void Start()
    {
        playerControls.Player.Attack.started += _ => Attack();
    }

    void Attack()
    {
        if (playerController.isAttacking) return;

        playerController.isAttacking = true;
        playerAnimator.SetTrigger(attackTrigger);
    }

    // FUNCIÓN ACTUALIZADA: Ahora proyecta un rectángulo que rota con la espada
    // public void TriggerHitbox()
    // {
    //     // OverlapBoxAll toma: Posición, Tamaño (X, Y), Ángulo de rotación en Z, y la Capa
    //     Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
    //         attackPoint.position, 
    //         hitboxSize, 
    //         attackPoint.eulerAngles.z, 
    //         enemyLayers
    //     );

    //     foreach (Collider2D enemy in hitEnemies)
    //     {
    //         EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
    //         if (enemyHealth != null)
    //         {
    //             enemyHealth.TakeDamage(attackDamage);
    //         }
    //     }
    // }

    public void TriggerHitbox()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
            attackPoint.position, 
            hitboxSize, 
            attackPoint.eulerAngles.z, 
            enemyLayers
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // 1. Calculamos el vector de dirección: (Posición del Enemigo - Posición del Jugador)
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;

                // 2. Le pasamos el daño, la dirección y la fuerza al enemigo
                enemyHealth.TakeDamage(attackDamage, knockbackDirection, knockbackForce);
            }
        }
    }

    public void FinishAttack()
    {
        playerController.isAttacking = false;
    }

    // GIZMOS ACTUALIZADOS: Dibuja la caja roja rotada en el Editor para que la ajustes fácilmente
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        // Guardamos la matriz original para no arruinar otros dibujos
        Matrix4x4 oldMatrix = Gizmos.matrix;
        // Rotamos el dibujo del Gizmo para que coincida con la rotación del AttackPoint
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
        // Restauramos la matriz
        Gizmos.matrix = oldMatrix;
    }
}