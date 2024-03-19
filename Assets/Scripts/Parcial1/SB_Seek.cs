using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



public class SB_Seek : MonoBehaviour
{
    public enum SteeringBehavior
    {
        None,  // 0
        Seek,   // 1
        Flee,  // 2
        Pursuit, // 3
        Evade,  // 4
        Wander, // 5
        MAX     // 6
    };

    public SteeringBehavior currentBehavior = SteeringBehavior.Seek;


    public bool useArrive = true;

    // radio del �rea en que nuestro agente que use arrive va a empezar a reducir su velocidad.
    public float slowAreaRadius = 5.0f;

    // Vector tridimensional para la posici�n del mouse en el mundo
    Vector3 mouseWorldPos = Vector3.zero;

    public float maxSpeed = 1.0f;

    // Qu� tanto tiempo queremos que pase antes de aplicar toda la steering force.
    public float maxSteeringForce = 1.0f;

    private Rigidbody rb;

    // Variable donde guardamos la referencia al GameObject que es nuestro objetivo.
    private GameObject TargetGameObject;
    // Referencia al Rigidbody del TargetGameObject.
    private Rigidbody rbTargetGameObject;

    private Vector3 TargetPosition = Vector3.zero;

    // Variables para Wander.
    public float sphereDistance = 1.0f;
    public float sphereRadius = 5.0f;


    void Start()
    {
        print("Funcion Start");
        rb = GetComponent<Rigidbody>();

        TargetGameObject = FindAnyObjectByType<SimpleAgent>().gameObject;
        rbTargetGameObject = TargetGameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    protected void Update()
    {
        // Lo que est� dentro de la funci�n update, se va a ejecutar cada que se pueda.
        // print("Funcion update");

        // Input.mousePosition // Nos da coordenadas en pixeles.
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);

        mouseWorldPos.z = 0;  // la sobreescribimos con 0 para que no afecte los c�lculos en 2D.

        // print(this.name);
        print(base.GetType());

    }

    int myCounter = 0;
    void FixedUpdate()
    {

        // La declaramos aqu� para poder usarla DENTRO del switch, pero que siga viva al salir del switch.
        Vector3 Distance = Vector3.zero;
        Vector3 steeringForce = Vector3.zero;

        // Seg�n el valor de la variable currentBehavior, es cu�l Steering Behavior vamos a ejecutar.
        switch (currentBehavior)
        {
            case SteeringBehavior.None:
                {
                    return;
                    // break;
                }
            case SteeringBehavior.Seek:
                {

                    steeringForce = Seek(mouseWorldPos);
                    break;
                }
            case SteeringBehavior.Flee:
                {

                    steeringForce = Flee(mouseWorldPos);
                    break;
                }
            case SteeringBehavior.Pursuit:
                {
                    steeringForce = Pursuit(TargetGameObject.transform.position, rbTargetGameObject.velocity);
                }
                break;
            case SteeringBehavior.Evade:
                {
                    steeringForce = Evade(TargetGameObject.transform.position, rbTargetGameObject.velocity);
                }
                break;
            case SteeringBehavior.Wander:
                {

                    Vector3 LookingDirection = rb.velocity.normalized;
                    // a partir de la posici�n de nuestro agente, la esfera va a estar 
                    // desplazada sphereDistance en la direcci�n de LookingDirection
                    Vector3 spherePosition = gameObject.transform.position + LookingDirection * sphereDistance;
                    Vector3 unitCircle = Random.insideUnitCircle;
                    Vector3 TargetInsideSphere = spherePosition + (unitCircle * sphereRadius);
                }
                break;
            case SteeringBehavior.MAX:
                break;
        }

        steeringForce = Vector3.Min(steeringForce, steeringForce.normalized * maxSteeringForce);

        rb.AddForce(steeringForce, ForceMode.Acceleration);
    }

    private Vector3 GetSteeringForce(Vector3 DistanceVector)
    {
        Vector3 desiredDirection = DistanceVector.normalized;  // queremos la direcci�n de ese vector, pero de magnitud 1.

        // queremos ir para esa direcci�n lo m�s r�pido que se pueda.
        Vector3 desiredVelocity = desiredDirection * maxSpeed;

        if (useArrive)
        {
            // Si vamos a usar arrive, puede que no querramos ir lo m�s r�pido posible.
            float speedPercentage = ArriveFunction(DistanceVector);
            desiredVelocity *= speedPercentage;
        }

        // La diferencia entre la velocidad que tenemos actualmente y la que queremos tener.
        Vector3 steeringForce = desiredVelocity - rb.velocity;

        return steeringForce;
    }

    private float ArriveFunction(Vector3 DistanceVector)
    {

        float Distance = DistanceVector.magnitude;
        // usamos esa distancia y la comparamos con el radio del �rea.
        if (Distance < slowAreaRadius)
        {
            // si la distancia es menor que el radio, Entonces en qu� porcentage de velocidad
            // deber�a ir mi agente
            return Distance / slowAreaRadius;
        }
        // sino, que vaya lo m�s r�pido que pueda.
        return 1.0f;
    }

    private Vector3 Seek(Vector3 TargetPosition)
    {
        return GetSteeringForce(TargetPosition - transform.position);
    }

    private Vector3 Flee(Vector3 TargetPosition)
    {
        return GetSteeringForce(transform.position - TargetPosition);
    }

    private Vector3 Pursuit(Vector3 TargetPosition, Vector3 TargetVelocity)
    {
        Vector3 predictedPosition = PredictPosition(TargetPosition, TargetVelocity);

        return Seek(predictedPosition);
    }

    private Vector3 Evade(Vector3 TargetPosition, Vector3 TargetVelocity)
    {
        Vector3 predictedPosition = PredictPosition(TargetPosition, TargetVelocity);

        return Flee(predictedPosition);
    }

    private Vector3 PredictPosition(Vector3 TargetPosition, Vector3 TargetVelocity)
    {

        Vector3 Distance = transform.position - TargetPosition;

        float predictedTime = Distance.magnitude / maxSpeed;

        Vector3 predictedPosition = TargetPosition + TargetVelocity * predictedTime;

        return predictedPosition;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, mouseWorldPos);

        // Comprobamos que s� lo hace.
        Gizmos.color = Color.red;

        Gizmos.color = Color.white;
        // Gizmos.DrawSphere(TargetPosition, 1);

        if (rb != null)
        {
            Vector3 LookingDirection = rb.velocity.normalized;
            // Esto de aqu� nos va a dar una l�nea chiquita.
            Gizmos.DrawLine(transform.position, transform.position + LookingDirection);

            Vector3 spherePosition = transform.position + LookingDirection * sphereDistance;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, spherePosition);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spherePosition, sphereRadius);

            Vector3 unitCircle = Random.onUnitSphere;
            // Punto en el c�rculo/esfera al cual vamos a hacer seek.
            Vector3 TargetInsideSphere = spherePosition + (unitCircle * sphereRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(TargetInsideSphere, 1.0f);
        }
    }
}
