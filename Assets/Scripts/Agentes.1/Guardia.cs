
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GuardScript : MonoBehaviour
{
    // Variables públicas para configurar el comportamiento del guardia desde el editor de Unity
    public Transform player; // Referencia al jugador para poder detectarlo
    public Collider capsuleC; // Colisionador del guardia, no se utiliza en este script
    public GameObject player1; // Objeto del jugador, utilizado para destruir al jugador si es necesario
    public float visionAngle = 45f; // Ángulo de visión del guardia para detectar al jugador
    public float visionRadius = 5f; // Radio de visión del guardia para detectar al jugador
    public float rotationInterval = 5f; // Tiempo entre rotaciones del guardia cuando no está en alerta
    public float alertDuration = 3f; // Duración del estado de alerta, no se utiliza en este script
    public float attackDuration = 5f; // Duración del estado de ataque
    public float pursuitSpeed = 5f; // Velocidad a la que el guardia persigue al jugador
    public Transform initialPosition; // Posición inicial del guardia para volver a ella si es necesario
    public float attackRange = 0.1f; // Rango de ataque del guardia, no se utiliza en este script
    private float timeSinceLastRotation = 0f; // Temporizador para controlar la rotación del guardia
    private bool isAlerted = false; // Indica si el guardia está en estado de alerta
    private bool isAttacking = false;
    private bool isReturning = false;
    private Vector3 lastPlayerPosition; // Última posición conocida del jugador
    public Color normalColor = Color.yellow; // Color del guardia en estado normal
    public Color alertedColor = Color.red; // Color del guardia en estado de alerta
    public Color attackingColor = Color.magenta; // Color del guardia en estado de ataque
    

    // Método Start llamado al inicio para inicializar variables
    void Start()
    {
        lastPlayerPosition = player.position; // Inicializa la última posición conocida del jugador
    }

    // Método Update llamado en cada frame para actualizar el estado del guardia
    void Update()
    {
        // Si el guardia está regresando, se llama a ReturnToInitialPosition y no se permite ninguna otra acción
        if (isReturning)
        {
            ReturnToInitialPosition();
        }
        else if (!isAlerted && !isAttacking && !isReturning)
            {
            RotateGuard();
            CheckForPlayer();
        }
        
            // Si el guardia no está alertado ni atacando, busca al jugador
            if (!isAlerted && !isAttacking)
            {
                RotateGuard();
                CheckForPlayer();
            }
            else if (isAlerted)
            {
                AlertState();
            }
            else if (isAttacking)
            {
                AttackState();
            }
        
    }




    // Método para rotar al guardia en intervalos regulares
    void RotateGuard()
    {
        timeSinceLastRotation += Time.deltaTime; // Aumenta el temporizador basado en el tiempo real
        if (timeSinceLastRotation >= rotationInterval)
        {
            // Rotar el guardia en un ángulo aleatorio entre 45 y 90 grados
            transform.Rotate(Vector3.up, Random.Range(45f, 90f));
            timeSinceLastRotation = 0f; // Restablecer el temporizador
        }
    }

    // Método para detectar al jugador dentro del campo de visión
    void CheckForPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position; // Calcular dirección hacia el jugador
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer); // Calcular ángulo hacia el jugador
        if (angleToPlayer <= visionAngle / 2f) // Si el jugador está dentro del ángulo de visión
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, visionRadius)) // Lanzar un rayo hacia el jugador
            {
                if (hit.collider.CompareTag("Player")) // Si el rayo colisiona con el jugador
                {
                    isAlerted = true; // El guardia entra en estado de alerta
                    lastPlayerPosition = player.position; // Actualiza la última posición conocida del jugador
                    isReturning = false;
                }
            }
        }
    }



    void AlertState()
    {
        visionAngle = 90f; // Aumentar el ángulo de visión para buscar al jugador más eficazmente

        Vector3 directionToLastSeen = lastPlayerPosition - transform.position; // Calcular dirección hacia la última posición conocida del jugador
        float distanceToLastSeen = directionToLastSeen.magnitude; // Calcular distancia hacia la última posición conocida del jugador

        // Implementa el comportamiento de "arrive" para acercarse a la última posición conocida del jugador
        if (distanceToLastSeen > 0.5f) // Si hay una distancia significativa hasta la última posición conocida
        {
            float speedFactor = Mathf.Min(distanceToLastSeen / 5f, 1f); // Calcular factor de velocidad para desacelerar al acercarse
            float desiredSpeed = speedFactor * pursuitSpeed; // Calcular velocidad deseada
            Vector3 desiredVelocity = directionToLastSeen.normalized * desiredSpeed; // Calcular vector de velocidad deseada
            GetComponent<Rigidbody>().velocity = desiredVelocity; // Aplicar velocidad al guardia
        }
        else
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero; // Detener al guardia si está suficientemente cerca
        }

        // Rotar hacia la última posición conocida del jugador
        if (directionToLastSeen != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToLastSeen);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f); // Suavizar la rotación
        }

        // Verificar si el jugador vuelve a entrar en el campo de visión
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToLastSeen, out hit, visionRadius))
        {
            if (hit.collider.CompareTag("Player")) // Si detecta al jugador
            {
                isAlerted = false; // Salir del estado de alerta
                isAttacking = true; // Entrar en estado de ataque
                isReturning = false;
                visionAngle = 45f; // Restablecer ángulo de visión
               
                return; // Salir del método
            }
        }

        // Si el guardia ha llegado a la última posición conocida y no detecta al jugador, vuelve a su posición inicial
        if (distanceToLastSeen <= 0.5f)
        {
            isReturning = true;
            isAlerted = false;
            isAttacking = false;
        }
    }





    void AttackState()
    {
        Vector3 directionToPlayer = player.position - transform.position; // Calcular dirección hacia el jugador
        transform.position += directionToPlayer.normalized * pursuitSpeed * Time.deltaTime; // Moverse hacia el jugador
        visionAngle = 25; // Reducir ángulo de visión para concentrarse en el ataque

        // Rotar hacia el jugador
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * pursuitSpeed);

        // Cuando el tiempo de ataque se agota, cambia al estado de regreso
        if ((attackDuration -= Time.deltaTime) <= 0)
        {
            isReturning = true;
            isAttacking = false;
            isAlerted = false; // Asegúrate de resetear también isAlerted
            attackDuration = 5f; // Restablecer la duración del ataque para futuros usos
            // No se realiza ninguna otra acción hasta que el guardia haya regresado
        }
    }


    // Método llamado cuando el guardia colisiona con el jugador
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Si el colisionador es el jugador
        {
            Destroy(player1); // Destruir al jugador
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recargar la escena actual
        }
    }


    // Método para volver a la posición inicial del guardia
    void ReturnToInitialPosition()
    {
        isReturning = true;
        isAttacking = false;
        isAlerted = false;
        // Calcula la dirección hacia la posición inicial
        Vector3 directionToInitial = (initialPosition.position - transform.position).normalized;
        // Calcula la distancia a la posición inicial
        float distanceToInitial = Vector3.Distance(transform.position, initialPosition.position);

        // Muestra información de depuración


        // Si la distancia es mayor que un umbral pequeño, mueve al guardia hacia la posición inicial
        if (distanceToInitial > 0.1f)
        {
            // Interpola suavemente la posición del guardia hacia la posición inicial
            transform.position = Vector3.MoveTowards(transform.position, initialPosition.position, pursuitSpeed * Time.deltaTime);
            
            // Rota al guardia para que mire hacia su posición inicial
            Quaternion targetRotation = Quaternion.LookRotation(directionToInitial);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * pursuitSpeed);
        }
        else
        {
            // Si está suficientemente cerca, asegura que el guardia esté exactamente en la posición inicial y restablece su rotación
            transform.position = initialPosition.position;
            transform.rotation = initialPosition.rotation;

            // Restablece el estado del guardia

            isReturning = false;
        }
    }

    void OnDrawGizmos()
    {
        if (isAlerted)
        {
            Gizmos.color = alertedColor;
        }
        else if (isAttacking)
        {
            Gizmos.color = attackingColor;
        }
        else
        {
            Gizmos.color = normalColor;
        }
        // Dibujar el área de visión del guardia


        Vector3 visionDirection = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + visionDirection * visionRadius);
        visionDirection = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + visionDirection * visionRadius);
    }
}