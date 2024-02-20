using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfiltradoController : MonoBehaviour
{

    //------------------------------------------------------------------------------------------------------------
    public enum SteeringBehavior
    {
        None,  // 0
        PointClick,// 1
    };

    public float maxSteeringForce = 1.0f;

    public float maxSpeed = 1.0f;

    // Vector tridimensional para la posición del mouse en el mundo
    Vector3 mouseWorldPos = Vector3.zero;

    // radio del área en que nuestro agente que use arrive va a empezar a reducir su velocidad.
    public float slowAreaRadius = 5.0f;

    // Bandera que determina si nuestro steering behavior activo debe detenerse al llegar al punto objetivo.
    // es decir, usar el arrive steering behavior.
    public bool useArrive = true;

    // Rigidbody ya trae dentro:
    // Vector tridimensional para la aceleración.
    // Vector tridimensional para representar esa velocidad
    private Rigidbody rb;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        maxSpeed = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
        //mouseWorldPos.z = 0;

        //PROJECTO -> MOVER CON CLICK
        //Si das click izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            //Esto se lo tuve que poner por que al apenas iniciar el programa, la capsula salia en direccion a 0,0,0, imagino que dado el vector 
            // inicializado como Vector3.zero
            maxSpeed = 5.0f;
            //Se genera un punto en la pantalla con la posicion donde hiciste click con el mouse
            mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

        }
    }

    private void FixedUpdate()
    {
        Vector3 Distance = Vector3.zero;
        Vector3 steeringForce = Vector3.zero;

        steeringForce = Seek(mouseWorldPos);
        steeringForce = Vector3.Min(steeringForce, steeringForce.normalized * maxSteeringForce);
        rb.AddForce(steeringForce, ForceMode.Acceleration);
    }

    private float ArriveFunction(Vector3 DistanceVector)
    {
        // te dice si el agente está a una distancia Menor que la del radio de la slowing area
        // (área de reducción de velocidad)
        // requisitos: posición del agente, radio del área, posición del objetivo 
        // calcular la distancia entre mi posición y la posición del objetivo.
        float Distance = DistanceVector.magnitude;
        // usamos esa distancia y la comparamos con el radio del área.
        if (Distance < slowAreaRadius)
        {
            // si la distancia es menor que el radio, Entonces en qué porcentage de velocidad
            // debería ir mi agente
            return Distance / slowAreaRadius;
        }
        // sino, que vaya lo más rápido que pueda.
        return 1.0f;
    }

    private Vector3 GetSteeringForce(Vector3 DistanceVector)
    {
        Vector3 desiredDirection = DistanceVector.normalized;  // queremos la dirección de ese vector, pero de magnitud 1.

        // queremos ir para esa dirección lo más rápido que se pueda.
        Vector3 desiredVelocity = desiredDirection * maxSpeed;

        if (useArrive)
        {
            // Si vamos a usar arrive, puede que no querramos ir lo más rápido posible.
            float speedPercentage = ArriveFunction(DistanceVector);
            desiredVelocity *= speedPercentage;
        }

        // La diferencia entre la velocidad que tenemos actualmente y la que queremos tener.
        Vector3 steeringForce = desiredVelocity - rb.velocity;

        return steeringForce;
    }

    private Vector3 Seek(Vector3 TargetPosition)
    {
        return GetSteeringForce(TargetPosition - transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, mouseWorldPos);
    }

    //Fuentes
    // PointClick-> https://www.youtube.com/watch?v=5KLV6QpSAdI
    //
}