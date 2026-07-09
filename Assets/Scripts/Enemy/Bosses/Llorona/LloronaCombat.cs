using System.Collections;
using UnityEngine;

public class LloronaCombat : EnemyCombat
{
    [Header("Configuración del Agarre (True Grab)")]
    [Tooltip("Qué tan ancho es el primer golpe para capturar al jugador.")]
    public float grabRangeMultiplier = 1.8f; 
    [Tooltip("Tiempo en segundos que tarda en arrastrar a Amira hasta sus pies.")]
    public float pullDuration = 0.2f;


    [Header("Configuración Ataque a Distancia (Proyectiles)")]
    public GameObject projectilePrefab; // Arrastra aquí el Prefab del proyectil
    public float projectileSpeed = 9f;
    [Tooltip("Separación en grados entre cada proyectil del abanico.")]
    public float spreadAngle = 18f;

    public void StartDraggerCombo(Vector2 targetPosition)
    {
        StartCoroutine(DraggerComboRoutine(targetPosition));
    }

    private IEnumerator DraggerComboRoutine(Vector2 targetPosition)
    {
        // =========================================================
        // FASE 1: EL AVISO / TELEGRAPH (0.65 segundos)
        // La Llorona levanta el brazo, vibra y cambia a morado.
        // =========================================================
        pathfinding.Stagger(0.65f + pullDuration); 
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = new Color(0.8f, 0.1f, 0.8f); // Morado brillante de peligro
        
        Debug.Log("¡La Llorona prepara su agarre de amplio alcance!");
        yield return new WaitForSeconds(0.65f);

        // =========================================================
        // FASE 2: GOLPE 1 - LA CAPTURA (Rango Amplio, Difícil de esquivar)
        // =========================================================
        spriteRenderer.color = originalColor;
        hasDealtDamage = false;

        // Pequeño salto hacia adelante
        Vector2 dashDir = (targetPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = dashDir * (dashSpeed * 0.5f);

        // CHEQUEO DE CAPTURA CON RANGO AMPLIO (Grab)
        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, attackRadius * grabRangeMultiplier, playerLayer);
        if (playerHit != null)
        {
            PlayerHealth playerHealth = playerHit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // 1. Le hacemos poco daño inicial (es solo la captura) y 0 FUERZA FÍSICA
                playerHealth.TakeDamage(damage / 2, Vector2.zero, 0f);
                
                // 2. ¡INICIAMOS EL ARRASTRE REAL! Traemos a Amira magnéticamente hacia La Llorona
                StartCoroutine(PullPlayerToMe(playerHit.transform, pullDuration));
                hasDealtDamage = true;
            }
        }

        rb.linearVelocity = Vector2.zero;
        // Esperamos a que termine de ser arrastrada
        yield return new WaitForSeconds(pullDuration);

        // =========================================================
        // FASE 3: LA VENTANA DE ESCAPE (0.35 segundos)
        // Amira ya está en los pies de La Llorona, pero ya puede moverse.
        // La Llorona levanta el otro brazo para rematar. ¡Es momento de hacer DASH!
        // =========================================================
        spriteRenderer.color = Color.yellow; // Aviso de que viene el golpe mortal
        Debug.Log("¡Amira fue atrapada! ¡ESQUIVA AHORA!");
        yield return new WaitForSeconds(0.35f);

        // =========================================================
        // FASE 4: GOLPE 2 - LA EJECUCIÓN (Fácil de esquivar si te moviste)
        // =========================================================
        spriteRenderer.color = originalColor;
        hasDealtDamage = false;

        // Ataca exactamente hacia el frente, donde arrastró al jugador antes
        rb.linearVelocity = dashDir * (dashSpeed * 1.3f);
        
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            // Este golpe duele el doble y ahora sí empuja hacia afuera de forma normal
            CheckHitPlayerCustom(damage * 2, 18f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // =========================================================
        // FASE 5: RECOVERY / VULNERABILIDAD (0.9 segundos)
        // La Llorona queda agotada. ¡Ventana de castigo para el jugador!
        // =========================================================
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.9f);

