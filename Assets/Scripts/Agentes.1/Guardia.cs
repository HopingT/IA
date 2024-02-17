using UnityEngine;

public class ConeVision : MonoBehaviour
{
    public float viewDistance = 10f; // Distancia m�xima de visi�n
    public float viewAngle = 45f; // �ngulo de visi�n c�nica
    public float rotationInterval = 5f; // Intervalo de rotaci�n en segundos
    public float rotationAngle = 45f; // �ngulo de rotaci�n

    private void Start()
    {
        // Comenzar la rotaci�n peri�dica
        InvokeRepeating("RotateGuard", 0f, rotationInterval);
    }

    private void RotateGuard()
    {
        // Rotar el agente Guardia en el eje Y
        transform.Rotate(Vector3.up, rotationAngle);
    }

    private void Update()
    {
        // Obtener la direcci�n hacia adelante del agente
        Vector3 forward = transform.forward;

        // Obtener todos los objetos dentro de la distancia de visi�n
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewDistance);

        // Para cada objetivo dentro de la distancia de visi�n
        foreach (Collider target in targetsInViewRadius)
        {
            // Calcular la direcci�n al objetivo
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;

            // Si el objetivo est� dentro del �ngulo de visi�n
            if (Vector3.Angle(forward, dirToTarget) < viewAngle / 2)
            {
                // Lanzar un rayo hacia el objetivo para comprobar si hay obst�culos
                RaycastHit hit;
                if (Physics.Raycast(transform.position, dirToTarget, out hit, viewDistance))
                {
                    // Si el rayo golpea al objetivo, significa que est� dentro del campo de visi�n
                    if (hit.collider == target)
                    {
                        Debug.DrawLine(transform.position, target.transform.position, Color.green); // Visualizaci�n del rayo
                        // Aqu� puedes agregar la l�gica para manejar el objetivo dentro del campo de visi�n
                        // Por ejemplo, podr�as perseguirlo, dispararle, etc.
                    }
                }
            }
        }
    }


    // M�todo para dibujar el campo de visi�n en el editor de Unity
    private void OnDrawGizmosSelected()
    {
        

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewDistance);
    }
}
