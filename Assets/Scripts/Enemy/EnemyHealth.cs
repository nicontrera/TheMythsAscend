using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Estadísticas")]
    [SerializeField] private int maxHealth = 30;
    private int currentHealth;

    [Header("Feedback Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    private Color originalColor;

    [Header("Knockback (Empuje)")]
    [Tooltip("0 = Recibe todo el empuje (Slime). 1 = Inmune al empuje (Jefe).")]
    [Range(0f, 1f)] 
    [SerializeField] private float knockbackResistance = 0f;
    
    private Rigidbody2D rb;

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer == null) 
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        originalColor = spriteRenderer.color;
    }

    // ACTUALIZADO: Ahora recibe la dirección del golpe y la fuerza del arma
    public void TakeDamage(int damage, Vector2 knockbackDirection, float weaponKnockbackForce)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} recibió {damage} de daño.");

        // 1. Activamos el parpadeo de color
        StartCoroutine(FlashDamage());

        // 2. Aplicamos el Knockback si el enemigo no es 100% inmune
        if (knockbackResistance < 1f && rb != null)
        {
            ApplyKnockback(knockbackDirection, weaponKnockbackForce);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplyKnockback(Vector2 direction, float force)
    {
        // Calculamos la fuerza final según la resistencia del enemigo
        float finalForce = force * (1f - knockbackResistance);

        // 1. NUEVO: Buscamos el Pathfinding y lo aturdimos durante un cuarto de segundo (0.25s)
        EnemyPathfinding pathfinding = GetComponent<EnemyPathfinding>();
        if (pathfinding != null)
        {
            pathfinding.Stagger(0.25f); 
        }

        // 2. Aplicamos la velocidad física del golpe
        rb.linearVelocity = direction * finalForce;

        // Reseteamos la velocidad actual para que el golpe se sienta en seco y contundente
        rb.linearVelocity = Vector2.zero;

        // Aplicamos el empuje de golpe (Impulse) en la dirección recibida
        rb.AddForce(direction * finalForce, ForceMode2D.Impulse);

        // ¡AGREGA ESTA LÍNEA PARA HACER DE DETECTIVE!
        Debug.Log($"Empujando a {gameObject.name} con fuerza de: {finalForce} en dirección: {direction}");
    }

    private IEnumerator FlashDamage()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto.");
        Destroy(gameObject);
    }
}