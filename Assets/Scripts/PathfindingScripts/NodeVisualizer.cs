using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeVisualizer : MonoBehaviour
{
    //Les asignamos una posicion en x y en y 
    public void SetPosition(float x, float y)
    {

        transform.position = new Vector3(x, y, 0f);
    }
}