        NotifyLloronaAIFinished();
    }

    // --- LA MAGIA DEL ARRASTRE REAL ---
    private IEnumerator PullPlayerToMe(Transform playerTrans, float duration)
    {
        PlayerController playerCtrl = playerTrans.GetComponent<PlayerController>();
        Rigidbody2D playerRb = playerTrans.GetComponent<Rigidbody2D>();

        // 1. Bloqueamos el control de Amira para que no pueda luchar contra la succión
        if (playerCtrl != null) playerCtrl.enabled = false;

        Vector2 startPos = playerTrans.position;
        // El punto de destino es justo enfrente de La Llorona (a 1 unidad de distancia)
        Vector2 dirToPlayer = (startPos - (Vector2)transform.position).normalized;
        Vector2 targetPos = (Vector2)transform.position + dirToPlayer * 1.1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (playerRb != null)
            {
                // Deslizamos a Amira suavemente de donde estaba hasta los pies del jefe
                Vector2 newPos = Vector2.Lerp(startPos, targetPos, elapsed / duration);
                playerRb.MovePosition(newPos);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. Lo dejamos en la posición final exacta
        if (playerRb != null) playerRb.MovePosition(targetPos);
        
        // 3. ¡MUY IMPORTANTE! Le devolvemos el control a Amira para que pueda esquivar el 2do golpe
        if (playerCtrl != null) playerCtrl.enabled = true;
    }

    private void CheckHitPlayerCustom(int customDamage, float customKnockbackForce)
    {
        if (hasDealtDamage) return;

        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, attackRadius, playerLayer);
        if (playerHit != null)
        {
            PlayerHealth playerHealth = playerHit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (playerHit.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(customDamage, knockbackDir, customKnockbackForce);
                hasDealtDamage = true;
            }
        }
    }

    private void NotifyLloronaAIFinished()
    {
        LloronaPhase1_AI bossAI = GetComponent<LloronaPhase1_AI>();
        if (bossAI != null) bossAI.OnAttackFinished();
    }

    // Dibujamos ambos rangos en el Editor para que veas la diferencia de tamaño
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius); // Rango del 2do golpe normal

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRadius * grabRangeMultiplier); // Rango gigante del Agarre
    }

    // =========================================================
    // NUEVO: ATAQUE A DISTANCIA - ABANICO DEL LAMENTO
    // =========================================================
    public void StartRangedAttack(Vector2 targetPosition)
    {
        StartCoroutine(RangedAttackRoutine(targetPosition));
    }

    private IEnumerator RangedAttackRoutine(Vector2 targetPosition)
    {
        // --- FASE 1: WIND-UP / AVISO (0.5 segundos) ---
        pathfinding.Stagger(0.5f + 0.4f);
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = new Color(0.2f, 0.8f, 1f); // Color Celeste/Agua brillante
        
        Debug.Log("¡La Llorona toma aire para lanzar su Lamento!");
        yield return new WaitForSeconds(0.5f);

        // --- FASE 2: DISPARO DEL ABANICO (3 Proyectiles) ---
        spriteRenderer.color = originalColor;

        if (projectilePrefab != null)
        {
            // Calculamos la dirección central hacia donde está Amira
            Vector2 centerDir = (targetPosition - (Vector2)transform.position).normalized;

            // Disparamos el proyectil 1 (Directo al centro)
            SpawnProjectile(centerDir);

            // Disparamos el proyectil 2 (Desviado a la izquierda usando trigonometría de Quaternion)
            Vector2 leftDir = Quaternion.Euler(0, 0, spreadAngle) * centerDir;
            SpawnProjectile(leftDir);

            // Disparamos el proyectil 3 (Desviado a la derecha)
            Vector2 rightDir = Quaternion.Euler(0, 0, -spreadAngle) * centerDir;
            SpawnProjectile(rightDir);
        }
        else
        {
            Debug.LogWarning("¡Falta asignar el Projectile Prefab en LloronaCombat!");
        }

        // --- FASE 3: RECOVERY / RECUPERACIÓN (0.4 segundos) ---
        yield return new WaitForSeconds(0.4f);

        NotifyLloronaAIFinished();
    }

    // Función auxiliar para instanciar y configurar cada gota/onda
    private void SpawnProjectile(Vector2 direction)
    {
        GameObject projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        EnemyProjectile projScript = projObj.GetComponent<EnemyProjectile>();
        
        if (projScript != null)
        {
            projScript.speed = projectileSpeed;
            projScript.damage = damage;
            projScript.playerLayer = playerLayer;
            projScript.Launch(direction);
        }
    }
}