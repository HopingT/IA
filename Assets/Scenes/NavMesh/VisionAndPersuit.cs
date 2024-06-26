using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent))]
public class VisionAndPursuit : MonoBehaviour
{
    public Transform target;
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;
    private Vector3 startPosition;
    private NavMeshAgent agent;
    public GameObject Player;
    public Transform spawnPoint;
    public Animator ani;
    private bool isGuarding = false; // Variable para verificar si est� en su posici�n de guardia

    void Start()
    {
        startPosition = transform.position;
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        DetectTarget();

        // Si est� en su posici�n original, activa la rotaci�n
        if (Vector3.Distance(transform.position, startPosition) < 0.5f)
        {
            Debug.Log("Est� cerca de la posici�n original");
            isGuarding = true;
            RotateOnAxis();
            ani.SetBool("Run", false);

        }
        else
        {
            isGuarding = false;
            ani.SetBool("Run", true);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnTarget();
        }
    }

    void DetectTarget()
    {
        if (target == null) return;

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
        {
            float dstToTarget = Vector3.Distance(transform.position, target.position);
            if (dstToTarget <= viewRadius)
            {
                StartCoroutine(FollowTarget(3));
            }
        }
    }

    IEnumerator FollowTarget(float duration)
    {
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
            yield return null;
        }

        agent.SetDestination(startPosition);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Verifica si el objeto colisionado es el objetivo
        if (collision.gameObject.CompareTag("Target"))
        {
            // Desactiva el objeto colisionado
            collision.gameObject.SetActive(false);
        }
    }


    public void RespawnTarget()
    {
        if (GameObject.FindGameObjectWithTag("Target") == null)
        {
            Player.SetActive(true);
            Player.transform.position = spawnPoint.position;
        }
    }

    // Rotar sobre su eje si est� en su posici�n original de guardia
    void RotateOnAxis()
    {
        if (isGuarding)
        {
            Debug.Log("Rotando sobre su eje");
            transform.Rotate(Vector3.up * Time.deltaTime * 30); // Modifica el valor de rotaci�n seg�n necesites
        }
    }

    // Visualizaci�n del cono de visi�n
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}