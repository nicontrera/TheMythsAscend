using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Estadísticas")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Feedback Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;
    private Color originalColor;

    private Rigidbody2D rb;
    private PlayerController playerController;
    private bool isInvulnerable = false;

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        
        if (spriteRenderer == null) 
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {
        // Si estamos invulnerables (recibiendo otro golpe o en un esquive), ignoramos el daño
        if (isInvulnerable) return;

        currentHealth -= damage;
        Debug.Log($"¡Amira recibió {damage} de daño! Salud restante: {currentHealth}");

        // 1. Activamos feedback de daño e invulnerabilidad temporal
        StartCoroutine(DamageRoutine(knockbackDir, knockbackForce));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageRoutine(Vector2 knockbackDir, float knockbackForce)
    {
        isInvulnerable = true;
        
        // Congelamos los controles de Amira temporalmente
        if (playerController != null) playerController.enabled = false;

        // Aplicamos el empuje físico
        rb.linearVelocity = knockbackDir * knockbackForce;
        spriteRenderer.color = damageColor;

        yield return new WaitForSeconds(flashDuration);

        // Devolvemos el color y el control al jugador
        spriteRenderer.color = originalColor;
        if (playerController != null) playerController.enabled = true;
        
        // Le damos un pequeño tiempo extra de invulnerabilidad para que no la abofeteen 2 veces seguidas
        yield return new WaitForSeconds(0.3f);
        isInvulnerable = false;
    }

    private void Die()
    {
        Debug.Log("¡Amira ha caído en batalla!");
        // Aquí luego puedes recargar la escena o mostrar el menú de Game Over
        gameObject.SetActive(false); 
    }
}