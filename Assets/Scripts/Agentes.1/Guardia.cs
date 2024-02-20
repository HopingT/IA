using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline;
using UnityEngine;

public class Guardia : MonoBehaviour
{
    //Todo para que el arrive funcione
    public enum SteeringBehavior
    {
        None,  // 0
        PointClick,// 1
        Pursuit, //2
    };
    public float maxSteeringForce = 1.0f;
    public float maxSpeed = 1.0f;
    public Rigidbody rb;
    public bool useArrive = true;
    public float slowAreaRadius = 5.0f;
    // Variable donde guardamos la referencia al GameObject que es nuestro objetivo.
    private GameObject TargetGameObject;
    // Referencia al Rigidbody del TargetGameObject.
    private Rigidbody rbTargetGameObject;
    //-----------------------------------------------------------
    private Vector3 PosicionInicialAgente;
    public GameObject agentPrefab;
    //--------------------------------------------------------------
    //Establecemos la posicion inicial de nuestro guardia como un vector 3
    private Vector3 posicionInicial;
    //Establecemos la ultima posicion detectada de nuestro agente como un vector 3
    private Vector3 ultimaPosicionDetectada;
    // Todo para que cambie de color en los diferentes estados
    //Accedemos al render de nuestro objeto
    private Renderer RendererObject;
    //Dependiendo de en que estado vaya a estar le modificamos su color
    public Color ColorNormal = Color.green;
    public Color ColorAlerta = Color.yellow;
    public Color ColorAtaque = Color.red;
    //----------------------------------------------------------------------

    //Generamos un cronometro para que pasado cierto tiempo cuando nuestro guardia este en alerta si no ha visto de nuevo a nuestro agente, vuelva a 
    //Su estado normal
    [SerializeField] private float CuentaRegresiva;
    //Generamos un cronometro de cada cuanto tiempo va a rotar solo en el estado normal
    [SerializeField] private float CronometroRotacion = 5f;
    //Generamos un cronometro de deteccion (Si el jugador permanece cierta cantidad de tiempo en el rango se cambiara de estado)
    [SerializeField] private float CronometroEstado = 0f;
    //Generamos un cronometro especificamente para el pursuit
    [SerializeField] private float CronometroPursuit = 0f;

    //Hacemos una lista con todos los posibles estados
    public enum Estados
    {
        None,
        Normal,
        Alerta,
        Ataque
    }
    //Generamos una variable que establecera el estado actual
    public Estados EstadoActual = Estados.None;


    //ANTES DEL EXAMEN
    //------------------------------------------------------------------------------------------------------------------------------------------
    // Ubicamos la posicion de nuestro objeto a detectar
    public Transform Agent;

    // Ubicamos la posicion de nuestro objeto detector
    public Transform VisionObject;

    // Establecemos un angulo de vision para nuestro objeto detector
    // Ademas a�adimos un rango de giro entre 0 y 360 gradfos que representan una vuelta completa
    [Range(0f, 360f)]
    public float VisionAngle = 30f;

    // Establecemos la distancia maxima hasta donde se va a poder ubicar o detectar a nuestro agente
    public float VisionDistance = 10f;

    // Creamos un booleano para poder identificar cuando hemos o no detectado a nuestro agente ademas de hacerlo visible en el inspector 
    // por cualquier inconveniente que pueda ocurrir
    [SerializeField] bool detected;

