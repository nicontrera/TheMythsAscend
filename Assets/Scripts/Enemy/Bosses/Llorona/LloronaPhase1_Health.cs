using System.Collections;
using UnityEngine;

// Heredamos de EnemyHealth para mantener el flash rojo y la compatibilidad con la espada
public class LloronaPhase1_Health : EnemyHealth
{
    [Header("Transición a Fase 2")]
    [Tooltip("Arrastra aquí el GameObject de Llorona_Fase2 (el coloso del lago) que está en tu escena.")]
    public GameObject phase2ColossusObject;
    
    [Tooltip("Tiempo en segundos que tarda en disolverse o gritar antes de que aparezca el gigante.")]
    public float transitionDelay = 1.5f;

    private bool isTransitioning = false;

    public override void TakeDamage(int damage, Vector2 knockbackDirection, float weaponKnockbackForce)
    {
        // Si ya está transformándose o está muerta, ignoramos más golpes
        if (isTransitioning || currentHealth <= 0) return;

        // 1. Aplicamos el daño normal (esto llama a base.TakeDamage y hace el flash rojo)
        base.TakeDamage(damage, knockbackDirection, weaponKnockbackForce);

        // 2. REVISIÓN DE UMBRAL: ¿Llegó al 50% o menos de su vida máxima?
        if (currentHealth <= (maxHealth / 2) && !isTransitioning)
        {
            StartCoroutine(PhaseTransitionRoutine());
        }
    }

    private IEnumerator PhaseTransitionRoutine()
    {
        isTransitioning = true;
        Debug.Log("¡La Llorona ha llegado al 50% de vida! ¡INICIANDO TRANSICIÓN A FASE 2!");

        // 1. APAGAR EL CEREBRO Y COMBATE DE LA FASE 1
        // Detenemos sus scripts para que deje de perseguir o atacar a Amira de inmediato
        LloronaPhase1_AI ai = GetComponent<LloronaPhase1_AI>();
        if (ai != null) ai.enabled = false;

        LloronaCombat combat = GetComponent<LloronaCombat>();
        if (combat != null) combat.enabled = false;

        // 2. FRENAR EN SECO Y HACERLA INVULNERABLE
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 3. EFECTO DRAMÁTICO (El aviso de la transformación)
        // La hacemos parpadear en azul/oscuro o temblar como si se deshiciera en agua
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0.1f, 0.3f, 0.8f);

        // Aquí en el futuro puedes agregar un sonido de grito terrorífico o un temblor de cámara
        
        yield return new WaitForSeconds(transitionDelay);

        // 4. ¡DESPERTAR AL COLOSO DEL LAGO!
        if (phase2ColossusObject != null)
        {
            // Encendemos el GameObject de la Fase 2 que estaba invisible en el norte
            phase2ColossusObject.SetActive(true);
            Debug.Log("¡El Coloso del Lago ha emergido!");
        }
        else
        {
            Debug.LogError("¡Ojo! Te olvidaste de arrastrar el objeto Phase 2 Colossus en el Inspector.");
        }

        // 5. DESTRUIR LA FASE 1
        // Desaparecemos esta versión pequeña del mapa
        Destroy(gameObject);
    }
}