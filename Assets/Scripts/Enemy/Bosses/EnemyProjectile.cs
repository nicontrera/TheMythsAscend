using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Configuración del Proyectil")]
    public float speed = 10f;
    public int damage = 10;
    public float knockbackForce = 5f;
    public float lifetime = 4f; // Tiempo antes de autodestruirse si no toca nada
    public LayerMask playerLayer;

    private Vector2 moveDirection;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Esta función la llamará La Llorona al disparar para darle dirección
    public void Launch(Vector2 direction)
    {
        moveDirection = direction.normalized;
        
        // Rotamos visualmente el sprite del proyectil para que apunte hacia donde vuela
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Destruimos el proyectil después de unos segundos para limpiar la memoria
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        // Movemos el proyectil en línea recta constantemente
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }

    // Detectamos si golpeamos a Amira usando un Trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        // Revisamos si el objeto que tocamos está en la capa del Jugador
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // El empuje va en la misma dirección en la que vuela el proyectil
                playerHealth.TakeDamage(damage, moveDirection, knockbackForce);
            }

            // ¡El proyectil se destruye al impactar con Amira!
            Destroy(gameObject);
        }
        // Opcional: Si quieres que los proyectiles se destruyan al chocar con las paredes del mapa:
        // else if (other.CompareTag("Wall") || other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        // { Destroy(gameObject); }
    }
}