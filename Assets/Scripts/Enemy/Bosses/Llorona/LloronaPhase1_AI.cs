using System.Collections;
using UnityEngine;

public class LloronaPhase1_AI : MonoBehaviour
{
    private enum BossState
    {
        Chasing,
        Attacking,
        Teleporting
    }

    [Header("Referencias")]
    [SerializeField] private GameObject puddlePrefab;
    private Transform playerTransform;
    private EnemyPathfinding pathfinding;
    private LloronaCombat combat;
    private SpriteRenderer spriteRenderer;
    private Collider2D bossCollider;

    [Header("Configuración de Combate")]
    public float meleeAttackRange = 2f;
    [Tooltip("Distancia máxima a la que intentará disparar proyectiles.")]
    public float rangedAttackRange = 8f;
    
    [Header("Cooldowns de Habilidades")]
    public float teleportCooldown = 9f; // Teletransporte en charco
    public float draggerCooldown = 6f;  // Combo Arrastrador Melee
    public float rangedCooldown = 4f;   // <-- NUEVO: Abanico de Proyectiles
    public float timeHidden = 1.2f;

    private float teleportTimer;
    private float draggerTimer;
    private float rangedTimer; // <-- NUEVO TIMING
    private BossState state;

    void Awake()
    {
        pathfinding = GetComponent<EnemyPathfinding>();
        combat = GetComponent<LloronaCombat>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
        state = BossState.Chasing;
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        teleportTimer = teleportCooldown;
        draggerTimer = 0f;
        rangedTimer = 2f; // Le damos 2 segunditos al empezar la pelea antes de que dispare la primera vez
    }

    void Update()
    {
        if (playerTransform == null || state == BossState.Teleporting || state == BossState.Attacking) return;

        // 1. Restamos tiempo a los 3 cooldowns
        teleportTimer -= Time.deltaTime;
        if (draggerTimer > 0) draggerTimer -= Time.deltaTime;
        if (rangedTimer > 0) rangedTimer -= Time.deltaTime;

        // 2. PRIORIDAD 1: ¿Está listo el teletransporte en charco?
        if (teleportTimer <= 0)
        {
            StartCoroutine(TeleportAmbushRoutine());
            return;
        }

        // 3. Evaluamos distancias para decidir si atacar Melee, Rango o Perseguir
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // --- ZONA MELEE (Cerca) ---
        if (distanceToPlayer <= meleeAttackRange)
        {
            state = BossState.Attacking;
            pathfinding.MoveTo(Vector2.zero);

            if (draggerTimer <= 0)
            {
                draggerTimer = draggerCooldown;
                combat.StartDraggerCombo(playerTransform.position);
            }
            else
            {
                combat.StartAttack(playerTransform.position); // Zarpazo normal
            }
        }
        // --- ZONA DE RANGO (Lejos, pero dentro de visión de disparo) ---
        else if (distanceToPlayer <= rangedAttackRange && rangedTimer <= 0)
        {
            Debug.Log("¡La Llorona dispara su Abanico de Lamento a distancia!");
            state = BossState.Attacking;
            pathfinding.MoveTo(Vector2.zero); // Se frena para disparar
            
            rangedTimer = rangedCooldown; // Reiniciamos el cooldown de disparo
            combat.StartRangedAttack(playerTransform.position);
        }
        // --- ZONA DE PERSECUCIÓN (Caminando hacia Amira) ---
        else
        {
            Vector2 chaseDir = (playerTransform.position - transform.position).normalized;
            pathfinding.MoveTo(chaseDir);
        }
    }

    private IEnumerator TeleportAmbushRoutine()
    {
        state = BossState.Teleporting;
        pathfinding.MoveTo(Vector2.zero);
        teleportTimer = teleportCooldown;

        spriteRenderer.enabled = false;
        bossCollider.enabled = false;

        Vector3 targetAmbushPos = playerTransform.position; 
        if (puddlePrefab != null) Instantiate(puddlePrefab, targetAmbushPos, Quaternion.identity);

        yield return new WaitForSeconds(timeHidden);

        transform.position = targetAmbushPos;
        spriteRenderer.enabled = true;
        bossCollider.enabled = true;

        state = BossState.Attacking;
        combat.StartAttack(playerTransform.position); 
    }

    public void OnAttackFinished()
    {
        state = BossState.Chasing;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);

        // Nuevo círculo celeste para ver en el editor hasta dónde alcanza a disparar
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangedAttackRange);
    }
}