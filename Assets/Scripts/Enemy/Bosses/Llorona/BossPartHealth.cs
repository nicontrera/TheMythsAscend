using UnityEngine;

public class BossPartHealth : EnemyHealth
{
    [Header("Conexión con el Jefe Principal")]
    public EnemyHealth mainBossHealth;

    [Header("Estado de Armadura / Vulnerabilidad")]
    [Tooltip("Si está en false, la mano es inmune y bloquea los ataques de Amira.")]
    public bool isVulnerable = false; // <-- Empieza en FALSE (inmune en la orilla o en el aire)

    public override void TakeDamage(int damage, Vector2 knockbackDirection, float weaponKnockbackForce)
    {
        // 1. SI LA MANO TIENE LA ARMADURA ACTIVA (NO ES VULNERABLE), IGNORAMOS EL GOLPE
        if (!isVulnerable)
        {
            Debug.Log($"¡El ataque golpeó a {gameObject.name}, pero ahora mismo es INMUNE/INTOCABLE!");
            // TIP DE DISEÑO: Aquí en el futuro puedes reproducir un sonido de metálico "Clink!" 
            // o instanciar una chispa gris para indicarle al jugador que su ataque rebotó en una armadura.
            return; 
        }

        // 2. SI ESTÁ VULNERABLE (CLAVADA EN EL SUELO):
        if (mainBossHealth != null)
        {
            // Opcional: Llamamos a base.TakeDamage pero con daño 0 para que LA MANO haga el flash rojo visual
            base.TakeDamage(0, Vector2.zero, 0f);

            // Le enviamos el daño real al cuerpo principal (Coloso), sin fuerza de empuje
            mainBossHealth.TakeDamage(damage, Vector2.zero, 0f);
            Debug.Log($"¡Golpe crítico a la mano vulnerable! Daño ({damage}) transferido al Coloso.");
        }
        else
        {
            Debug.LogWarning("¡Falta conectar el Main Boss Health en la mano!");
        }
    }

    // Función auxiliar para que la IA encienda y apague la armadura fácilmente
    public void SetVulnerable(bool state)
    {
        isVulnerable = state;
    }
}