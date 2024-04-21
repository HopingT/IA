using UnityEngine;
using UnityEngine.AI;

public class PlayerNavMesh : MonoBehaviour
{
    public float speed = 5f; // Velocidad de movimiento
    public float jumpHeight = 2f; // Altura del salto
    private bool isJumping = false; // Variable para controlar el estado de salto
    private NavMeshAgent navMeshAgent;
    public Animator ani;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Obtener la entrada del teclado
        float horizontalInput = 0f;
        float verticalInput = 0f;
        ani.SetBool("Walk", false);

        if (Input.GetKey(KeyCode.W))
        {
            verticalInput += 1f;
            ani.SetBool("Walk", true);
        }

        if (Input.GetKey(KeyCode.S))
        {
            verticalInput -= 1f;
            ani.SetBool("Walk", true);
        }

        if (Input.GetKey(KeyCode.D))
        {
            ani.SetBool("Walk", true);
            horizontalInput += 1f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            ani.SetBool("Walk", true);
            horizontalInput -= 1f;
        }

        // Calcular la dirección del movimiento
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput) * speed * Time.deltaTime;

        // Aplicar el movimiento al NavMeshAgent
        MovePlayer(movement);

        // Controlar el salto
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            Jump();
        }
    }

    void MovePlayer(Vector3 movement)
    {
        // Obtener la posición objetivo sumando el movimiento actual a la posición actual del jugador
        Vector3 targetPosition = transform.position + movement;

        // Mover al jugador hacia la posición objetivo usando NavMeshAgent
        navMeshAgent.SetDestination(targetPosition);
    }

    void Jump()
    {
        // Calcular la posición de salto
        Vector3 jumpPosition = transform.position + Vector3.up * jumpHeight;

        // Mover al jugador a la posición de salto
        navMeshAgent.Warp(jumpPosition);

        // Marcar al jugador como en estado de salto
        isJumping = true;

        // Activar la animación de salto
        ani.SetBool("Jump", true);
    }

    // Método que se llama cuando el jugador aterriza después de un salto
    void OnAnimatorMove()
    {
        // Verificar si el jugador está en estado de salto y está cerca del suelo
        if (isJumping && navMeshAgent.remainingDistance < 0.1f)
        {
            // Marcar al jugador como no en estado de salto
            isJumping = false;

            // Desactivar la animación de salto
            ani.SetBool("Jump", false);
        }
    }
}
