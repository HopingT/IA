using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerNavMesh : MonoBehaviour
{
    [SerializeField] private Transform movePositionTransforn;
    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (movePositionTransforn != null) // Asegúrate de que hay un objetivo
        {
            navMeshAgent.destination = movePositionTransforn.position;
        }
    }

    // Método público para actualizar el objetivo
    public void UpdateTarget(Transform newTarget)
    {
        movePositionTransforn = newTarget;
    }
}
