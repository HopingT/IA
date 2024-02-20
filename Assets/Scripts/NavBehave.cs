using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; 

public class NavBehave : MonoBehaviour
{
   public NavMeshAgent agent;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            Ray moveposition = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(moveposition, out var hitInfo))
            {
                agent.SetDestination(hitInfo.point);
            }
        }
    }
}
