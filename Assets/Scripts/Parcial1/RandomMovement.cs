using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    // Tiempo m�nimo y m�ximo de espera entre cambios de direcci�n
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    // Velocidad de movimiento de la caja
    public float moveSpeed = 1f;

    // Referencia al transform de la caja
    private Transform boxTransform;

    // Variable para controlar si la caja est� movi�ndose hacia adelante o hacia atr�s
    private bool movingForward = true;

    // Tiempo de espera actual antes de cambiar de direcci�n
    private float currentWaitTime;

    // Tiempo acumulado para esperar entre cambios de direcci�n
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
            // Cambiar de direcci�n
            movingForward = !movingForward;

            // Calcular un nuevo tiempo de espera aleatorio
            currentWaitTime = Random.Range(minWaitTime, maxWaitTime);

            // Reiniciar el tiempo acumulado
            elapsedTime = 0f;
        }

        // Calcular la direcci�n de movimiento en funci�n de la direcci�n actual
        float direction = movingForward ? 1f : -1f;

        // Mover la caja en el eje Z
        boxTransform.Translate(Vector3.forward * direction * moveSpeed * Time.deltaTime);
    }
}