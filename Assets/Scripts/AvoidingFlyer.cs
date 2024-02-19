using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//Referencia de video https://www.youtube.com/watch?v=SVazwHyfB7g
public class AvoidingFlyers : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidad de movimiento del agente
    public float stoppingDistance = 0.1f; // Distancia m�nima para considerar que ha llegado al objetivo

    public int numberOfRays = 17; // N�mero de rayos utilizados para detectar obst�culos
    public float angle = 90; // �ngulo de barrido de los rayos

    public float rayRange = 2;// Rango de los rayos

    private List<Vector3> targetPositions = new List<Vector3>(); // Lista de posiciones hacia las que se dirige el agente
    private int currentTargetIndex = -1; // �ndice del destino actual en la lista
    private bool isMoving = false; // Indica si el agente est� en movimiento


    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Verifica si se ha hecho clic con el mouse
        {
            SetTargetPosition(); // Establece la posici�n del objetivo
        }

        if (isMoving) // Verifica si el agente est� en movimiento
        {
            MoveToTarget(); // Mueve al agente hacia su objetivo
        }

        // Inicializa la variable deltaPosition que se utilizar� para calcular el movimiento del agente
        var deltaPosition = Vector3.zero;
        for (int i = 0; i < numberOfRays; i++)
        {
            // Calcula la direcci�n del rayo
            var rotation = this.transform.rotation;
            var rotationMod = Quaternion.AngleAxis((i / ((float)numberOfRays - 1)) * angle * 2 - angle, this.transform.up);
            var direccion = rotation * rotationMod * Vector3.forward;

            // Crea un rayo desde la posici�n del agente en la direcci�n calculada
            var ray = new Ray(this.transform.position, direccion);
            RaycastHit hitInfo;
            // Realiza un lanzamiento de rayo para detectar obst�culos
            if (Physics.Raycast(ray, out hitInfo, rayRange))
            {
                deltaPosition -= (1.0f / numberOfRays) * moveSpeed * direccion;
            }
            else
            {
                deltaPosition += (1.0f / numberOfRays) * moveSpeed * direccion;
            }
        }
        // Aplica el movimiento en solitario
        this.transform.position += deltaPosition * Time.deltaTime;
    }

    void SetTargetPosition()
    {
        Plane plane = new Plane(Vector3.up, 0f);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;

        if (plane.Raycast(ray, out distance))
        {
            Vector3 newTargetPosition = ray.GetPoint(distance);
            targetPositions.Add(newTargetPosition);

            // Si el agente no est� en movimiento, comienza a moverse hacia el nuevo destino
            if (!isMoving)
            {
                currentTargetIndex = targetPositions.Count - 1;
                isMoving = true;
            }
            else
            {
                // Si el agente ya est� en movimiento, cambia el destino actual
                currentTargetIndex = targetPositions.Count - 1;
            }
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < numberOfRays; i++)
        {
            // Calcula la direcci�n del rayo
            var rotation = this.transform.rotation;
            var rotationMod = Quaternion.AngleAxis((i / ((float)numberOfRays - 1)) * angle * 2 - angle, this.transform.up);
            var direccion = rotation * rotationMod * Vector3.forward;
            Gizmos.DrawRay(this.transform.position, direccion);
        }
    }

    void MoveToTarget()
    {
        // Verifica si hay un destino actual v�lido
        if (currentTargetIndex >= 0 && currentTargetIndex < targetPositions.Count)
        {
            Vector3 targetPosition = targetPositions[currentTargetIndex];
            transform.LookAt(targetPosition);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Si el agente llega al destino actual, avanza al siguiente destino
            if (Vector3.Distance(transform.position, targetPosition) <= stoppingDistance)
            {
                currentTargetIndex++;

                // Si se han alcanzado todos los destinos, detiene al agente
                if (currentTargetIndex >= targetPositions.Count)
                {
                    isMoving = false;
                    targetPositions.Clear();
                    currentTargetIndex = -1;
                }
            }
        }
    }
}