using UnityEngine;

public class ConeVision : MonoBehaviour
{
    public float viewDistance = 10f; // Distancia máxima de visión
    public float viewAngle = 45f; // Ángulo de visión cónica
    public float rotationInterval = 5f; // Intervalo de rotación en segundos
    public float rotationAngle = 45f; // Ángulo de rotación

    private void Start()
    {
        // Comenzar la rotación periódica
        InvokeRepeating("RotateGuard", 0f, rotationInterval);
    }

    private void RotateGuard()
    {
        // Rotar el agente Guardia en el eje Y
        transform.Rotate(Vector3.up, rotationAngle);
    }

    private void Update()
    {
        // Obtener la dirección hacia adelante del agente
        Vector3 forward = transform.forward;

        // Obtener todos los objetos dentro de la distancia de visión
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewDistance);

        // Para cada objetivo dentro de la distancia de visión
        foreach (Collider target in targetsInViewRadius)
        {
            // Calcular la dirección al objetivo
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;

            // Si el objetivo está dentro del ángulo de visión
            if (Vector3.Angle(forward, dirToTarget) < viewAngle / 2)
            {
                // Lanzar un rayo hacia el objetivo para comprobar si hay obstáculos
                RaycastHit hit;
                if (Physics.Raycast(transform.position, dirToTarget, out hit, viewDistance))
                {
                    // Si el rayo golpea al objetivo, significa que está dentro del campo de visión
                    if (hit.collider == target)
                    {
                        Debug.DrawLine(transform.position, target.transform.position, Color.green); // Visualización del rayo
                        // Aquí puedes agregar la lógica para manejar el objetivo dentro del campo de visión
                        // Por ejemplo, podrías perseguirlo, dispararle, etc.
                    }
                }
            }
        }
    }


    // Método para dibujar el campo de visión en el editor de Unity
    private void OnDrawGizmosSelected()
    {
        

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewDistance);
    }
}
