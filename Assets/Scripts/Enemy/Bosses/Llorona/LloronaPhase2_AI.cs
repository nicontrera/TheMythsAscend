using System.Collections;
using UnityEngine;

public class LloronaPhase2_AI : MonoBehaviour
{
    private enum Phase2State
    {
        Idle,
        SlammingHand,
        ShootingTears
    }

    [Header("Referencias de las Manos")]
    public Transform leftHand;  // Objeto de la mano izquierda
    public Transform rightHand; // Objeto de la mano derecha
    public Vector2 leftHandRestPos;  // Posición de reposo en la orilla
    public Vector2 rightHandRestPos; // Posición de reposo en la orilla

    [Header("Configuración del Aplastamiento (Hand Slam)")]
    public float slamCooldown = 4f;    // Cada cuánto aplasta el piso
    public float hoverDuration = 0.8f; // Cuánto tiempo avisa en el aire (sombra)
    public float vulnerableTime = 3f;  // ¡CUÁNTO TIEMPO QUEDA EN EL PISO PARA QUE AMIRA LE PEGUE!
    public float slamRadius = 1.8f;    // Radio del daño al aplastar
    public int slamDamage = 20;
    public float slamKnockback = 25f;
    public LayerMask playerLayer;

    private Transform playerTransform;
    private float timer;
    private Phase2State state;
    private bool useLeftHandNext = true; // Alternamos entre mano izquierda y derecha

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // Guardamos las posiciones iniciales donde descansan las manos al empezar
        if (leftHand != null) leftHandRestPos = leftHand.position;
        if (rightHand != null) rightHandRestPos = rightHand.position;

        timer = 2f; // Pequeña pausa al iniciar la Fase 2
        state = Phase2State.Idle;
    }

    void Update()
    {
        if (playerTransform == null || state != Phase2State.Idle) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            // Elegimos qué mano atacará este turno
            Transform activeHand = useLeftHandNext ? leftHand : rightHand;
            Vector2 restPos = useLeftHandNext ? leftHandRestPos : rightHandRestPos;
            
            useLeftHandNext = !useLeftHandNext; // Alternamos para el siguiente turno
            timer = slamCooldown;

            StartCoroutine(HandSlamRoutine(activeHand, restPos));
        }
    }

    private IEnumerator HandSlamRoutine(Transform hand, Vector2 restPos)
    {
        state = Phase2State.SlammingHand;
        SpriteRenderer handSprite = hand.GetComponentInChildren<SpriteRenderer>();

        // Obtenemos el script de salud de ESTA mano en específico
        BossPartHealth partHealth = hand.GetComponent<BossPartHealth>();
        
        // Aseguramos que empiece el ataque siendo invulnerable
        if (partHealth != null) partHealth.SetVulnerable(false);

        // 1. ELEVAR LA MANO Y BUSCAR A AMIRA (Wind-up)
        Debug.Log($"¡La Llorona levanta su mano para aplastar!");
        if (handSprite != null) handSprite.color = Color.yellow; // Aviso visual

        Vector2 startPos = hand.position;
        // La mano flota justo por encima de donde está Amira en ese instante
        Vector2 targetHoverPos = (Vector2)playerTransform.position + new Vector2(0f, 2f);

        // Mover hacia arriba del jugador suavemente
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            hand.position = Vector2.Lerp(startPos, targetHoverPos, elapsed / 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. PAUSA TERRORÍFICA EN EL AIRE (0.8s) - ¡El jugador debe correr de la sombra!
        yield return new WaitForSeconds(hoverDuration);

        // 3. ¡EL APLASTAMIENTO! (Slam Down a máxima velocidad)
        if (handSprite != null) handSprite.color = Color.red;
        Vector2 slamPos = hand.position - new Vector3(0f, 2f, 0f); // Cae 2 unidades directo al piso

        elapsed = 0f;
        while (elapsed < 0.12f) // Caída ultra rápida de 0.12 segundos
        {
            hand.position = Vector2.Lerp(targetHoverPos, slamPos, elapsed / 0.12f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        hand.position = slamPos; // Clavar en el piso exacto

        // 4. DAÑO EN EL ÁREA DE IMPACTO (Onda de choque)
        CheckSlamDamage(slamPos);

        // 5. ¡VENTANA DE VULNERABILIDAD! (3 Segundos)
        // La mano se queda quieta en el piso. ¡Amira debe aprovechar para destrozarla a espadazos!
        Debug.Log("¡La mano está clavada en el piso! ¡ARMADURA DESACTIVADA, ATACA AHORA!");
        if (handSprite != null) handSprite.color = new Color(0.5f, 0.8f, 1f); // Color de fatiga

        // ¡LE QUITAMOS LA ARMADURA! Ahora la espada de Amira SÍ le bajará vida al jefe
        if (partHealth != null) partHealth.SetVulnerable(true);

        yield return new WaitForSeconds(vulnerableTime);

        // =========================================================
        // 5.5. FIN DE LA VENTANA -> VOLVER A HABILITAR LA ARMADURA
        // =========================================================
        Debug.Log("¡La mano recupera su armadura y se retira!");
        if (partHealth != null) partHealth.SetVulnerable(false); // ¡Vuelve a ser inmune!

        // 6. RETIRADA (La mano vuelve suavemente a la orilla del lago)
        if (handSprite != null) handSprite.color = Color.white;
        startPos = hand.position;
        elapsed = 0f;
        while (elapsed < 0.8f)
        {
            hand.position = Vector2.Lerp(startPos, restPos, elapsed / 0.8f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        hand.position = restPos;

        state = Phase2State.Idle;
    }

    private void CheckSlamDamage(Vector2 impactCenter)
    {
        Collider2D playerHit = Physics2D.OverlapCircle(impactCenter, slamRadius, playerLayer);
        if (playerHit != null)
        {
            PlayerHealth playerHealth = playerHit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (playerHit.transform.position - (Vector3)impactCenter).normalized;
                playerHealth.TakeDamage(slamDamage, knockbackDir, slamKnockback);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (leftHand != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(leftHand.position, slamRadius);
        }
        if (rightHand != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(rightHand.position, slamRadius);
        }
    }
}