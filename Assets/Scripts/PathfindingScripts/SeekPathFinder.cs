using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class SeekPathfinder : MonoBehaviour
{
    [SerializeField] NodeVisualizer[] nodeVisualizers;
    public enum SteeringBehavior
    {
        None,  // 0
        Seek,   // 1
        MAX     // 2
    };

    public SteeringBehavior currentBehavior = SteeringBehavior.Seek;

    // Bandera que determina si nuestro steering behavior activo debe detenerse al llegar al punto objetivo.
    // es decir, usar el arrive steering behavior.
    public bool useArrive = true;

    // radio del �rea en que nuestro agente que use arrive va a empezar a reducir su velocidad.
    public float slowAreaRadius = 5.0f;

    public float maxSpeed = 1.0f;

    // Qu� tanto tiempo queremos que pase antes de aplicar toda la steering force.
    public float maxSteeringForce = 1.0f;

    // Rigidbody ya trae dentro:
    // Vector tridimensional para la aceleraci�n.
    // Vector tridimensional para representar esa velocidad
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        // Le pasamos el rigidbody al rb para que funcione el seek
        rb = GetComponent<Rigidbody>();

        //TargetGameObject = FindAnyObjectByType<NodeVisualizer>().gameObject;
        //rbTargetGameObject = TargetGameObject.GetComponent<Rigidbody>();
    }


    // Update is called once per frame
    protected void Update()
    {


    }

    void FixedUpdate()
    {
        // Buscamos todos los objetos en la escena que tengan el componente NodeVisualizer
        nodeVisualizers = FindObjectsOfType<NodeVisualizer>();
        Vector3 Distance = Vector3.zero;
        Vector3 steeringForce = Vector3.zero;

        //Por cada visualizador de nodo en la lista de visualizadores de nodos
        foreach (NodeVisualizer nodeVisualizer in nodeVisualizers)
        {
            // Calculamos la direcci�n hacia el nodo
            Vector3 direction = nodeVisualizer.transform.position;
            // La declaramos aqu� para poder usarla DENTRO del switch, pero que siga viva al salir del switch.
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
                        // En qu� direcci�n vamos a hacer que se mueva nuestro agente? En la direcci�n en la que est� el mouse.
                        // Cuando hablemos de direcci�n, queremos vectores normalizados (es decir, de magnitude 1).
                        //Hacemos seek al transform del visualizador de nodo guardado en direction
                        steeringForce = Seek(direction);
                        break;
                    }
                case SteeringBehavior.MAX:
                    break;
            }

        }
        // Aqu� la limitamos a que sea la m�nima entre la fuerza que marca el algoritmo y la m�xima
        // que deseamos que pueda tener.
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
        // te dice si el agente est� a una distancia Menor que la del radio de la slowing area
        // (�rea de reducci�n de velocidad)
        // requisitos: posici�n del agente, radio del �rea, posici�n del objetivo 
        // calcular la distancia entre mi posici�n y la posici�n del objetivo.
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

    //Fuentes
    //Codigo del profe realizado en clase
    //Mi intuicion�jaja
}