    // Declaramos un Vector3 que seran posteriormente los puntos a partir de donde se dividira el angulo de vision en 2 y asi obtener mitades 
    // para facilitar la deteccion del agente, para esto necesitamos el angulo y distancia maxima 
    Vector3 PointForAngle(float angle, float distance)
    {
        // Aqui regresamos que la vision del objeto este situada justo en el o junto al VisionObject y para esto ocupamos el TransformDirecion
        // (De lo contrario este angulo apareceria en otra ubicacion del entorno)
        return VisionObject.TransformDirection(

            // Creamos un nuevo vector2 y ocupando trigonometria y formulas matematicas determinamos el coseno y seno del angulo total para
            // posteriormente convertirlo a radianes y finalmente multiplicar ambos resultados por la distancia maxima de vision
            new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad))) * distance;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        //Accedemos al componente renderer
        RendererObject = GetComponent<Renderer>();
        //Establecemos el color inicial
        RendererObject.material.color = ColorNormal;
        //Adquitimos la posicion inicial
        posicionInicial = transform.position;
        //Encontramos a nuestro objeto con el script MouseClick
        TargetGameObject = FindAnyObjectByType<InfiltradoController>().gameObject;
        //Adquirimos su rigidbody del objeto
        rbTargetGameObject = TargetGameObject.GetComponent<Rigidbody>();
        //Establecemos la posicion inical del infiltrador
        PosicionInicialAgente = TargetGameObject.transform.position;
    }

    private void Update()
    {
        // Establecemos que siempre se intente establecer que el agente no fue detectado
        detected = false;

        // Creamos un vector 2 para el agente que servira para detectar cuando el mismo este dentro del angulo y rango de deteccion
        // del VisionObject
        Vector2 agentVector = Agent.position - VisionObject.position;

        // Si el angulo que se fotma entre el agentVector y la posicion derecha del visionObject es menor a la mitad del visionAngle y ...
        if (Vector3.Angle(agentVector.normalized, VisionObject.right) < VisionAngle * 0.5f)
        {
            // ... Si la magnitud/tama�o del vector entre el agente y el VisionObject es menor a la distancia de vision del VisionObject 
            // se establece que ha sido detectado el agente
            if (agentVector.magnitude < VisionDistance)
            {
                detected = true;
            }
        }

        //Si ha sido detectado
        if (detected == true)
        {
            //El cronometro empezara a sumar 
            CronometroEstado += Time.deltaTime;
            //La cuenta regresiva se restablece
            CuentaRegresiva = 0;
            //Cambiamos la ultima posicion detectada a la agent.position
            ultimaPosicionDetectada = Agent.position;
        }//Si no ha sido detectado
        else if (detected == false)
        {
            //La cuenta regresiva estara actuando
            CuentaRegresiva += Time.deltaTime;
        }

        //Si entramos en estado de ataque comienza el cronometro de pursuit
        if (EstadoActual == Estados.Ataque)
        {
            CronometroPursuit += Time.deltaTime;
        }

        //Con el espacio hacemos que vuelvas a activar al personaje
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CrearNuevoAgente();
        }


    }

    private void FixedUpdate()
    {
        //Si la cuenta regresiva es mayor o igual a 5
        if (CuentaRegresiva >= 5)
        {
            //El estado siempre va a ser estado normal
            CambiarEstado(Estados.Normal);
            //La cuenta regresiva regresara a ser 0 una vez el estado normal este activo
            CuentaRegresiva = 0;
            CronometroEstado = 0;
        }

        if (CronometroPursuit >= 5)
        {
            //El estado siempre va a ser estado normal
            CambiarEstado(Estados.Normal);
            //La cuenta regresiva regresara a ser 0 una vez el estado normal este activo
            CuentaRegresiva = 0;
            //El cronometro de estado y pursuit igual se restablece
            CronometroEstado = 0;
            CronometroPursuit = 0;
        }

        //Si el cronometro de estado es igual o menor a 0 estamos en estado normal, si es mayor a 1 pero menor que 2 estamos en estado alerta
        //Y si es mayor a 2 pasamos al estado de ataque
        if (CronometroEstado <= 0)
        {
            CambiarEstado(Estados.Normal);
        }
        else if (CronometroEstado >= 1 && CronometroEstado < 2)
        {
            CambiarEstado(Estados.Alerta);
        }
        else if (CronometroEstado >= 2)
        {
            CambiarEstado(Estados.Ataque);
        }


    }

    //Creamos un void que nos servira para poder cambiar de estados mediante eventos o variables del juego
    private void CambiarEstado(Estados nuevoEstado)
    {
        Vector3 Distance = Vector3.zero;
        Vector3 steeringForce = Vector3.zero;
        //Cambiamos el estado actual al nuevo estado y de esta forma el nuevo estado nos permitira cambiar entre estados
        EstadoActual = nuevoEstado;
        switch (EstadoActual)
        {
            case Estados.Normal:
                Rotar();
                RendererObject.material.color = ColorNormal;
                VisionAngle = 30;
                steeringForce = Seek(posicionInicial);
                break;
            case Estados.Alerta:
                steeringForce = Seek(ultimaPosicionDetectada);
                RendererObject.material.color = ColorAlerta;
                VisionAngle = 60f;
                break;
            case Estados.Ataque:
                RendererObject.material.color = ColorAtaque;
                steeringForce = Pursuit(TargetGameObject.transform.position, rbTargetGameObject.velocity);

                break;
        }
        steeringForce = Vector3.Min(steeringForce, steeringForce.normalized * maxSteeringForce);

        rb.AddForce(steeringForce, ForceMode.Acceleration);
    }

    //Funcion de arrive del profe
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

    //Funcion de GetSteeringForce del profe
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

    //Funcion de Seek del profe
    private Vector3 Seek(Vector3 TargetPosition)
    {
        return GetSteeringForce(TargetPosition - transform.position);
    }

    //Funcion pursuit del profe
    private Vector3 Pursuit(Vector3 TargetPosition, Vector3 TargetVelocity)
    {
        Vector3 predictedPosition = PredictPosition(TargetPosition, TargetVelocity);

        return Seek(predictedPosition);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (EstadoActual == Estados.Ataque)
        {
            TargetGameObject.SetActive(false);
            //Destroy(TargetGameObject);
            CambiarEstado(Estados.Normal);
            CronometroPursuit = 5;
            //CrearNuevoAgente();
        }


    }

    private void CrearNuevoAgente()
    {
        /*GameObject nuevoAgente = Instantiate(agentPrefab, PosicionInicialAgente, Quaternion.identity);
        Agent = nuevoAgente.transform;*/
        //TargetGameObject.SetActive(true);

        if (!Agent.gameObject.activeSelf)
        {
            // Reactivar el agente
            Agent.gameObject.SetActive(true);
            // Reiniciar la posici�n del agente a la posici�n inicial
            Agent.position = PosicionInicialAgente;
        }
    }

    //Generamos una funcion que nos permitira rotar el objeto pasados 5 segundos en el cronometro de la rotacion
    private void Rotar()
    {

        if (CronometroRotacion <= 0f)
        {
            transform.Rotate(0, 0, 45, 0);
            CronometroRotacion += 5f;
        }
        else if (CronometroRotacion > 0f)
        {
            CronometroRotacion -= Time.deltaTime;
        }
    }

    //Funcion de predecir posicion del profe
    private Vector3 PredictPosition(Vector3 TargetPosition, Vector3 TargetVelocity)
    {
        // Pursuit no es mucho m�s que hacerle Seek a la posici�n futura del objetivo.
        // Primero calculamos el tiempo T que nos tomar�a llegar al TargetPosition.
        Vector3 Distance = transform.position - TargetPosition;
        // con esa distancia, podemos saber cu�nto tiempo nos tomar� recorrer esa 
        // distancia usando nuestra m�xima velocidad.
        // TiempoT = Distancia/MaxSpeed
        float predictedTime = Distance.magnitude / maxSpeed;
        // usamos Distance.magnitude porque queremos cu�nto mide el vector, no hacia d�nde (o no hacia qu� direcci�n).
        // Ahora s� podemos predecir la posici�n futura de nuestro TargetObject.
        // Su posici�n futura es: Su posici�n actual + su velocidad * cu�nto tiempo transcurre.
        Vector3 predictedPosition = TargetPosition + TargetVelocity * predictedTime;

        return predictedPosition;
    }

    // Utilizamos el metodo OnDrawGizmos para poder visualizar el rango y angulo de vision de nuestro objeto
    private void OnDrawGizmos()
    {
        // Si nuestro anugulo de vision es igual o menor a 0 no se dibuja nada
        if (VisionAngle <= 0f) return;

        // Dividimos el angulo de vision a la mitad para facilitar la deteccion
        float HalfVisionAngle = VisionAngle * 0.5f;

        // p1 = primera mitad del angulo, p2 = segunda mitad del angulo
        Vector2 p1, p2;

        // Establecemos quien es la mitad "positiva" y quien es la mitad "negativa" en cuestion de las mitades del angulo
        p1 = PointForAngle(HalfVisionAngle, VisionDistance);
        p2 = PointForAngle(-HalfVisionAngle, VisionDistance);

        // Establecemos que el color cuando es detectado sea rojo y cuando no lo este sea de color verde
        Gizmos.color = detected ? Color.red : Color.green;

        // Dibujamos el limite de hasta donde llega el angulo de deteccion tanto de la mitad superior o primera mitad asi como de la mitad inferior
        // o segunda mitad
        Gizmos.DrawLine(VisionObject.position, (Vector2)VisionObject.position + p1);
        Gizmos.DrawLine(VisionObject.position, (Vector2)VisionObject.position + p2);

        // Desde donde esta nuestro objeto tiramos un rayo en su direccion de la derecha con la finalidad de saber para donde esta apuntando
        Gizmos.DrawRay(VisionObject.position, VisionObject.right * 4f);
    }

    // Fuentes:
    // https://www.youtube.com/watch?v=lV47ED8h61k (Video anexado a la tarea en el classroom por el profesor)
    // Cambiar de color el objeto ->https://www.youtube.com/watch?v=-_2rMgCqEk0 (Basado en el minuto 5:00 de este video)
    // Modificar el switch-case mediante una variable -> https://www.youtube.com/watch?v=Cywj2rx2AMc (El minuto 3:52 me dio la idea)
}