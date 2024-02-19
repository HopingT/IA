using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardScript : MonoBehaviour
{
    public Transform player; // Referencia al infiltrador
    public Collider capsuleC;
    public GameObject player1;
    public float visionAngle = 45f; // Ángulo de visión del guardia
    public float visionRadius = 10f; // Radio de visión del guardia
    public float rotationInterval = 5f; // Intervalo de rotación del guardia
    public float alertDuration = 3f; // Duración del estado de alerta
    public float attackDuration = 5f; // Duración del estado de ataque
    public float pursuitSpeed = 5f; // Velocidad de persecución del guardia
    public Transform initialPosition; // Posición inicial del guardia
    public float attackRange = 0.1f;
    private float timeSinceLastRotation = 0f;
    private bool isAlerted = false;
    private bool isAttacking = false;
    private Vector3 lastPlayerPosition;
    public Color normalColor = Color.yellow;
    public Color alertedColor = Color.red;
    public Color attackingColor = Color.magenta;

    void Start()
    {
        lastPlayerPosition = initialPosition.position;
    }

    void Update()
    {
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

    void RotateGuard()
    {
        timeSinceLastRotation += Time.deltaTime;
        if (timeSinceLastRotation >= rotationInterval)
        {
            // Rotar el guardia en un ángulo aleatorio
            transform.Rotate(Vector3.up, Random.Range(45f, 90f));
            timeSinceLastRotation = 0f;
        }
    }

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
                    isAlerted = true;
                    lastPlayerPosition = player.position;
                }
            }
        }
    }

    void AlertState()
    {
        // Ampliar ligeramente el cono de visión
        visionAngle = 90f; // Ángulo ligeramente más amplio que el normal

        // Calcular la dirección y distancia al jugador
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Moverse hacia la última posición conocida del jugador con Arrive
        Vector3 moveDirection = (lastPlayerPosition - transform.position).normalized;
        float distanceToLastSeen = Vector3.Distance(transform.position, lastPlayerPosition);
        float desiredSpeed = Mathf.Min(distanceToLastSeen / 2f, pursuitSpeed); // Ajustar la velocidad deseada para el Arrive
        Vector3 desiredVelocity = moveDirection * desiredSpeed;
        Vector3 steering = desiredVelocity - GetComponent<Rigidbody>().velocity;
        Vector3 acceleration = Vector3.ClampMagnitude(steering, pursuitSpeed);
        GetComponent<Rigidbody>().velocity += acceleration * Time.deltaTime;

        // Rotar hacia la última posición conocida del jugador
        Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * pursuitSpeed);

        // Verificar si el jugador es visible
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, visionRadius))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Cambiar al estado de ataque si el jugador es visible
                isAlerted = false;
                isAttacking = true;
                visionAngle = 45f; // Restablecer el ángulo de visión
                return;
            }
        }

        // Si el guardia ve la última posición conocida, mantenerse alerta
        if (Vector3.Angle(transform.forward, directionToPlayer) < visionAngle / 2f && distanceToLastSeen <= visionRadius)
        {
            // Temporizador para volver al estado normal si el jugador no es detectado
            alertDuration = 3f; // Reiniciar el temporizador
            return;
        }

        // Temporizador para volver al estado normal si el jugador no es detectado
        if ((alertDuration -= Time.deltaTime) <= 0)
        {
            // Si el jugador no es detectado después de volver a la última posición conocida, regresar a la posición inicial
            ReturnToInitialPosition();
        }
    }




    void AttackState()
    {
        
        // Calculate direction to the player
        Vector3 directionToPlayer = player.position - transform.position;

        // Move towards the player
        transform.position += directionToPlayer.normalized * pursuitSpeed * Time.deltaTime;
        visionAngle = 120;
        // Rotate towards the player
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * pursuitSpeed);

        // Check if the player is within attack range
        float distanceToPlayer = directionToPlayer.magnitude;
        // If attack duration has passed, return to normal state
        if ((attackDuration -= Time.deltaTime) <= 0)
        {
            isAttacking = false;
            attackDuration = 5;
            ReturnToInitialPosition();
            // Optionally: Implement behavior to return to normal state or continue patrolling
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Destroy(player1);
        }
    }


    void ReturnToInitialPosition()
    {
        // Implementar el comportamiento de Arrive para volver a la posición inicial
        Vector3 directionToInitialPosition = initialPosition.position - transform.position;
        float distanceToInitialPosition = directionToInitialPosition.magnitude;

        if (distanceToInitialPosition > 1f) // Threshold to stop close to the position, e.g., 1 meter
        {
            // Move towards the initial position
            Vector3 moveDirection = directionToInitialPosition.normalized;
            transform.position += moveDirection * pursuitSpeed * Time.deltaTime;

            // Rotate towards the initial position
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * pursuitSpeed);
        }
        else
        {
            // If reached initial position, return to normal state
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
        // Dibujar el área de visión del guardia
       

        Vector3 visionDirection = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + visionDirection * visionRadius);
        visionDirection = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + visionDirection * visionRadius);
    }
}
