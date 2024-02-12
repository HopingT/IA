using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gizmos_draw : MonoBehaviour
{

    public Transform Agent; // Declaración de una variable pública que contendrá una referencia a un objeto de tipo Transform, se asignará en el inspector de Unity.

    public Transform VisionObject; // Declaración de una variable pública que contendrá una referencia a un objeto de tipo Transform, se asignará en el inspector de Unity.

    [Range(0f, 360f)]
    public float VisionAngle = 30f; // Declaración de un valor flotante público que representa el ángulo de visión, con un rango permitido de 0 a 360 grados, se asignará en el inspector de Unity.

    public float VisionDistance = 10f; // Declaración de un valor flotante público que representa la distancia de visión, se asignará en el inspector de Unity.

    [SerializeField] bool detected; // Declaración de una variable booleana serializada que indicará si un objetivo está detectado.

    Vector3 PointForAngle(float angle, float distance) // Declaración de una función que devuelve un vector en una dirección específica basada en un ángulo y una distancia dados.
    {
        return VisionObject.TransformDirection(
            new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad))) * distance; // Cálculo del vector basado en el ángulo y la distancia proporcionados.
    }

    private void Update()
    {

        detected = false; // Restablece el estado detectado a falso al comienzo de cada frame.

        Vector2 agentVector = Agent.position - VisionObject.position; // Calcula el vector entre la posición del agente y la posición del objeto de visión.

        if (Vector3.Angle(agentVector.normalized, VisionObject.right) < VisionAngle * 0.5f) // Comprueba si el ángulo entre el vector del agente y la derecha del objeto de visión es menor que la mitad del ángulo de visión.
        {
            if (agentVector.magnitude < VisionDistance) // Comprueba si la magnitud del vector del agente es menor que la distancia de visión.
            {
                detected = true; // Establece el estado detectado como verdadero si el agente está dentro del campo de visión y la distancia de visión.
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (VisionAngle <= 0f) return; // Si el ángulo de visión es menor o igual a cero, no se dibujarán gizmos y se sale del método.

        float HalfVisionAngle = VisionAngle * 0.5f; // Calcula la mitad del ángulo de visión.

        Vector2 p1, p2; // Declaración de dos puntos de visión.

        p1 = PointForAngle(HalfVisionAngle, VisionDistance); // Calcula el primer punto de visión.
        p2 = PointForAngle(-HalfVisionAngle, VisionDistance); // Calcula el segundo punto de visión.

        Gizmos.color = detected ? Color.red : Color.green; // Establece el color de los gizmos basado en si el objetivo está detectado o no.

        Gizmos.DrawLine(VisionObject.position, (Vector2)VisionObject.position + p1); // Dibuja una línea desde la posición del objeto de visión hasta el primer punto de visión.
        Gizmos.DrawLine(VisionObject.position, (Vector2)VisionObject.position + p2); // Dibuja una línea desde la posición del objeto de visión hasta el segundo punto de visión.

        Gizmos.DrawRay(VisionObject.position, VisionObject.right * 4f); // Dibuja un rayo desde la posición del objeto de visión hacia la derecha, para representar la dirección de visión.
    }
}
