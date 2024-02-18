using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    // Tiempo mínimo y máximo de espera entre cambios de dirección
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    // Velocidad de movimiento de la caja
    public float moveSpeed = 1f;

    // Referencia al transform de la caja
    private Transform boxTransform;

    // Variable para controlar si la caja está moviéndose hacia adelante o hacia atrás
    private bool movingForward = true;

    // Tiempo de espera actual antes de cambiar de dirección
    private float currentWaitTime;

    // Tiempo acumulado para esperar entre cambios de dirección
    private float elapsedTime = 0f;

    void Start()
    {
        // Obtener la referencia al transform de la caja
        boxTransform = transform;

        // Inicializar el tiempo de espera actual
        currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
    }

    void Update()
    {
        // Actualizar el tiempo acumulado
        elapsedTime += Time.deltaTime;

        // Si el tiempo acumulado supera el tiempo de espera actual
        if (elapsedTime >= currentWaitTime)
        {
            // Cambiar de dirección
            movingForward = !movingForward;

            // Calcular un nuevo tiempo de espera aleatorio
            currentWaitTime = Random.Range(minWaitTime, maxWaitTime);

            // Reiniciar el tiempo acumulado
            elapsedTime = 0f;
        }

        // Calcular la dirección de movimiento en función de la dirección actual
        float direction = movingForward ? 1f : -1f;

        // Mover la caja en el eje Z
        boxTransform.Translate(Vector3.forward * direction * moveSpeed * Time.deltaTime);
    }
}