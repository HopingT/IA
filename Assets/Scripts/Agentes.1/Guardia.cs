using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GuardScript : MonoBehaviour
{
    // Referencias y configuraciones del comportamiento del guardia
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
    private bool isAttacking = false; // Indica si el guardia está en estado de ataque
    private bool isReturning = false; // Indica si el guardia está regresando a su posición inicial
    private Vector3 lastPlayerPosition; // Última posición conocida del jugador
    public Color normalColor = Color.yellow; // Color del guardia en estado normal
    public Color alertedColor = Color.red; // Color del guardia en estado de alerta
    public Color attackingColor = Color.magenta; // Color del guardia en estado de ataque

    void Start()
    {
        // Inicializa la última posición conocida del jugador con su posición actual al inicio
        lastPlayerPosition = player.position;
    }

    void Update()
    {
        // Controla el comportamiento del guardia basado en su estado actual
        if (isReturning)
        {
            // Si el guardia está regresando a su posición inicial
            ReturnToInitialPosition();
        }
        else
        {
            // Si el guardia no está alertado ni atacando, realiza la rotación y busca al jugador
            if (!isAlerted && !isAttacking)
            {
                RotateGuard();
                CheckForPlayer();
            }
            // Si el guardia está alertado, entra en estado de alerta
            else if (isAlerted)
            {
                AlertState();
            }
            // Si el guardia está atacando, entra en estado de ataque
            else if (isAttacking)
            {
                AttackState();
            }
        }
    }

    // Rota al guardia en intervalos regulares para simular vigilancia
    void RotateGuard()
    {
        timeSinceLastRotation += Time.deltaTime;
        if (timeSinceLastRotation >= rotationInterval)
        {
            transform.Rotate(Vector3.up, Random.Range(45f, 90f));
            timeSinceLastRotation = 0f;
        }
    }

    // Verifica si el jugador está dentro del campo de visión del guardia
    void CheckForPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer <= visionAngle / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, visionRadius))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    // Si el jugador es detectado, el guardia entra en estado de alerta
                    isAlerted = true;
                    lastPlayerPosition = player.position;
                    isReturning = false;
                }
            }
        }
    }

    // Maneja el comportamiento del guardia cuando está en estado de alerta
    void AlertState()
    {
        visionAngle = 90f; // Aumenta el ángulo de visión para mejorar la detección del jugador
        RaycastHit hit;
        // Calcula la dirección y distancia hacia la última posición conocida del jugador
        Vector3 directionToLastSeen = lastPlayerPosition - transform.position;
        float distanceToLastSeen = directionToLastSeen.magnitude;

        // Si hay una distancia significativa hasta la última posición conocida, el guardia se mueve hacia allí
        if (distanceToLastSeen > 0.5f)
        {
            float speedFactor = Mathf.Min(distanceToLastSeen / 5f, 1f);
            float desiredSpeed = speedFactor * pursuitSpeed;
            Vector3 desiredVelocity = directionToLastSeen.normalized * desiredSpeed;
            GetComponent<Rigidbody>().velocity = desiredVelocity;
        }
        else
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero; // Detiene el guardia si está cerca de la última posición conocida
        }

        // Rotar hacia la última posición conocida del jugador
        if (directionToLastSeen != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToLastSeen);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
        }

        // Verifica si el jugador vuelve a entrar en el campo de visión
        if (Physics.Raycast(transform.position, directionToLastSeen, out hit, visionRadius))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Si detecta al jugador, cambia al estado de ataque
                isAlerted = false;
                isAttacking = true;
                isReturning = false;
                visionAngle = 45f; // Restablece el ángulo de visión
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

    // Maneja el comportamiento del guardia cuando está en estado de ataque
    void AttackState()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        transform.position += directionToPlayer.normalized * pursuitSpeed * Time.deltaTime; // Mueve al guardia hacia el jugador

        visionAngle = 25; // Reduce el ángulo de visión durante el ataque

        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * pursuitSpeed);

        // Finaliza el ataque después de un tiempo determinado y vuelve a la posición inicial
        if ((attackDuration -= Time.deltaTime) <= 0)
        {
            isReturning = true;
            isAttacking = false;
            isAlerted = false;
            attackDuration = 5f; // Restablece la duración del ataque
        }
    }

    // Destruye al jugador si colisiona con el guardia y recarga la escena
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(player1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // Vuelve a la posición inicial del guardia
    void ReturnToInitialPosition()
    {
        isReturning = true;
        isAttacking = false;
        isAlerted = false;

        Vector3 directionToInitial = (initialPosition.position - transform.position).normalized;
        float distanceToInitial = Vector3.Distance(transform.position, initialPosition.position);

        if (distanceToInitial > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPosition.position, pursuitSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(directionToInitial);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * pursuitSpeed);
        }
        else
        {
            transform.position = initialPosition.position;
            transform.rotation = initialPosition.rotation;
            isReturning = false;
        }
    }

    // Visualiza el área de visión del guardia en el editor de Unity
    void OnDrawGizmos()
    {
        Gizmos.color = isAlerted ? alertedColor : isAttacking ? attackingColor : normalColor;
        Vector3 visionDirection = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + visionDirection * visionRadius);
        visionDirection = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + visionDirection * visionRadius);
    }
}
