using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private InputSystem_Actions playerControls;
    private Animator playerAnimator;
    
    // Referencia a nuestro script de movimiento
    private PlayerController playerController; 

    private readonly int attackTrigger = Animator.StringToHash("Attack");

    void Awake()
    {
        playerControls = new InputSystem_Actions();
        playerAnimator = GetComponent<Animator>();
        // Buscamos el componente en el mismo GameObject
        playerController = GetComponent<PlayerController>(); 
    }

    void OnEnable() { playerControls.Enable(); }
    void OnDisable() { playerControls.Disable(); } // Agregado por seguridad

    void Start()
    {
        playerControls.Player.Attack.started += _ => Attack();
    }

    void Attack()
    {
        // Si ya está atacando, ignoramos el click para no reiniciar la animación a la mitad
        if (playerController.isAttacking) return;

        // 1. Congelamos al jugador modificando el bool del otro script
        playerController.isAttacking = true;

        // 2. Disparamos la animación
        playerAnimator.SetTrigger(attackTrigger);
    }

    // 3. Esta función va a ser llamada por la animación cuando el espadazo termine
    public void FinishAttack()
    {
        playerController.isAttacking = false;
    }
}