using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gizmos_draw : MonoBehaviour
{

    public Transform Agent; // Declaraci�n de una variable p�blica que contendr� una referencia a un objeto de tipo Transform, se asignar� en el inspector de Unity.

    public Transform VisionObject; // Declaraci�n de una variable p�blica que contendr� una referencia a un objeto de tipo Transform, se asignar� en el inspector de Unity.

    [Range(0f, 360f)]
    public float VisionAngle = 30f; // Declaraci�n de un valor flotante p�blico que representa el �ngulo de visi�n, con un rango permitido de 0 a 360 grados, se asignar� en el inspector de Unity.

    public float VisionDistance = 10f; // Declaraci�n de un valor flotante p�blico que representa la distancia de visi�n, se asignar� en el inspector de Unity.

    [SerializeField] bool detected; // Declaraci�n de una variable booleana serializada que indicar� si un objetivo est� detectado.

    Vector3 PointForAngle(float angle, float distance) // Declaraci�n de una funci�n que devuelve un vector en una direcci�n espec�fica basada en un �ngulo y una distancia dados.
    {
        return VisionObject.TransformDirection(
            new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad))) * distance; // C�lculo del vector basado en el �ngulo y la distancia proporcionados.
    }

    private void Update()
    {

        detected = false; // Restablece el estado detectado a falso al comienzo de cada frame.

        Vector2 agentVector = Agent.position - VisionObject.position; // Calcula el vector entre la posici�n del agente y la posici�n del objeto de visi�n.

        if (Vector3.Angle(agentVector.normalized, VisionObject.right) < VisionAngle * 0.5f) // Comprueba si el �ngulo entre el vector del agente y la derecha del objeto de visi�n es menor que la mitad del �ngulo de visi�n.
        {
            if (agentVector.magnitude < VisionDistance) // Comprueba si la magnitud del vector del agente es menor que la distancia de visi�n.
            {
                detected = true; // Establece el estado detectado como verdadero si el agente est� dentro del campo de visi�n y la distancia de visi�n.
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (VisionAngle <= 0f) return; // Si el �ngulo de visi�n es menor o igual a cero, no se dibujar�n gizmos y se sale del m�todo.

        float HalfVisionAngle = VisionAngle * 0.5f; // Calcula la mitad del �ngulo de visi�n.

        Vector2 p1, p2; // Declaraci�n de dos puntos de visi�n.

        p1 = PointForAngle(HalfVisionAngle, VisionDistance); // Calcula el primer punto de visi�n.
        p2 = PointForAngle(-HalfVisionAngle, VisionDistance); // Calcula el segundo punto de visi�n.

        Gizmos.color = detected ? Color.red : Color.green; // Establece el color de los gizmos basado en si el objetivo est� detectado o no.

        Gizmos.DrawLine(VisionObject.position, (Vector2)VisionObject.position + p1); // Dibuja una l�nea desde la posici�n del objeto de visi�n hasta el primer punto de visi�n.
        Gizmos.DrawLine(VisionObject.position, (Vector2)VisionObject.position + p2); // Dibuja una l�nea desde la posici�n del objeto de visi�n hasta el segundo punto de visi�n.

        Gizmos.DrawRay(VisionObject.position, VisionObject.right * 4f); // Dibuja un rayo desde la posici�n del objeto de visi�n hacia la derecha, para representar la direcci�n de visi�n.
    }
}
