using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline;
using UnityEngine;

<<<<<<< HEAD
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
    // Ademas añadimos un rango de giro entre 0 y 360 gradfos que representan una vuelta completa
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
            // ... Si la magnitud/tamaño del vector entre el agente y el VisionObject es menor a la distancia de vision del VisionObject 
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
=======
public class GuardScript : MonoBehaviour
{
    public Transform player; // Referencia al infiltrador
    public Collider capsuleC;
    public GameObject player1;
    public float visionAngle = 45f; // Ángulo de visión del guardia
    public float visionRadius = 10f; // Radio de visión del guardia
    public float rotationInterval = 5f; // Intervalo de rotación del guardia
    public float alertDuration = 3f; // Duración del estado de alerta
    public float attackDuration = 5f; // Duración del estado de ataque
    public float pursuitSpeed = 5f; // Velocidad de persecución del guardia
    public Transform initialPosition; // Posición inicial del guardia
    public float attackRange = 0.1f;
    private float timeSinceLastRotation = 0f;
    private bool isAlerted = false;
    private bool isAttacking = false;
    private Vector3 lastPlayerPosition;
    public Color normalColor = Color.yellow;
    public Color alertedColor = Color.red;
    public Color attackingColor = Color.magenta;

    void Start()
    {
        lastPlayerPosition = initialPosition.position;
    }

    void Update()
    {
        if (!isAlerted && !isAttacking)
        {
            RotateGuard();
            CheckForPlayer();
        }
        else if (isAlerted)
        {
            AlertState();
        }
        else if (isAttacking)
        {
            AttackState();
        }
    }

    void RotateGuard()
    {
        timeSinceLastRotation += Time.deltaTime;
        if (timeSinceLastRotation >= rotationInterval)
        {
            // Rotar el guardia en un ángulo aleatorio
            transform.Rotate(Vector3.up, Random.Range(45f, 90f));
            timeSinceLastRotation = 0f;
        }
    }

    void CheckForPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer <= visionAngle / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, visionRadius))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    isAlerted = true;
                    lastPlayerPosition = player.position;
                }
            }
        }
    }

    void AlertState()
    {
        // Ampliar ligeramente el cono de visión
        visionAngle = 90f; // Ángulo ligeramente más amplio que el normal

        // Calcular la dirección y distancia al jugador
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Moverse hacia la última posición conocida del jugador con Arrive
        Vector3 moveDirection = (lastPlayerPosition - transform.position).normalized;
        float distanceToLastSeen = Vector3.Distance(transform.position, lastPlayerPosition);
        float desiredSpeed = Mathf.Min(distanceToLastSeen / 2f, pursuitSpeed); // Ajustar la velocidad deseada para el Arrive
        Vector3 desiredVelocity = moveDirection * desiredSpeed;
        Vector3 steering = desiredVelocity - GetComponent<Rigidbody>().velocity;
        Vector3 acceleration = Vector3.ClampMagnitude(steering, pursuitSpeed);
        GetComponent<Rigidbody>().velocity += acceleration * Time.deltaTime;

        // Rotar hacia la última posición conocida del jugador
        Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * pursuitSpeed);

        // Verificar si el jugador es visible
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, visionRadius))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Cambiar al estado de ataque si el jugador es visible
                isAlerted = false;
                isAttacking = true;
                visionAngle = 45f; // Restablecer el ángulo de visión
                return;
            }
        }

        // Si el guardia ve la última posición conocida, mantenerse alerta
        if (Vector3.Angle(transform.forward, directionToPlayer) < visionAngle / 2f && distanceToLastSeen <= visionRadius)
        {
            // Temporizador para volver al estado normal si el jugador no es detectado
            alertDuration = 3f; // Reiniciar el temporizador
            return;
        }

        // Temporizador para volver al estado normal si el jugador no es detectado
        if ((alertDuration -= Time.deltaTime) <= 0)
        {
            // Si el jugador no es detectado después de volver a la última posición conocida, regresar a la posición inicial
            ReturnToInitialPosition();
>>>>>>> parent of 45c10da (Primera parte modificaciones)
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

<<<<<<< HEAD
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
=======



    void AttackState()
    {
        
        // Calculate direction to the player
        Vector3 directionToPlayer = player.position - transform.position;

        // Move towards the player
        transform.position += directionToPlayer.normalized * pursuitSpeed * Time.deltaTime;
        visionAngle = 120;
        // Rotate towards the player
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * pursuitSpeed);

        // Check if the player is within attack range
        float distanceToPlayer = directionToPlayer.magnitude;
        // If attack duration has passed, return to normal state
        if ((attackDuration -= Time.deltaTime) <= 0)
        {
            isAttacking = false;
            attackDuration = 5;
            ReturnToInitialPosition();
            // Optionally: Implement behavior to return to normal state or continue patrolling
>>>>>>> parent of 45c10da (Primera parte modificaciones)
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

<<<<<<< HEAD
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
=======
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Destroy(player1);
>>>>>>> parent of 45c10da (Primera parte modificaciones)
        }
        steeringForce = Vector3.Min(steeringForce, steeringForce.normalized * maxSteeringForce);

        rb.AddForce(steeringForce, ForceMode.Acceleration);
    }

<<<<<<< HEAD
    //Funcion de arrive del profe
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
=======

    void ReturnToInitialPosition()
    {
        // Implementar el comportamiento de Arrive para volver a la posición inicial
        Vector3 directionToInitialPosition = initialPosition.position - transform.position;
        float distanceToInitialPosition = directionToInitialPosition.magnitude;

        if (distanceToInitialPosition > 1f) // Threshold to stop close to the position, e.g., 1 meter
        {
            // Move towards the initial position
            Vector3 moveDirection = directionToInitialPosition.normalized;
            transform.position += moveDirection * pursuitSpeed * Time.deltaTime;

            // Rotate towards the initial position
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * pursuitSpeed);
        }
        else
        {
            // If reached initial position, return to normal state
            isAttacking = false;
            isAlerted = false;
>>>>>>> parent of 45c10da (Primera parte modificaciones)
        }
        // sino, que vaya lo más rápido que pueda.
        return 1.0f;
    }

<<<<<<< HEAD
    //Funcion de GetSteeringForce del profe
    private Vector3 GetSteeringForce(Vector3 DistanceVector)
=======

    void OnDrawGizmosSelected()
>>>>>>> parent of 45c10da (Primera parte modificaciones)
    {
        Vector3 desiredDirection = DistanceVector.normalized;  // queremos la dirección de ese vector, pero de magnitud 1.

<<<<<<< HEAD
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
            // Reiniciar la posición del agente a la posición inicial
            Agent.position = PosicionInicialAgente;
        }
=======
        Vector3 visionDirection = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + visionDirection * visionRadius);
        visionDirection = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + visionDirection * visionRadius);
>>>>>>> parent of 45c10da (Primera parte modificaciones)
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
        // Pursuit no es mucho más que hacerle Seek a la posición futura del objetivo.
        // Primero calculamos el tiempo T que nos tomaría llegar al TargetPosition.
        Vector3 Distance = transform.position - TargetPosition;
        // con esa distancia, podemos saber cuánto tiempo nos tomará recorrer esa 
        // distancia usando nuestra máxima velocidad.
        // TiempoT = Distancia/MaxSpeed
        float predictedTime = Distance.magnitude / maxSpeed;
        // usamos Distance.magnitude porque queremos cuánto mide el vector, no hacia dónde (o no hacia qué dirección).
        // Ahora sí podemos predecir la posición futura de nuestro TargetObject.
        // Su posición futura es: Su posición actual + su velocidad * cuánto tiempo transcurre.
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