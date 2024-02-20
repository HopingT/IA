using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GuardScript : MonoBehaviour
{
    // Variables p�blicas para configurar el comportamiento del guardia desde el editor de Unity
    public Transform player; // Referencia al jugador para poder detectarlo
    public Collider capsuleC; // Colisionador del guardia, no se utiliza en este script
    public GameObject player1; // Objeto del jugador, utilizado para destruir al jugador si es necesario
    public float visionAngle = 45f; // �ngulo de visi�n del guardia para detectar al jugador
    public float visionRadius = 5f; // Radio de visi�n del guardia para detectar al jugador
    public float rotationInterval = 5f; // Tiempo entre rotaciones del guardia cuando no est� en alerta
    public float alertDuration = 3f; // Duraci�n del estado de alerta, no se utiliza en este script
    public float attackDuration = 5f; // Duraci�n del estado de ataque
    public float pursuitSpeed = 5f; // Velocidad a la que el guardia persigue al jugador
    public Transform initialPosition; // Posici�n inicial del guardia para volver a ella si es necesario
    public float attackRange = 0.1f; // Rango de ataque del guardia, no se utiliza en este script
    private float timeSinceLastRotation = 0f; // Temporizador para controlar la rotaci�n del guardia
    private bool isAlerted = false; // Indica si el guardia est� en estado de alerta
    private bool isAttacking = false; // Indica si el guardia est� atacando
    private Vector3 lastPlayerPosition; // �ltima posici�n conocida del jugador
    public Color normalColor = Color.yellow; // Color del guardia en estado normal
    public Color alertedColor = Color.red; // Color del guardia en estado de alerta
    public Color attackingColor = Color.magenta; // Color del guardia en estado de ataque

    // M�todo Start llamado al inicio para inicializar variables
    void Start()
    {
        lastPlayerPosition = player.position; // Inicializa la �ltima posici�n conocida del jugador
    }

    // M�todo Update llamado en cada frame para actualizar el estado del guardia
    void Update()
    {
        // Si el guardia no est� alertado ni atacando, rota y busca al jugador
        if (!isAlerted && !isAttacking)
        {
            RotateGuard();
            CheckForPlayer();
            visionAngle = 45f; // Restablece el �ngulo de visi�n, aunque ya se inicializa con este valor
        }
        // Si el guardia est� alertado, entra en estado de alerta
        else if (isAlerted)
        {
            AlertState();
        }
        // Si el guardia est� atacando, entra en estado de ataque
        else if (isAttacking)
        {
            AttackState();
            visionAngle = 25f; // Reduce el �ngulo de visi�n para concentrarse en el ataque
        }
    }

    // M�todo para rotar al guardia en intervalos regulares
    void RotateGuard()
    {
        timeSinceLastRotation += Time.deltaTime; // Aumenta el temporizador basado en el tiempo real
        if (timeSinceLastRotation >= rotationInterval)
        {
            // Rotar el guardia en un �ngulo aleatorio entre 45 y 90 grados
            transform.Rotate(Vector3.up, Random.Range(45f, 90f));
            timeSinceLastRotation = 0f; // Restablecer el temporizador
        }
    }

    // M�todo para detectar al jugador dentro del campo de visi�n
    void CheckForPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position; // Calcular direcci�n hacia el jugador
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer); // Calcular �ngulo hacia el jugador
        if (angleToPlayer <= visionAngle / 2f) // Si el jugador est� dentro del �ngulo de visi�n
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, visionRadius)) // Lanzar un rayo hacia el jugador
            {
                if (hit.collider.CompareTag("Player")) // Si el rayo colisiona con el jugador
                {
                    isAlerted = true; // El guardia entra en estado de alerta
                    lastPlayerPosition = player.position; // Actualiza la �ltima posici�n conocida del jugador
                } else
                {
                    ReturnToInitialPosition();
                }
                    
            }
        }
    }



    void AlertState()
    {
        visionAngle = 90f; // Aumentar el �ngulo de visi�n para buscar al jugador m�s eficazmente

        Vector3 directionToLastSeen = lastPlayerPosition - transform.position; // Calcular direcci�n hacia la �ltima posici�n conocida del jugador
        float distanceToLastSeen = directionToLastSeen.magnitude; // Calcular distancia hacia la �ltima posici�n conocida del jugador

        // Implementa el comportamiento de "arrive" para acercarse a la �ltima posici�n conocida del jugador
        if (distanceToLastSeen > 0.5f) // Si hay una distancia significativa hasta la �ltima posici�n conocida
        {
            float speedFactor = Mathf.Min(distanceToLastSeen / 5f, 1f); // Calcular factor de velocidad para desacelerar al acercarse
            float desiredSpeed = speedFactor * pursuitSpeed; // Calcular velocidad deseada
            Vector3 desiredVelocity = directionToLastSeen.normalized * desiredSpeed; // Calcular vector de velocidad deseada
            GetComponent<Rigidbody>().velocity = desiredVelocity; // Aplicar velocidad al guardia
        }
        else
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero; // Detener al guardia si est� suficientemente cerca
        }

        // Rotar hacia la �ltima posici�n conocida del jugador
        if (directionToLastSeen != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToLastSeen);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f); // Suavizar la rotaci�n
        }

        // Verificar si el jugador vuelve a entrar en el campo de visi�n
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToLastSeen, out hit, visionRadius))
        {
            if (hit.collider.CompareTag("Player")) // Si detecta al jugador
            {
                isAlerted = false; // Salir del estado de alerta
                isAttacking = true; // Entrar en estado de ataque
                visionAngle = 45f; // Restablecer �ngulo de visi�n
                visionRadius = 5f; // Restablecer radio de visi�n
                return; // Salir del m�todo
            }
        }

        // Si el guardia ha llegado a la �ltima posici�n conocida y no detecta al jugador, vuelve a su posici�n inicial
        if (distanceToLastSeen <= 0.2f)
        {
            ReturnToInitialPosition();
        }
    }





    void AttackState()
    {
        Vector3 directionToPlayer = player.position - transform.position; // Calcular direcci�n hacia el jugador
        transform.position += directionToPlayer.normalized * pursuitSpeed * Time.deltaTime; // Moverse hacia el jugador
        visionAngle = 25; // Reducir �ngulo de visi�n para concentrarse en el ataque

        // Rotar hacia el jugador
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * pursuitSpeed);

        // Calcular distancia al jugador
        float distanceToPlayer = directionToPlayer.magnitude;
        if ((attackDuration -= Time.deltaTime) <= 0) // Reducir duraci�n del ataque y verificar si ha terminado
        {
            isAttacking = false; // Salir del estado de ataque
            attackDuration = 5; // Restablecer duraci�n del ataque
            ReturnToInitialPosition(); // Volver a la posici�n inicial
        }
    }
    // M�todo llamado cuando el guardia colisiona con el jugador
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Si el colisionador es el jugador
        {
            Destroy(player1); // Destruir al jugador
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recargar la escena actual
        }
    }


    // M�todo para volver a la posici�n inicial del guardia
    void ReturnToInitialPosition()
    {
        Rigidbody rb = GetComponent<Rigidbody>(); // Obtiene el componente Rigidbody del guardia
        Vector3 directionToInitial = initialPosition.position - transform.position; // Calcula la direcci�n hacia la posici�n inicial
        float distanceToInitial = directionToInitial.magnitude; // Calcula la distancia a la posici�n inicial

        // Verifica si el guardia est� suficientemente cerca de la posici�n inicial
        if (distanceToInitial>0.1)
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPosition.position, pursuitSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(directionToInitial);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * pursuitSpeed);
        }
        else
        {
            // Si est� suficientemente cerca, detiene al guardia y lo coloca exactamente en la posici�n inicial
            rb.velocity = Vector3.zero; // Detiene el movimiento del guardia
            transform.position = initialPosition.position; // Coloca al guardia en la posici�n inicial
            transform.rotation = initialPosition.rotation; // Restablece la rotaci�n del guardia a su estado inicial
            
            // Restablece el estado del guardia
            isAttacking = false;
            isAlerted = false;
        }
    }




    void OnDrawGizmosSelected()
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
        // Dibujar el �rea de visi�n del guardia
       

        Vector3 visionDirection = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + visionDirection * visionRadius);
        visionDirection = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + visionDirection * visionRadius);
        if (player != null)
        {
            // Cambiar el color de los Gizmos para la trayectoria
            Gizmos.color = Color.blue; // Puedes cambiar este color a lo que prefieras
                                       // Dibujar una l�nea desde la posici�n del guardia hasta la posici�n del jugador
            Gizmos.DrawLine(transform.position, player.position);
        }
        if (initialPosition != null)
        {
            Gizmos.color = Color.green; // Color para la posici�n inicial, cambia a lo que prefieras
                                        // Dibujar una esfera peque�a en la posici�n inicial
            Gizmos.DrawSphere(initialPosition.position, 0.5f); // Ajusta el tama�o de la esfera seg�n necesites
        }
    }
}
