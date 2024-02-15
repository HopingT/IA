using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SimpleAgent : MonoBehaviour
{
    public float MaxSpeed = 1.0f;
    public float MaxSteeringForce = 1.0f;

    float CurrentTime = 0.0f;
    public Rigidbody rb;

    // Start is called before the first frame update
    void Awake()
    {
        // Lo llamamos en Awake para que sea antes de todos los Start de cualquier script.
        // Lo seteamos en el awake.
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CurrentTime += Time.deltaTime;
        float xPos = Mathf.Sin(CurrentTime);
        float yPos = Mathf.Cos(CurrentTime);
        Vector3 targetPosition = new Vector3(xPos, yPos, 0.0f);

        Vector3 Distance = targetPosition - transform.position;

        Vector3 desiredDirection = Distance.normalized;  // queremos la dirección de ese vector, pero de magnitud 1.

        // queremos ir para esa dirección lo más rápido que se pueda.
        Vector3 desiredVelocity = desiredDirection * MaxSpeed;

        // La diferencia entre la velocidad que tenemos actualmente y la que queremos tener.
        Vector3 steeringForce = desiredVelocity - rb.velocity;

        // Aquí la limitamos a que sea la mínima entre la fuerza que marca el algoritmo y la máxima
        // que deseamos que pueda tener.
        steeringForce = Vector3.Min(steeringForce, steeringForce.normalized * MaxSteeringForce);

        rb.AddForce(steeringForce, ForceMode.Acceleration);

    }

    private void OnDrawGizmos()
    {
        float xPos = Mathf.Sin(CurrentTime);
        float yPos = Mathf.Cos(CurrentTime);
        Vector3 targetPosition = new Vector3(xPos, yPos, 0.0f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 0.2f);
    }

}
