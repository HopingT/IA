using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiltradoController : MonoBehaviour
{
  public float moveSpeed = 5f; // Velocidad de movimiento del infiltrador
    public float stoppingDistance = 0.1f; // Distancia mínima para considerar que ha llegado al objetivo

    private List<Vector3> targetPositions = new List<Vector3>(); // Lista de posiciones hacia las que se dirige el infiltrador
    private int currentTargetIndex = -1; // Índice del destino actual en la lista
    private bool isMoving = false; // Indica si el infiltrador está en movimiento

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetTargetPosition();
        }

        if (isMoving)
        {
            MoveToTarget();
        }
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

            // Si el infiltrador no está en movimiento, comienza a moverse hacia el nuevo destino
            if (!isMoving)
            {
                currentTargetIndex = targetPositions.Count - 1;
                isMoving = true;
            }
            else
            {
                // Si el infiltrador ya está en movimiento, cambia el destino actual
                currentTargetIndex = targetPositions.Count - 1;
            }
        }
    }

    void MoveToTarget()
    {
        // Verifica si hay un destino actual válido
        if (currentTargetIndex >= 0 && currentTargetIndex < targetPositions.Count)
        {
            Vector3 targetPosition = targetPositions[currentTargetIndex];
            transform.LookAt(targetPosition);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Si el infiltrador llega al destino actual, avanza al siguiente destino
            if (Vector3.Distance(transform.position, targetPosition) <= stoppingDistance)
            {
                currentTargetIndex++;

                // Si se han alcanzado todos los destinos, detiene al infiltrador
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
